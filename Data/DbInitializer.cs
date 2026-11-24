using System.Data;
using Microsoft.Data.SqlClient;
using UniversityExamSystem.Services;

namespace UniversityExamSystem.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(DatabaseHelper db, IAuthService authService)
        {
            try
            {
                // Ensure default passwords match BCrypt verification
                var usersToCheck = new[]
                {
                    ("admin@puexam.edu.np", "Admin@123"),
                    ("controller@puexam.edu.np", "Admin@123"),
                    ("college@puexam.edu.np", "College@123"),
                    ("examiner@puexam.edu.np", "Examiner@123"),
                    ("student@puexam.edu.np", "Student@123")
                };

                foreach (var (email, rawPassword) in usersToCheck)
                {
                    var sqlSelect = "SELECT PasswordHash FROM dbo.Users WHERE Email = @Email";
                    var currentHash = (string?)await db.ExecuteScalarAsync(sqlSelect, CommandType.Text, new[]
                    {
                        new SqlParameter("@Email", email)
                    });

                    if (currentHash != null && !authService.VerifyPassword(rawPassword, currentHash))
                    {
                        var newHash = authService.HashPassword(rawPassword);
                        var sqlUpdate = "UPDATE dbo.Users SET PasswordHash = @Hash WHERE Email = @Email";
                        await db.ExecuteNonQueryAsync(sqlUpdate, CommandType.Text, new[]
                        {
                            new SqlParameter("@Hash", newHash),
                            new SqlParameter("@Email", email)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DbInitializer Notice: {ex.Message}");
            }
        }
    }
}
