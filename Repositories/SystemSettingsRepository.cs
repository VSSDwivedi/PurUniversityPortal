using System.Data;
using Microsoft.Data.SqlClient;
using UniversityExamSystem.Data;
using UniversityExamSystem.Models;

namespace UniversityExamSystem.Repositories
{
    public interface ISystemSettingsRepository
    {
        Task<Dictionary<string, string>> GetAllSettingsAsync();
        Task<string> GetSettingAsync(string key, string defaultValue = "");
        Task<bool> UpdateSettingAsync(string key, string value);
        Task<AdminDashboardViewModel> GetDashboardStatsAsync();
        Task LogAuditAsync(int? userId, string action, string tableName, string? recordId, string? oldValues, string? newValues, string? ipAddress);
    }

    public class SystemSettingsRepository : ISystemSettingsRepository
    {
        private readonly DatabaseHelper _db;

        public SystemSettingsRepository(DatabaseHelper db)
        {
            _db = db;
        }

        public async Task<Dictionary<string, string>> GetAllSettingsAsync()
        {
            const string sql = "SELECT SettingKey, SettingValue FROM dbo.SystemSettings";
            var list = await _db.ExecuteReaderListAsync(sql, r => new
            {
                Key = r.GetString(r.GetOrdinal("SettingKey")),
                Value = r.GetString(r.GetOrdinal("SettingValue"))
            });

            return list.ToDictionary(x => x.Key, x => x.Value);
        }

        public async Task<string> GetSettingAsync(string key, string defaultValue = "")
        {
            const string sql = "SELECT TOP 1 SettingValue FROM dbo.SystemSettings WHERE SettingKey = @Key";
            var result = await _db.ExecuteScalarAsync(sql, CommandType.Text, new[] { new SqlParameter("@Key", key) });
            return result != null && result != DBNull.Value ? (string)result : defaultValue;
        }

        public async Task<bool> UpdateSettingAsync(string key, string value)
        {
            const string sql = @"
                IF EXISTS (SELECT 1 FROM dbo.SystemSettings WHERE SettingKey = @Key)
                BEGIN
                    UPDATE dbo.SystemSettings SET SettingValue = @Value, UpdatedDate = GETDATE() WHERE SettingKey = @Key;
                END
                ELSE
                BEGIN
                    INSERT INTO dbo.SystemSettings (SettingKey, SettingValue, SettingGroup) VALUES (@Key, @Value, 'General');
                END";

            var rows = await _db.ExecuteNonQueryAsync(sql, CommandType.Text, new[]
            {
                new SqlParameter("@Key", key),
                new SqlParameter("@Value", value)
            });

            return rows > 0;
        }

        public async Task<AdminDashboardViewModel> GetDashboardStatsAsync()
        {
            var vm = await _db.ExecuteReaderSingleAsync(
                "dbo.sp_GetAdminDashboardStats",
                r => new AdminDashboardViewModel
                {
                    TotalStudents = r.GetInt32(r.GetOrdinal("TotalStudents")),
                    RegisteredStudents = r.GetInt32(r.GetOrdinal("RegisteredStudents")),
                    PendingRegistrations = r.GetInt32(r.GetOrdinal("PendingRegistrations")),
                    TotalColleges = r.GetInt32(r.GetOrdinal("TotalColleges")),
                    TotalFaculties = r.GetInt32(r.GetOrdinal("TotalFaculties")),
                    TotalPrograms = r.GetInt32(r.GetOrdinal("TotalPrograms")),
                    TotalSubjects = r.GetInt32(r.GetOrdinal("TotalSubjects")),
                    TotalExaminations = r.GetInt32(r.GetOrdinal("TotalExaminations")),
                    TotalApplications = r.GetInt32(r.GetOrdinal("TotalApplications")),
                    ApprovedApplications = r.GetInt32(r.GetOrdinal("ApprovedApplications")),
                    AdmitCardsGenerated = r.GetInt32(r.GetOrdinal("AdmitCardsGenerated")),
                    ResultsPublished = r.GetInt32(r.GetOrdinal("ResultsPublished"))
                },
                CommandType.StoredProcedure
            ) ?? new AdminDashboardViewModel();

            // Populate College chart data
            const string sqlColleges = @"
                SELECT c.CollegeName, COUNT(s.StudentId) AS StudentCount
                FROM dbo.Colleges c
                LEFT JOIN dbo.Students s ON c.CollegeId = s.CollegeId AND s.IsActive = 1
                WHERE c.IsActive = 1
                GROUP BY c.CollegeName";

            var collegeStats = await _db.ExecuteReaderListAsync(sqlColleges, r => new
            {
                Name = r.GetString(r.GetOrdinal("CollegeName")),
                Count = r.GetInt32(r.GetOrdinal("StudentCount"))
            });

            vm.CollegeNames = collegeStats.Select(x => x.Name).ToList();
            vm.CollegeStudentCounts = collegeStats.Select(x => x.Count).ToList();

            // Populate Program chart data
            const string sqlPrograms = @"
                SELECT p.ProgramCode, COUNT(s.StudentId) AS StudentCount
                FROM dbo.Programs p
                LEFT JOIN dbo.Students s ON p.ProgramId = s.ProgramId AND s.IsActive = 1
                WHERE p.IsActive = 1
                GROUP BY p.ProgramCode";

            var progStats = await _db.ExecuteReaderListAsync(sqlPrograms, r => new
            {
                Code = r.GetString(r.GetOrdinal("ProgramCode")),
                Count = r.GetInt32(r.GetOrdinal("StudentCount"))
            });

            vm.ProgramNames = progStats.Select(x => x.Code).ToList();
            vm.ProgramStudentCounts = progStats.Select(x => x.Count).ToList();

            return vm;
        }

        public async Task LogAuditAsync(int? userId, string action, string tableName, string? recordId, string? oldValues, string? newValues, string? ipAddress)
        {
            try
            {
                const string sql = @"
                    INSERT INTO dbo.AuditLogs (UserId, Action, TableName, RecordId, OldValues, NewValues, IpAddress, Timestamp)
                    VALUES (@UserId, @Action, @Table, @Record, @Old, @New, @Ip, GETDATE());";

                await _db.ExecuteNonQueryAsync(sql, CommandType.Text, new[]
                {
                    new SqlParameter("@UserId", (object?)userId ?? DBNull.Value),
                    new SqlParameter("@Action", action),
                    new SqlParameter("@Table", tableName),
                    new SqlParameter("@Record", (object?)recordId ?? DBNull.Value),
                    new SqlParameter("@Old", (object?)oldValues ?? DBNull.Value),
                    new SqlParameter("@New", (object?)newValues ?? DBNull.Value),
                    new SqlParameter("@Ip", (object?)ipAddress ?? DBNull.Value)
                });
            }
            catch
            {
                // Non-blocking audit log
            }
        }
    }
}
