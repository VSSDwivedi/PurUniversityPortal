using System.Data;
using Microsoft.Data.SqlClient;
using UniversityExamSystem.Data;
using UniversityExamSystem.Models;

namespace UniversityExamSystem.Repositories
{
    public interface IUserRepository
    {
        Task<User?> AuthenticateAsync(string loginIdentifier);
        Task<User?> GetByIdAsync(int userId);
        Task<List<User>> GetAllUsersAsync();
        Task<int> CreateUserAsync(User user, string roleName);
        Task<bool> UpdateUserStatusAsync(int userId, bool isActive);
        Task<bool> UpdatePasswordAsync(int userId, string newHash);
        Task UpdateLastLoginAsync(int userId);
    }

    public class UserRepository : IUserRepository
    {
        private readonly DatabaseHelper _db;

        public UserRepository(DatabaseHelper db)
        {
            _db = db;
        }

        public async Task<User?> AuthenticateAsync(string loginIdentifier)
        {
            var parameters = new[]
            {
                new SqlParameter("@LoginIdentifier", loginIdentifier.Trim())
            };

            return await _db.ExecuteReaderSingleAsync(
                "dbo.sp_AuthenticateUser",
                reader => new User
                {
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                    Username = reader.GetString(reader.GetOrdinal("Username")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                    FullName = reader.GetString(reader.GetOrdinal("FullName")),
                    PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                    UserType = reader.GetString(reader.GetOrdinal("UserType")),
                    ReferenceId = reader.IsDBNull(reader.GetOrdinal("ReferenceId")) ? null : reader.GetInt32(reader.GetOrdinal("ReferenceId")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                    RoleName = reader.IsDBNull(reader.GetOrdinal("RoleName")) ? reader.GetString(reader.GetOrdinal("UserType")) : reader.GetString(reader.GetOrdinal("RoleName")),
                    CollegeName = reader.IsDBNull(reader.GetOrdinal("CollegeName")) ? null : reader.GetString(reader.GetOrdinal("CollegeName")),
                    ProgramName = reader.IsDBNull(reader.GetOrdinal("ProgramName")) ? null : reader.GetString(reader.GetOrdinal("ProgramName")),
                    RegistrationNumber = reader.IsDBNull(reader.GetOrdinal("RegistrationNumber")) ? null : reader.GetString(reader.GetOrdinal("RegistrationNumber")),
                    StudentUniqueId = reader.IsDBNull(reader.GetOrdinal("StudentUniqueId")) ? null : reader.GetString(reader.GetOrdinal("StudentUniqueId"))
                },
                CommandType.StoredProcedure,
                parameters
            );
        }

        public async Task<User?> GetByIdAsync(int userId)
        {
            const string sql = @"
                SELECT u.UserId, u.Username, u.Email, u.PasswordHash, u.FullName, u.PhoneNumber, 
                       u.UserType, u.ReferenceId, u.IsActive, u.LastLoginDate, u.CreatedDate,
                       r.RoleName
                FROM dbo.Users u
                LEFT JOIN dbo.UserRoles ur ON u.UserId = ur.UserId
                LEFT JOIN dbo.Roles r ON ur.RoleId = r.RoleId
                WHERE u.UserId = @UserId";

            return await _db.ExecuteReaderSingleAsync(
                sql,
                reader => new User
                {
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                    Username = reader.GetString(reader.GetOrdinal("Username")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                    FullName = reader.GetString(reader.GetOrdinal("FullName")),
                    PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                    UserType = reader.GetString(reader.GetOrdinal("UserType")),
                    ReferenceId = reader.IsDBNull(reader.GetOrdinal("ReferenceId")) ? null : reader.GetInt32(reader.GetOrdinal("ReferenceId")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                    LastLoginDate = reader.IsDBNull(reader.GetOrdinal("LastLoginDate")) ? null : reader.GetDateTime(reader.GetOrdinal("LastLoginDate")),
                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                    RoleName = reader.IsDBNull(reader.GetOrdinal("RoleName")) ? reader.GetString(reader.GetOrdinal("UserType")) : reader.GetString(reader.GetOrdinal("RoleName"))
                },
                CommandType.Text,
                new[] { new SqlParameter("@UserId", userId) }
            );
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            const string sql = @"
                SELECT u.UserId, u.Username, u.Email, u.PasswordHash, u.FullName, u.PhoneNumber, 
                       u.UserType, u.ReferenceId, u.IsActive, u.LastLoginDate, u.CreatedDate,
                       r.RoleName
                FROM dbo.Users u
                LEFT JOIN dbo.UserRoles ur ON u.UserId = ur.UserId
                LEFT JOIN dbo.Roles r ON ur.RoleId = r.RoleId
                ORDER BY u.CreatedDate DESC";

            return await _db.ExecuteReaderListAsync(
                sql,
                reader => new User
                {
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                    Username = reader.GetString(reader.GetOrdinal("Username")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                    FullName = reader.GetString(reader.GetOrdinal("FullName")),
                    PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                    UserType = reader.GetString(reader.GetOrdinal("UserType")),
                    ReferenceId = reader.IsDBNull(reader.GetOrdinal("ReferenceId")) ? null : reader.GetInt32(reader.GetOrdinal("ReferenceId")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                    LastLoginDate = reader.IsDBNull(reader.GetOrdinal("LastLoginDate")) ? null : reader.GetDateTime(reader.GetOrdinal("LastLoginDate")),
                    CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                    RoleName = reader.IsDBNull(reader.GetOrdinal("RoleName")) ? reader.GetString(reader.GetOrdinal("UserType")) : reader.GetString(reader.GetOrdinal("RoleName"))
                }
            );
        }

        public async Task<int> CreateUserAsync(User user, string roleName)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();
            try
            {
                const string sqlUser = @"
                    INSERT INTO dbo.Users (Username, Email, PasswordHash, FullName, PhoneNumber, UserType, ReferenceId, IsActive, CreatedDate)
                    VALUES (@Username, @Email, @PasswordHash, @FullName, @PhoneNumber, @UserType, @ReferenceId, @IsActive, GETDATE());
                    SELECT SCOPE_IDENTITY();";

                using var cmdUser = new SqlCommand(sqlUser, conn, tx);
                cmdUser.Parameters.AddWithValue("@Username", user.Username);
                cmdUser.Parameters.AddWithValue("@Email", user.Email);
                cmdUser.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                cmdUser.Parameters.AddWithValue("@FullName", user.FullName);
                cmdUser.Parameters.AddWithValue("@PhoneNumber", (object?)user.PhoneNumber ?? DBNull.Value);
                cmdUser.Parameters.AddWithValue("@UserType", user.UserType);
                cmdUser.Parameters.AddWithValue("@ReferenceId", (object?)user.ReferenceId ?? DBNull.Value);
                cmdUser.Parameters.AddWithValue("@IsActive", user.IsActive);

                var newUserId = Convert.ToInt32(await cmdUser.ExecuteScalarAsync());

                // Assign role
                const string sqlRole = "SELECT RoleId FROM dbo.Roles WHERE RoleName = @RoleName";
                using var cmdRole = new SqlCommand(sqlRole, conn, tx);
                cmdRole.Parameters.AddWithValue("@RoleName", roleName);
                var roleIdObj = await cmdRole.ExecuteScalarAsync();

                if (roleIdObj != null)
                {
                    const string sqlAssign = "INSERT INTO dbo.UserRoles (UserId, RoleId) VALUES (@UserId, @RoleId)";
                    using var cmdAssign = new SqlCommand(sqlAssign, conn, tx);
                    cmdAssign.Parameters.AddWithValue("@UserId", newUserId);
                    cmdAssign.Parameters.AddWithValue("@RoleId", Convert.ToInt32(roleIdObj));
                    await cmdAssign.ExecuteNonQueryAsync();
                }

                tx.Commit();
                return newUserId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<bool> UpdateUserStatusAsync(int userId, bool isActive)
        {
            const string sql = "UPDATE dbo.Users SET IsActive = @IsActive, UpdatedDate = GETDATE() WHERE UserId = @UserId";
            var rows = await _db.ExecuteNonQueryAsync(sql, CommandType.Text, new[]
            {
                new SqlParameter("@IsActive", isActive),
                new SqlParameter("@UserId", userId)
            });
            return rows > 0;
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string newHash)
        {
            const string sql = "UPDATE dbo.Users SET PasswordHash = @Hash, UpdatedDate = GETDATE() WHERE UserId = @UserId";
            var rows = await _db.ExecuteNonQueryAsync(sql, CommandType.Text, new[]
            {
                new SqlParameter("@Hash", newHash),
                new SqlParameter("@UserId", userId)
            });
            return rows > 0;
        }

        public async Task UpdateLastLoginAsync(int userId)
        {
            const string sql = "UPDATE dbo.Users SET LastLoginDate = GETDATE() WHERE UserId = @UserId";
            await _db.ExecuteNonQueryAsync(sql, CommandType.Text, new[]
            {
                new SqlParameter("@UserId", userId)
            });
        }
    }
}
