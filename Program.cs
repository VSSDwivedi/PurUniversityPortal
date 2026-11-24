using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using UniversityExamSystem.Data;
using UniversityExamSystem.Repositories;
using UniversityExamSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Persist Data Protection keys to prevent CryptographicException across restarts / debug sessions
var keysDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtectionKeys");
if (!Directory.Exists(keysDirectory))
{
    Directory.CreateDirectory(keysDirectory);
}

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysDirectory))
    .SetApplicationName("PurbanchalUniversityExamSystem");

// Session Configuration with dedicated cookie name
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "PurbanchalUniversity.Session";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "PurbanchalUniversity.AuthCookie";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Super Admin", "Admin", "SuperAdmin"));
    options.AddPolicy("RequireControllerRole", policy => policy.RequireRole("Super Admin", "Admin", "Examination Controller", "Controller"));
    options.AddPolicy("RequireCollegeRole", policy => policy.RequireRole("College Admin", "CollegeAdmin", "Super Admin", "Admin"));
    options.AddPolicy("RequireStudentRole", policy => policy.RequireRole("Student"));
    options.AddPolicy("RequireExaminerRole", policy => policy.RequireRole("Examiner", "Super Admin", "Admin", "Examination Controller"));
});

// Data & Core Services
builder.Services.AddScoped<DatabaseHelper>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();
builder.Services.AddScoped<IFileService, FileService>();

// ADO.NET Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAcademicRepository, AcademicRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IExaminationRepository, ExaminationRepository>();
builder.Services.AddScoped<IMarksRepository, MarksRepository>();
builder.Services.AddScoped<IResultRepository, ResultRepository>();
builder.Services.AddScoped<INoticeRepository, NoticeRepository>();
builder.Services.AddScoped<ISystemSettingsRepository, SystemSettingsRepository>();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Seed/Verify initial passwords
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DatabaseHelper>();
    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
    await DbInitializer.InitializeAsync(db, authService);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Pre-session sanitization middleware: silently cleans up stale/invalid session cookies
// from previous debug sessions, wiped data protection keys, or localhost collisions
app.Use(async (context, next) =>
{
    const string sessionCookieName = "PurbanchalUniversity.Session";

    // Clean up generic .AspNetCore.Session cookie if sent by browser from other projects
    if (context.Request.Cookies.ContainsKey(".AspNetCore.Session"))
    {
        context.Response.Cookies.Delete(".AspNetCore.Session");
    }

    if (context.Request.Cookies.TryGetValue(sessionCookieName, out var cookieVal) && !string.IsNullOrWhiteSpace(cookieVal))
    {
        bool isValid = false;
        try
        {
            var dp = context.RequestServices.GetService<IDataProtectionProvider>();
            if (dp != null)
            {
                var protector = dp.CreateProtector("Microsoft.AspNetCore.Session.SessionMiddleware");
                var raw = Convert.FromBase64String(cookieVal);
                protector.Unprotect(raw);
                isValid = true;
            }
        }
        catch
        {
            isValid = false;
        }

        if (!isValid)
        {
            // Delete corrupt/stale cookie so browser doesn't send it again
            context.Response.Cookies.Delete(sessionCookieName);

            // Strip from incoming Cookie header so SessionMiddleware never tries to decrypt bad ciphertext
            var rawCookie = context.Request.Headers.Cookie.ToString();
            if (!string.IsNullOrEmpty(rawCookie))
            {
                var filtered = rawCookie
                    .Split(';')
                    .Select(c => c.Trim())
                    .Where(c => !c.StartsWith(sessionCookieName + "=", StringComparison.OrdinalIgnoreCase)
                             && !c.StartsWith(".AspNetCore.Session=", StringComparison.OrdinalIgnoreCase));
                context.Request.Headers.Cookie = string.Join("; ", filtered);
            }
        }
    }

    await next();
});

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
