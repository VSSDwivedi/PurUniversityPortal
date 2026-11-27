using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityExamSystem.Models;
using UniversityExamSystem.Repositories;
using UniversityExamSystem.Services;

namespace UniversityExamSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IStudentRepository _studentRepo;
        private readonly IAcademicRepository _academicRepo;
        private readonly IAuthService _authService;
        private readonly IFileService _fileService;

        public AccountController(
            IUserRepository userRepo,
            IStudentRepository studentRepo,
            IAcademicRepository academicRepo,
            IAuthService authService,
            IFileService fileService)
        {
            _userRepo = userRepo;
            _studentRepo = studentRepo;
            _academicRepo = academicRepo;
            _authService = authService;
            _fileService = fileService;
        }

        [HttpGet]
        [Route("Account/Login")]
        [Route("college-login-new")]
        [Route("college-login")]
        [Route("studentlogin")]
        public IActionResult Login(string? returnUrl = null, string? role = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectByUserRole();
            }

            ViewBag.ReturnUrl = returnUrl;
            ViewBag.ActiveRole = role;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [Route("Account/Login")]
        [Route("college-login-new")]
        [Route("college-login")]
        [Route("college-auth-new")]
        [Route("studentlogin/authenticate")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, [FromForm] string? loginId, [FromForm] string? loginPwd)
        {
            if (string.IsNullOrEmpty(model.LoginIdentifier) && !string.IsNullOrEmpty(loginId))
            {
                model.LoginIdentifier = loginId;
            }
            if (string.IsNullOrEmpty(model.Password) && !string.IsNullOrEmpty(loginPwd))
            {
                model.Password = loginPwd;
            }
            //if (!ModelState.IsValid)
            //{
            //    return View(model);
            //}

            var user = await _userRepo.AuthenticateAsync(model.LoginIdentifier);

            if (user == null || !_authService.VerifyPassword(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Invalid login credentials. Please check your Student ID / Registration No / Email and password.");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Your account has been deactivated. Please contact the Examination Controller Office.");
                return View(model);
            }

            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.RoleName),
                new Claim("UserType", user.UserType),
                new Claim("Username", user.Username)
            };

            if (user.ReferenceId.HasValue)
            {
                claims.Add(new Claim("ReferenceId", user.ReferenceId.Value.ToString()));
            }
            if (!string.IsNullOrEmpty(user.RegistrationNumber))
            {
                claims.Add(new Claim("RegistrationNumber", user.RegistrationNumber));
            }
            if (!string.IsNullOrEmpty(user.StudentUniqueId))
            {
                claims.Add(new Claim("StudentUniqueId", user.StudentUniqueId));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProps = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(model.RememberMe ? 72 : 8)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);
            await _userRepo.UpdateLastLoginAsync(user.UserId);

            // Store in session
            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            HttpContext.Session.SetString("FullName", user.FullName);
            HttpContext.Session.SetString("Role", user.RoleName);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectByUserRole(user.RoleName, user.UserType);
        }

        [HttpGet]
        [Route("Account/Register")]
        [Route("studentlogin/register")]
        [Route("examregistration")]
        public async Task<IActionResult> Register()
        {
            await PopulateRegistrationDropdowns();
            return View(new StudentRegistrationViewModel
            {
                DateOfBirth = DateTime.Today.AddYears(-19)
            });
        }

        [HttpPost]
        [Route("Account/Register")]
        [Route("studentlogin/register")]
        [Route("examregistration")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(StudentRegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateRegistrationDropdowns();
                return View(model);
            }

            try
            {
                string? photoUrl = null;
                string? docUrl = null;

                if (model.PhotoFile != null)
                {
                    photoUrl = await _fileService.SaveFileAsync(model.PhotoFile, "students");
                }
                if (model.IdentityDocumentFile != null)
                {
                    docUrl = await _fileService.SaveFileAsync(model.IdentityDocumentFile, "documents");
                }

                var student = new Student
                {
                    FullName = model.FullName.Trim(),
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender,
                    Email = model.Email.Trim().ToLowerInvariant(),
                    MobileNumber = model.MobileNumber.Trim(),
                    Address = model.Address.Trim(),
                    FacultyId = model.FacultyId,
                    ProgramId = model.ProgramId,
                    CollegeId = model.CollegeId,
                    AcademicYearId = model.AcademicYearId,
                    CurrentSemesterId = model.CurrentSemesterId,
                    PreviousQualification = model.PreviousQualification,
                    PhotoUrl = photoUrl,
                    IdentityDocumentUrl = docUrl
                };

                var passwordHash = _authService.HashPassword(model.Password);

                var (studentId, uniqueId, appNumber) = await _studentRepo.RegisterStudentAsync(student, passwordHash);

                // Add documents if uploaded
                if (!string.IsNullOrEmpty(docUrl))
                {
                    await _studentRepo.AddStudentDocumentAsync(new StudentDocument
                    {
                        StudentId = studentId,
                        DocumentType = "Identity Document",
                        DocumentName = model.IdentityDocumentFile!.FileName,
                        FilePath = docUrl,
                        FileSizeKB = model.IdentityDocumentFile.Length / 1024
                    });
                }

                TempData["RegistrationSuccess"] = true;
                TempData["StudentUniqueId"] = uniqueId;
                TempData["ApplicationNumber"] = appNumber;
                TempData["StudentName"] = student.FullName;

                return RedirectToAction(nameof(RegistrationConfirmation));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateRegistrationDropdowns();
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult RegistrationConfirmation()
        {
            if (TempData["StudentUniqueId"] == null)
            {
                return RedirectToAction(nameof(Login));
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> TrackRegistration(string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return View();
            }

            var student = await _studentRepo.TrackRegistrationAsync(query);
            ViewBag.Query = query;
            ViewBag.Searched = true;

            return View(student);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                TempData["ErrorMessage"] = "Current password and new password are required.";
                return RedirectToAction("Profile", "Student");
            }
            if (newPassword.Length < 8)
            {
                TempData["ErrorMessage"] = "The new password must contain at least 8 characters.";
                return RedirectToAction("Profile", "Student");
            }
            if (newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "New password and confirmation password do not match.";
                return RedirectToAction("Profile", "Student");
            }

            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId);
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null || !_authService.VerifyPassword(currentPassword, user.PasswordHash))
            {
                TempData["ErrorMessage"] = "The current password is incorrect.";
                return RedirectToAction("Profile", "Student");
            }

            await _userRepo.UpdatePasswordAsync(userId, _authService.HashPassword(newPassword));
            TempData["SuccessMessage"] = "Password changed successfully.";
            return RedirectToAction("Profile", "Student");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task PopulateRegistrationDropdowns()
        {
            ViewBag.Faculties = await _academicRepo.GetFacultiesAsync();
            ViewBag.Colleges = await _academicRepo.GetCollegesAsync();
            ViewBag.Programs = await _academicRepo.GetProgramsAsync();
            ViewBag.AcademicYears = await _academicRepo.GetAcademicYearsAsync();
            ViewBag.Semesters = await _academicRepo.GetSemestersAsync();
        }

        private IActionResult RedirectByUserRole(string? roleName = null, string? userType = null)
        {
            var role = roleName ?? User.FindFirstValue(ClaimTypes.Role) ?? "";
            var type = userType ?? User.FindFirstValue("UserType") ?? "";

            if (role == "Super Admin" || role == "Admin" || type == "SuperAdmin" || type == "Admin")
            {
                return RedirectToAction("Index", "Admin");
            }
            if (role == "Examination Controller" || type == "Controller")
            {
                return RedirectToAction("Index", "Admin");
            }
            if (role == "College Admin" || type == "CollegeAdmin")
            {
                return RedirectToAction("Index", "College");
            }
            if (role == "Examiner" || type == "Examiner")
            {
                return RedirectToAction("Index", "AdminMarks");
            }
            return RedirectToAction("Index", "Student");
        }
    }
}
