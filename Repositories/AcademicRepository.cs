using System.Data;
using Microsoft.Data.SqlClient;
using UniversityExamSystem.Data;
using UniversityExamSystem.Models;

namespace UniversityExamSystem.Repositories
{
    public interface IAcademicRepository
    {
        // Faculties
        Task<List<Faculty>> GetFacultiesAsync();
        Task<Faculty?> GetFacultyByIdAsync(int facultyId);
        Task<int> AddFacultyAsync(Faculty faculty);
        Task<bool> UpdateFacultyAsync(Faculty faculty);
        Task<bool> DeleteFacultyAsync(int facultyId);

        // Colleges
        Task<List<College>> GetCollegesAsync();
        Task<College?> GetCollegeByIdAsync(int collegeId);
        Task<int> AddCollegeAsync(College college);
        Task<bool> UpdateCollegeAsync(College college);
        Task<bool> DeleteCollegeAsync(int collegeId);

        // Programs
        Task<List<ProgramModel>> GetProgramsAsync();
        Task<List<ProgramModel>> GetProgramsByFacultyAsync(int facultyId);
        Task<ProgramModel?> GetProgramByIdAsync(int programId);
        Task<int> AddProgramAsync(ProgramModel program);
        Task<bool> UpdateProgramAsync(ProgramModel program);
        Task<bool> DeleteProgramAsync(int programId);

        // Subjects
        Task<List<Subject>> GetSubjectsAsync(int? programId = null, int? semesterId = null);
        Task<Subject?> GetSubjectByIdAsync(int subjectId);
        Task<int> AddSubjectAsync(Subject subject);
        Task<bool> UpdateSubjectAsync(Subject subject);
        Task<bool> DeleteSubjectAsync(int subjectId);

        // Academic Years & Semesters
        Task<List<AcademicYear>> GetAcademicYearsAsync();
        Task<AcademicYear?> GetCurrentAcademicYearAsync();
        Task<List<Semester>> GetSemestersAsync();
        Task<Semester?> GetSemesterByIdAsync(int semesterId);

        // Grade Scales
        Task<List<GradeScale>> GetGradeScalesAsync();
        Task<GradeScale?> GetGradeScaleByIdAsync(int gradeScaleId);
        Task<bool> UpdateGradeScaleAsync(GradeScale scale);
        Task<int> AddGradeScaleAsync(GradeScale scale);

        // Examination Centers
        Task<List<ExaminationCenter>> GetCentersAsync();
        Task<ExaminationCenter?> GetCenterByIdAsync(int centerId);
        Task<int> AddCenterAsync(ExaminationCenter center);
        Task<bool> UpdateCenterAsync(ExaminationCenter center);
    }

    public class AcademicRepository : IAcademicRepository
    {
        private readonly DatabaseHelper _db;

        public AcademicRepository(DatabaseHelper db)
        {
            _db = db;
        }

        // ======================== FACULTIES ========================
        public async Task<List<Faculty>> GetFacultiesAsync()
        {
            const string sql = @"
                SELECT f.FacultyId, f.FacultyCode, f.FacultyName, f.DeanName, f.Description, f.IsActive,
                       (SELECT COUNT(*) FROM dbo.Programs p WHERE p.FacultyId = f.FacultyId) AS ProgramCount
                FROM dbo.Faculties f
                ORDER BY f.FacultyName";

            return await _db.ExecuteReaderListAsync(sql, r => new Faculty
            {
                FacultyId = r.GetInt32(r.GetOrdinal("FacultyId")),
                FacultyCode = r.GetString(r.GetOrdinal("FacultyCode")),
                FacultyName = r.GetString(r.GetOrdinal("FacultyName")),
                DeanName = r.IsDBNull(r.GetOrdinal("DeanName")) ? null : r.GetString(r.GetOrdinal("DeanName")),
                Description = r.IsDBNull(r.GetOrdinal("Description")) ? null : r.GetString(r.GetOrdinal("Description")),
                IsActive = r.GetBoolean(r.GetOrdinal("IsActive")),
                ProgramCount = r.GetInt32(r.GetOrdinal("ProgramCount"))
            });
        }

        public async Task<Faculty?> GetFacultyByIdAsync(int facultyId)
        {
            const string sql = "SELECT FacultyId, FacultyCode, FacultyName, DeanName, Description, IsActive, 0 AS ProgramCount FROM dbo.Faculties WHERE FacultyId = @Id";
            return await _db.ExecuteReaderSingleAsync(sql, r => new Faculty
            {
                FacultyId = r.GetInt32(r.GetOrdinal("FacultyId")),
                FacultyCode = r.GetString(r.GetOrdinal("FacultyCode")),
                FacultyName = r.GetString(r.GetOrdinal("FacultyName")),
                DeanName = r.IsDBNull(r.GetOrdinal("DeanName")) ? null : r.GetString(r.GetOrdinal("DeanName")),
                Description = r.IsDBNull(r.GetOrdinal("Description")) ? null : r.GetString(r.GetOrdinal("Description")),
                IsActive = r.GetBoolean(r.GetOrdinal("IsActive"))
            }, CommandType.Text, new[] { new SqlParameter("@Id", facultyId) });
        }

        public async Task<int> AddFacultyAsync(Faculty faculty)
        {
            const string sql = @"
                INSERT INTO dbo.Faculties (FacultyCode, FacultyName, DeanName, Description, IsActive, CreatedDate)
                VALUES (@FacultyCode, @FacultyName, @DeanName, @Description, @IsActive, GETDATE());
                SELECT SCOPE_IDENTITY();";

            var result = await _db.ExecuteScalarAsync(sql, CommandType.Text, new[]
            {
                new SqlParameter("@FacultyCode", faculty.FacultyCode),
                new SqlParameter("@FacultyName", faculty.FacultyName),
                new SqlParameter("@DeanName", (object?)faculty.DeanName ?? DBNull.Value),
                new SqlParameter("@Description", (object?)faculty.Description ?? DBNull.Value),
                new SqlParameter("@IsActive", faculty.IsActive)
            });
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateFacultyAsync(Faculty faculty)
        {
            const string sql = @"
                UPDATE dbo.Faculties
                SET FacultyCode = @FacultyCode, FacultyName = @FacultyName, DeanName = @DeanName,
                    Description = @Description, IsActive = @IsActive, UpdatedDate = GETDATE()
                WHERE FacultyId = @FacultyId";

            var rows = await _db.ExecuteNonQueryAsync(sql, CommandType.Text, new[]
            {
                new SqlParameter("@FacultyCode", faculty.FacultyCode),
                new SqlParameter("@FacultyName", faculty.FacultyName),
                new SqlParameter("@DeanName", (object?)faculty.DeanName ?? DBNull.Value),
                new SqlParameter("@Description", (object?)faculty.Description ?? DBNull.Value),
                new SqlParameter("@IsActive", faculty.IsActive),
                new SqlParameter("@FacultyId", faculty.FacultyId)
            });
            return rows > 0;
        }

        public async Task<bool> DeleteFacultyAsync(int facultyId)
        {
            const string sql = "UPDATE dbo.Faculties SET IsActive = 0, UpdatedDate = GETDATE() WHERE FacultyId = @Id";
            var rows = await _db.ExecuteNonQueryAsync(sql, CommandType.Text, new[] { new SqlParameter("@Id", facultyId) });
            return rows > 0;
        }

        // ======================== COLLEGES ========================
        public async Task<List<College>> GetCollegesAsync()
        {
            const string sql = @"
                SELECT c.CollegeId, c.CollegeCode, c.CollegeName, c.Address, c.ContactNumber, 
                       c.Email, c.PrincipalName, c.AffiliationDate, c.Website, c.LogoUrl, c.IsActive,
                       (SELECT COUNT(*) FROM dbo.Students s WHERE s.CollegeId = c.CollegeId) AS StudentCount
                FROM dbo.Colleges c
                ORDER BY c.CollegeName";

            return await _db.ExecuteReaderListAsync(sql, r => new College
            {
                CollegeId = r.GetInt32(r.GetOrdinal("CollegeId")),
                CollegeCode = r.GetString(r.GetOrdinal("CollegeCode")),
                CollegeName = r.GetString(r.GetOrdinal("CollegeName")),
                Address = r.GetString(r.GetOrdinal("Address")),
                ContactNumber = r.GetString(r.GetOrdinal("ContactNumber")),
                Email = r.GetString(r.GetOrdinal("Email")),
                PrincipalName = r.IsDBNull(r.GetOrdinal("PrincipalName")) ? null : r.GetString(r.GetOrdinal("PrincipalName")),
                AffiliationDate = r.IsDBNull(r.GetOrdinal("AffiliationDate")) ? null : r.GetDateTime(r.GetOrdinal("AffiliationDate")),
                Website = r.IsDBNull(r.GetOrdinal("Website")) ? null : r.GetString(r.GetOrdinal("Website")),
                LogoUrl = r.IsDBNull(r.GetOrdinal("LogoUrl")) ? null : r.GetString(r.GetOrdinal("LogoUrl")),
                IsActive = r.GetBoolean(r.GetOrdinal("IsActive")),
                StudentCount = r.GetInt32(r.GetOrdinal("StudentCount"))
            });
        }

        public async Task<College?> GetCollegeByIdAsync(int collegeId)
        {
            const string sql = @"
                SELECT CollegeId, CollegeCode, CollegeName, Address, ContactNumber, 
                       Email, PrincipalName, AffiliationDate, Website, LogoUrl, IsActive, 0 AS StudentCount
                FROM dbo.Colleges
                WHERE CollegeId = @Id";

            return await _db.ExecuteReaderSingleAsync(sql, r => new College
            {
                CollegeId = r.GetInt32(r.GetOrdinal("CollegeId")),
                CollegeCode = r.GetString(r.GetOrdinal("CollegeCode")),
                CollegeName = r.GetString(r.GetOrdinal("CollegeName")),
                Address = r.GetString(r.GetOrdinal("Address")),
                ContactNumber = r.GetString(r.GetOrdinal("ContactNumber")),
                Email = r.GetString(r.GetOrdinal("Email")),
                PrincipalName = r.IsDBNull(r.GetOrdinal("PrincipalName")) ? null : r.GetString(r.GetOrdinal("PrincipalName")),
                AffiliationDate = r.IsDBNull(r.GetOrdinal("AffiliationDate")) ? null : r.GetDateTime(r.GetOrdinal("AffiliationDate")),
                Website = r.IsDBNull(r.GetOrdinal("Website")) ? null : r.GetString(r.GetOrdinal("Website")),
                LogoUrl = r.IsDBNull(r.GetOrdinal("LogoUrl")) ? null : r.GetString(r.GetOrdinal("LogoUrl")),
                IsActive = r.GetBoolean(r.GetOrdinal("IsActive"))
            }, CommandType.Text, new[] { new SqlParameter("@Id", collegeId) });
        }

        public async Task<int> AddCollegeAsync(College college)
        {
            const string sql = @"
                INSERT INTO dbo.Colleges (CollegeCode, CollegeName, Address, ContactNumber, Email, PrincipalName, AffiliationDate, Website, LogoUrl, IsActive, CreatedDate)
                VALUES (@Code, @Name, @Address, @Contact, @Email, @Principal, @AffiliationDate, @Website, @LogoUrl, @IsActive, GETDATE());
                SELECT SCOPE_IDENTITY();";

            var result = await _db.ExecuteScalarAsync(sql, CommandType.Text, new[]
            {
                new SqlParameter("@Code", college.CollegeCode),
                new SqlParameter("@Name", college.CollegeName),
                new SqlParameter("@Address", college.Address),
                new SqlParameter("@Contact", college.ContactNumber),
                new SqlParameter("@Email", college.Email),
                new SqlParameter("@Principal", (object?)college.PrincipalName ?? DBNull.Value),
                new SqlParameter("@AffiliationDate", (object?)college.AffiliationDate ?? DBNull.Value),
                new SqlParameter("@Website", (object?)college.Website ?? DBNull.Value),
                new SqlParameter("@LogoUrl", (object?)college.LogoUrl ?? DBNull.Value),
                new SqlParameter("@IsActive", college.IsActive)
            });
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateCollegeAsync(College college)
        {
            const string sql = @"
                UPDATE dbo.Colleges
                SET CollegeCode = @Code, CollegeName = @Name, Address = @Address,
                    ContactNumber = @Contact, Email = @Email, PrincipalName = @Principal,
                    AffiliationDate = @AffiliationDate, Website = @Website, LogoUrl = @LogoUrl,
                    IsActive = @IsActive, UpdatedDate = GETDATE()
                WHERE CollegeId = @Id";

            var rows = await _db.ExecuteNonQueryAsync(sql, CommandType.Text, new[]
            {
                new SqlParameter("@Code", college.CollegeCode),
                new SqlParameter("@Name", college.CollegeName),
                new SqlParameter("@Address", college.Address),
                new SqlParameter("@Contact", college.ContactNumber),
                new SqlParameter("@Email", college.Email),
                new SqlParameter("@Principal", (object?)college.PrincipalName ?? DBNull.Value),
                new SqlParameter("@AffiliationDate", (object?)college.AffiliationDate ?? DBNull.Value),
                new SqlParameter("@Website", (object?)college.Website ?? DBNull.Value),
                new SqlParameter("@LogoUrl", (object?)college.LogoUrl ?? DBNull.Value),
                new SqlParameter("@IsActive", college.IsActive),
                new SqlParameter("@Id", college.CollegeId)
            });
            return rows > 0;
        }

        public async Task<bool> DeleteCollegeAsync(int collegeId)
        {
            const string sql = "UPDATE dbo.Colleges SET IsActive = 0, UpdatedDate = GETDATE() WHERE CollegeId = @Id";
            var rows = await _db.ExecuteNonQueryAsync(sql, CommandType.Text, new[] { new SqlParameter("@Id", collegeId) });
            return rows > 0;
        }

        // ======================== PROGRAMS ========================
        public async Task<List<ProgramModel>> GetProgramsAsync()
        {
            const string sql = @"
                SELECT p.ProgramId, p.ProgramCode, p.ProgramName, p.FacultyId, f.FacultyName,
                       p.DegreeLevel, p.DurationYears, p.TotalSemesters, p.TotalCredits, p.Description, p.IsActive
                FROM dbo.Programs p
                JOIN dbo.Faculties f ON p.FacultyId = f.FacultyId
                ORDER BY f.FacultyName, p.ProgramName";

            return await _db.ExecuteReaderListAsync(sql, r => new ProgramModel
            {
                ProgramId = r.GetInt32(r.GetOrdinal("ProgramId")),
                ProgramCode = r.GetString(r.GetOrdinal("ProgramCode")),
                ProgramName = r.GetString(r.GetOrdinal("ProgramName")),
                FacultyId = r.GetInt32(r.GetOrdinal("FacultyId")),
                FacultyName = r.GetString(r.GetOrdinal("FacultyName")),
                DegreeLevel = r.GetString(r.GetOrdinal("DegreeLevel")),
                DurationYears = r.GetDecimal(r.GetOrdinal("DurationYears")),
                TotalSemesters = r.GetInt32(r.GetOrdinal("TotalSemesters")),
                TotalCredits = r.GetInt32(r.GetOrdinal("TotalCredits")),
                Description = r.IsDBNull(r.GetOrdinal("Description")) ? null : r.GetString(r.GetOrdinal("Description")),
                IsActive = r.GetBoolean(r.GetOrdinal("IsActive"))
            });
        }

        public async Task<List<ProgramModel>> GetProgramsByFacultyAsync(int facultyId)
        {
            const string sql = @"
                SELECT p.ProgramId, p.ProgramCode, p.ProgramName, p.FacultyId, f.FacultyName,
                       p.DegreeLevel, p.DurationYears, p.TotalSemesters, p.TotalCredits, p.Description, p.IsActive
                FROM dbo.Programs p
                JOIN dbo.Faculties f ON p.FacultyId = f.FacultyId
                WHERE p.FacultyId = @FacultyId AND p.IsActive = 1
                ORDER BY p.ProgramName";

            return await _db.ExecuteReaderListAsync(sql, r => new ProgramModel
            {
                ProgramId = r.GetInt32(r.GetOrdinal("ProgramId")),
                ProgramCode = r.GetString(r.GetOrdinal("ProgramCode")),
                ProgramName = r.GetString(r.GetOrdinal("ProgramName")),
                FacultyId = r.GetInt32(r.GetOrdinal("FacultyId")),
                FacultyName = r.GetString(r.GetOrdinal("FacultyName")),
                DegreeLevel = r.GetString(r.GetOrdinal("DegreeLevel")),
                DurationYears = r.GetDecimal(r.GetOrdinal("DurationYears")),
                TotalSemesters = r.GetInt32(r.GetOrdinal("TotalSemesters")),
                TotalCredits = r.GetInt32(r.GetOrdinal("TotalCredits")),
                Description = r.IsDBNull(r.GetOrdinal("Description")) ? null : r.GetString(r.GetOrdinal("Description")),
                IsActive = r.GetBoolean(r.GetOrdinal("IsActive"))
            }, CommandType.Text, new[] { new SqlParameter("@FacultyId", facultyId) });
        }

        public async Task<ProgramModel?> GetProgramByIdAsync(int programId)
        {
            const string sql = @"
                SELECT p.ProgramId, p.ProgramCode, p.ProgramName, p.FacultyId, f.FacultyName,
                       p.DegreeLevel, p.DurationYears, p.TotalSemesters, p.TotalCredits, p.Description, p.IsActive
                FROM dbo.Programs p
                JOIN dbo.Faculties f ON p.FacultyId = f.FacultyId
                WHERE p.ProgramId = @Id";

            return await _db.ExecuteReaderSingleAsync(sql, r => new ProgramModel
            {
                ProgramId = r.GetInt32(r.GetOrdinal("ProgramId")),
                ProgramCode = r.GetString(r.GetOrdinal("ProgramCode")),
                ProgramName = r.GetString(r.GetOrdinal("ProgramName")),
                FacultyId = r.GetInt32(r.GetOrdinal("FacultyId")),
                FacultyName = r.GetString(r.GetOrdinal("FacultyName")),
                DegreeLevel = r.GetString(r.GetOrdinal("DegreeLevel")),
                DurationYears = r.GetDecimal(r.GetOrdinal("DurationYears")),
                TotalSemesters = r.GetInt32(r.GetOrdinal("TotalSemesters")),
                TotalCredits = r.GetInt32(r.GetOrdinal("TotalCredits")),
                Description = r.IsDBNull(r.GetOrdinal("Description")) ? null : r.GetString(r.GetOrdinal("Description")),
                IsActive = r.GetBoolean(r.GetOrdinal("IsActive"))
            }, CommandType.Text, new[] { new SqlParameter("@Id", programId) });
        }

        public async Task<int> AddProgramAsync(ProgramModel program)
        {
            const string sql = @"
                INSERT INTO dbo.Programs (ProgramCode, ProgramName, FacultyId, DegreeLevel, DurationYears, TotalSemesters, TotalCredits, Description, IsActive, CreatedDate)
                VALUES (@Code, @Name, @FacultyId, @DegreeLevel, @DurationYears, @TotalSemesters, @TotalCredits, @Description, @IsActive, GETDATE());
                SELECT SCOPE_IDENTITY();";

            var result = await _db.ExecuteScalarAsync(sql, CommandType.Text, new[]
            {
                new SqlParameter("@Code", program.ProgramCode),
                new SqlParameter("@Name", program.ProgramName),
                new SqlParameter("@FacultyId", program.FacultyId),
                new SqlParameter("@DegreeLevel", program.DegreeLevel),
                new SqlParameter("@DurationYears", program.DurationYears),
                new SqlParameter("@TotalSemesters", program.TotalSemesters),
                new SqlParameter("@TotalCredits", program.TotalCredits),
                new SqlParameter("@Description", (object?)program.Description ?? DBNull.Value),
                new SqlParameter("@IsActive", program.IsActive)
            });
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateProgramAsync(ProgramModel program)
        {
            const string sql = @"
                UPDATE dbo.Programs
                SET ProgramCode = @Code, ProgramName = @Name, FacultyId = @FacultyId, DegreeLevel = @DegreeLevel,
                    DurationYears = @DurationYears, TotalSemesters = @TotalSemesters, TotalCredits = @TotalCredits,
                    Description = @Description, IsActive = @IsActive, UpdatedDate = GETDATE()
                WHERE ProgramId = @Id";

            var rows = await _db.ExecuteNonQueryAsync(sql, CommandType.Text, new[]
            {
                new SqlParameter("@Code", program.ProgramCode),
                new SqlParameter("@Name", program.ProgramName),
                new SqlParameter("@FacultyId", program.FacultyId),
                new SqlParameter("@DegreeLevel", program.DegreeLevel),
                new SqlParameter("@DurationYears", program.DurationYears),
                new SqlParameter("@TotalSemesters", program.TotalSemesters),
                new SqlParameter("@TotalCredits", program.TotalCredits),
                new SqlParameter("@Description", (object?)program.Description ?? DBNull.Value),
                new SqlParameter("@IsActive", program.IsActive),
                new SqlParameter("@Id", program.ProgramId)
            });
            return rows > 0;
        }

        public async Task<bool> DeleteProgramAsync(int programId)
        {
            const string sql = "UPDATE dbo.Programs SET IsActive = 0, UpdatedDate = GETDATE() WHERE ProgramId = @Id";
            var rows = await _db.ExecuteNonQueryAsync(sql, CommandType.Text, new[] { new SqlParameter("@Id", programId) });
            return rows > 0;
        }

        // ======================== SUBJECTS ========================
        public async Task<List<Subject>> GetSubjectsAsync(int? programId = null, int? semesterId = null)
        {
            var sql = @"
                SELECT s.SubjectId, s.SubjectCode, s.SubjectName, s.ProgramId, p.ProgramName,
                       s.SemesterId, sem.SemesterName, s.CreditHours, s.FullMarks, s.PassMarks,
                       s.InternalMarks, s.ExternalMarks, s.PracticalMarks, s.HasPractical, s.IsElective, s.IsActive
                FROM dbo.Subjects s
                JOIN dbo.Programs p ON s.ProgramId = p.ProgramId
                JOIN dbo.Semesters sem ON s.SemesterId = sem.SemesterId
                WHERE 1=1";

            var parameters = new List<SqlParameter>();
            if (programId.HasValue)
            {
                sql += " AND s.ProgramId = @ProgramId";
                parameters.Add(new SqlParameter("@ProgramId", programId.Value));
            }
            if (semesterId.HasValue)
            {
                sql += " AND s.SemesterId = @SemesterId";
                parameters.Add(new SqlParameter("@SemesterId", semesterId.Value));
            }
            sql += " ORDER BY p.ProgramName, sem.SemesterNumber, s.SubjectCode";

            return await _db.ExecuteReaderListAsync(sql, r => new Subject
            {
                SubjectId = r.GetInt32(r.GetOrdinal("SubjectId")),
                SubjectCode = r.GetString(r.GetOrdinal("SubjectCode")),
                SubjectName = r.GetString(r.GetOrdinal("SubjectName")),
                ProgramId = r.GetInt32(r.GetOrdinal("ProgramId")),
                ProgramName = r.GetString(r.GetOrdinal("ProgramName")),
                SemesterId = r.GetInt32(r.GetOrdinal("SemesterId")),
                SemesterName = r.GetString(r.GetOrdinal("SemesterName")),
                CreditHours = r.GetInt32(r.GetOrdinal("CreditHours")),
                FullMarks = r.GetInt32(r.GetOrdinal("FullMarks")),
                PassMarks = r.GetInt32(r.GetOrdinal("PassMarks")),
                InternalMarks = r.GetInt32(r.GetOrdinal("InternalMarks")),
                ExternalMarks = r.GetInt32(r.GetOrdinal("ExternalMarks")),
                PracticalMarks = r.GetInt32(r.GetOrdinal("PracticalMarks")),
                HasPractical = r.GetBoolean(r.GetOrdinal("HasPractical")),
                IsElective = r.GetBoolean(r.GetOrdinal("IsElective")),
                IsActive = r.GetBoolean(r.GetOrdinal("IsActive"))
            }, CommandType.Text, parameters.ToArray());
        }

        public async Task<Subject?> GetSubjectByIdAsync(int subjectId)
        {
            const string sql = @"
                SELECT s.SubjectId, s.SubjectCode, s.SubjectName, s.ProgramId, p.ProgramName,
                       s.SemesterId, sem.SemesterName, s.CreditHours, s.FullMarks, s.PassMarks,
                       s.InternalMarks, s.ExternalMarks, s.PracticalMarks, s.HasPractical, s.IsElective, s.IsActive
                FROM dbo.Subjects s
                JOIN dbo.Programs p ON s.ProgramId = p.ProgramId
                JOIN dbo.Semesters sem ON s.SemesterId = sem.SemesterId
                WHERE s.SubjectId = @Id";

            return await _db.ExecuteReaderSingleAsync(sql, r => new Subject
            {
                SubjectId = r.GetInt32(r.GetOrdinal("SubjectId")),
                SubjectCode = r.GetString(r.GetOrdinal("SubjectCode")),
                SubjectName = r.GetString(r.GetOrdinal("SubjectName")),
                ProgramId = r.GetInt32(r.GetOrdinal("ProgramId")),
                ProgramName = r.GetString(r.GetOrdinal("ProgramName")),
                SemesterId = r.GetInt32(r.GetOrdinal("SemesterId")),
                SemesterName = r.GetString(r.GetOrdinal("SemesterName")),
                CreditHours = r.GetInt32(r.GetOrdinal("CreditHours")),
                FullMarks = r.GetInt32(r.GetOrdinal("FullMarks")),
                PassMarks = r.GetInt32(r.GetOrdinal("PassMarks")),
                InternalMarks = r.GetInt32(r.GetOrdinal("InternalMarks")),
                ExternalMarks = r.GetInt32(r.GetOrdinal("ExternalMarks")),
                PracticalMarks = r.GetInt32(r.GetOrdinal("PracticalMarks")),
                HasPractical = r.GetBoolean(r.GetOrdinal("HasPractical")),
                IsElective = r.GetBoolean(r.GetOrdinal("IsElective")),
                IsActive = r.GetBoolean(r.GetOrdinal("IsActive"))
            }, CommandType.Text, new[] { new SqlParameter("@Id", subjectId) });
        }

        public async Task<int> AddSubjectAsync(Subject subject)
        {
            const string sql = @"
                INSERT INTO dbo.Subjects (SubjectCode, SubjectName, ProgramId, SemesterId, CreditHours, FullMarks, PassMarks, InternalMarks, ExternalMarks, PracticalMarks, HasPractical, IsElective, IsActive, CreatedDate)
                VALUES (@Code, @Name, @ProgramId, @SemesterId, @CreditHours, @FullMarks, @PassMarks, @InternalMarks, @ExternalMarks, @PracticalMarks, @HasPractical, @IsElective, @IsActive, GETDATE());
                SELECT SCOPE_IDENTITY();";

            var result = await _db.ExecuteScalarAsync(sql, CommandType.Text, new[]
            {
                new SqlParameter("@Code", subject.SubjectCode),
                new SqlParameter("@Name", subject.SubjectName),
                new SqlParameter("@ProgramId", subject.ProgramId),
                new SqlParameter("@SemesterId", subject.SemesterId),
                new SqlParameter("@CreditHours", subject.CreditHours),
                new SqlParameter("@FullMarks", subject.FullMarks),
                new SqlParameter("@PassMarks", subject.PassMarks),
                new SqlParameter("@InternalMarks", subject.InternalMarks),
                new SqlParameter("@ExternalMarks", subject.ExternalMarks),
                new SqlParameter("@PracticalMarks", subject.PracticalMarks),
                new SqlParameter("@HasPractical", subject.HasPractical),
                new SqlParameter("@IsElective", subject.IsElective),
                new SqlParameter("@IsActive", subject.IsActive)
            });
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateSubjectAsync(Subject subject)
        {
            const string sql = @"
                UPDATE dbo.Subjects
                SET SubjectCode = @Code, SubjectName = @Name, ProgramId = @ProgramId, SemesterId = @SemesterId,
                    CreditHours = @CreditHours, FullMarks = @FullMarks, PassMarks = @PassMarks,
                    InternalMarks = @InternalMarks, ExternalMarks = @ExternalMarks, PracticalMarks = @PracticalMarks,
                    HasPractical = @HasPractical, IsElective = @IsElective, IsActive = @IsActive, UpdatedDate = GETDATE()
                WHERE SubjectId = @Id";

            var rows = await _db.ExecuteNonQueryAsync(sql, CommandType.Text, new[]
            {
                new SqlParameter("@Code", subject.SubjectCode),
                new SqlParameter("@Name", subject.SubjectName),
                new SqlParameter("@ProgramId", subject.ProgramId),
                new SqlParameter("@SemesterId", subject.SemesterId),
                new SqlParameter("@CreditHours", subject.CreditHours),
                new SqlParameter("@FullMarks", subject.FullMarks),
                new SqlParameter("@PassMarks", subject.PassMarks),
                new SqlParameter("@InternalMarks", subject.InternalMarks),
                new SqlParameter("@ExternalMarks", subject.ExternalMarks),
                new SqlParameter("@PracticalMarks", subject.PracticalMarks),
                new SqlParameter("@HasPractical", subject.HasPractical),
                new SqlParameter("@IsElective", subject.IsElective),
                new SqlParameter("@IsActive", subject.IsActive),
                new SqlParameter("@Id", subject.SubjectId)
            });
            return rows > 0;
        }

        public async Task<bool> DeleteSubjectAsync(int subjectId)
        {
            const string sql = "UPDATE dbo.Subjects SET IsActive = 0, UpdatedDate = GETDATE() WHERE SubjectId = @Id";
            var rows = await _db.ExecuteNonQueryAsync(sql, CommandType.Text, new[] { new SqlParameter("@Id", subjectId) });
            return rows > 0;
        }

        // ======================== ACADEMIC YEARS & SEMESTERS ========================
        public async Task<List<AcademicYear>> GetAcademicYearsAsync()
        {
            const string sql = "SELECT AcademicYearId, YearName, StartDate, EndDate, IsCurrent, IsActive FROM dbo.AcademicYears ORDER BY YearName DESC";
            return await _db.ExecuteReaderListAsync(sql, r => new AcademicYear
            {
                AcademicYearId = r.GetInt32(r.GetOrdinal("AcademicYearId")),
                YearName = r.GetString(r.GetOrdinal("YearName")),
                StartDate = r.GetDateTime(r.GetOrdinal("StartDate")),
                EndDate = r.GetDateTime(r.GetOrdinal("EndDate")),
                IsCurrent = r.GetBoolean(r.GetOrdinal("IsCurrent")),
                IsActive = r.GetBoolean(r.GetOrdinal("IsActive"))
            });
        }

        public async Task<AcademicYear?> GetCurrentAcademicYearAsync()
        {
            const string sql = "SELECT TOP 1 AcademicYearId, YearName, StartDate, EndDate, IsCurrent, IsActive FROM dbo.AcademicYears WHERE IsCurrent = 1 AND IsActive = 1";
            return await _db.ExecuteReaderSingleAsync(sql, r => new AcademicYear
            {
                AcademicYearId = r.GetInt32(r.GetOrdinal("AcademicYearId")),
                YearName = r.GetString(r.GetOrdinal("YearName")),
                StartDate = r.GetDateTime(r.GetOrdinal("StartDate")),
                EndDate = r.GetDateTime(r.GetOrdinal("EndDate")),
                IsCurrent = r.GetBoolean(r.GetOrdinal("IsCurrent")),
                IsActive = r.GetBoolean(r.GetOrdinal("IsActive"))
            });
        }

        public async Task<List<Semester>> GetSemestersAsync()
        {
            const string sql = "SELECT SemesterId, SemesterName, SemesterNumber, Description, IsActive FROM dbo.Semesters ORDER BY SemesterNumber";
            return await _db.ExecuteReaderListAsync(sql, r => new Semester
            {
                SemesterId = r.GetInt32(r.GetOrdinal("SemesterId")),
                SemesterName = r.GetString(r.GetOrdinal("SemesterName")),
                SemesterNumber = r.GetInt32(r.GetOrdinal("SemesterNumber")),
                Description = r.IsDBNull(r.GetOrdinal("Description")) ? null : r.GetString(r.GetOrdinal("Description")),
                IsActive = r.GetBoolean(r.GetOrdinal("IsActive"))
            });
        }

        public async Task<Semester?> GetSemesterByIdAsync(int semesterId)
        {
            const string sql = "SELECT SemesterId, SemesterName, SemesterNumber, Description, IsActive FROM dbo.Semesters WHERE SemesterId = @Id";
            return await _db.ExecuteReaderSingleAsync(sql, r => new Semester
            {
                SemesterId = r.GetInt32(r.GetOrdinal("SemesterId")),
                SemesterName = r.GetString(r.GetOrdinal("SemesterName")),
                SemesterNumber = r.GetInt32(r.GetOrdinal("SemesterNumber")),
                Description = r.IsDBNull(r.GetOrdinal("Description")) ? null : r.GetString(r.GetOrdinal("Description")),
                IsActive = r.GetBoolean(r.GetOrdinal("IsActive"))
            }, CommandType.Text, new[] { new SqlParameter("@Id", semesterId) });
        }

        // ======================== GRADE SCALES ========================
        public async Task<List<GradeScale>> GetGradeScalesAsync()
        {
            const string sql = "SELECT GradeScaleId, ProgramLevel, MinPercentage, MaxPercentage, Grade, GradePoint, Description, IsActive FROM dbo.GradeScales ORDER BY GradePoint DESC";
            return await _db.ExecuteReaderListAsync(sql, r => new GradeScale
            {
                GradeScaleId = r.GetInt32(r.GetOrdinal("GradeScaleId")),
                ProgramLevel = r.GetString(r.GetOrdinal("ProgramLevel")),
                MinPercentage = r.GetDecimal(r.GetOrdinal("MinPercentage")),
                MaxPercentage = r.GetDecimal(r.GetOrdinal("MaxPercentage")),
                Grade = r.GetString(r.GetOrdinal("Grade")),
                GradePoint = r.GetDecimal(r.GetOrdinal("GradePoint")),
                Description = r.GetString(r.GetOrdinal("Description")),
                IsActive = r.GetBoolean(r.GetOrdinal("IsActive"))
            });
        }

        public async Task<GradeScale?> GetGradeScaleByIdAsync(int gradeScaleId)
        {
            const string sql = "SELECT GradeScaleId, ProgramLevel, MinPercentage, MaxPercentage, Grade, GradePoint, Description, IsActive FROM dbo.GradeScales WHERE GradeScaleId = @Id";
            return await _db.ExecuteReaderSingleAsync(sql, r => new GradeScale
            {
                GradeScaleId = r.GetInt32(r.GetOrdinal("GradeScaleId")),
                ProgramLevel = r.GetString(r.GetOrdinal("ProgramLevel")),
                MinPercentage = r.GetDecimal(r.GetOrdinal("MinPercentage")),
                MaxPercentage = r.GetDecimal(r.GetOrdinal("MaxPercentage")),
                Grade = r.GetString(r.GetOrdinal("Grade")),
                GradePoint = r.GetDecimal(r.GetOrdinal("GradePoint")),
                Description = r.GetString(r.GetOrdinal("Description")),
                IsActive = r.GetBoolean(r.GetOrdinal("IsActive"))
            }, CommandType.Text, new[] { new SqlParameter("@Id", gradeScaleId) });
        }

        public async Task<bool> UpdateGradeScaleAsync(GradeScale scale)
        {
            const string sql = @"
                UPDATE dbo.GradeScales
                SET MinPercentage = @Min, MaxPercentage = @Max, Grade = @Grade,
                    GradePoint = @Point, Description = @Desc, IsActive = @IsActive
                WHERE GradeScaleId = @Id";

            var rows = await _db.ExecuteNonQueryAsync(sql, CommandType.Text, new[]
            {
                new SqlParameter("@Min", scale.MinPercentage),
                new SqlParameter("@Max", scale.MaxPercentage),
                new SqlParameter("@Grade", scale.Grade),
                new SqlParameter("@Point", scale.GradePoint),
                new SqlParameter("@Desc", scale.Description),
                new SqlParameter("@IsActive", scale.IsActive),
                new SqlParameter("@Id", scale.GradeScaleId)
            });
            return rows > 0;
        }

        public async Task<int> AddGradeScaleAsync(GradeScale scale)
        {
            const string sql = @"
                INSERT INTO dbo.GradeScales (ProgramLevel, MinPercentage, MaxPercentage, Grade, GradePoint, Description, IsActive, CreatedDate)
                VALUES (@Level, @Min, @Max, @Grade, @Point, @Desc, @IsActive, GETDATE());
                SELECT SCOPE_IDENTITY();";

            var result = await _db.ExecuteScalarAsync(sql, CommandType.Text, new[]
            {
                new SqlParameter("@Level", scale.ProgramLevel),
                new SqlParameter("@Min", scale.MinPercentage),
                new SqlParameter("@Max", scale.MaxPercentage),
                new SqlParameter("@Grade", scale.Grade),
                new SqlParameter("@Point", scale.GradePoint),
                new SqlParameter("@Desc", scale.Description),
                new SqlParameter("@IsActive", scale.IsActive)
            });
            return Convert.ToInt32(result);
        }

        // ======================== EXAMINATION CENTERS ========================
        public async Task<List<ExaminationCenter>> GetCentersAsync()
        {
            const string sql = "SELECT CenterId, CenterCode, CenterName, Location, Capacity, SuperintendentName, ContactNumber, IsActive FROM dbo.ExaminationCenters ORDER BY CenterName";
            return await _db.ExecuteReaderListAsync(sql, r => new ExaminationCenter
            {
                CenterId = r.GetInt32(r.GetOrdinal("CenterId")),
                CenterCode = r.GetString(r.GetOrdinal("CenterCode")),
                CenterName = r.GetString(r.GetOrdinal("CenterName")),
                Location = r.GetString(r.GetOrdinal("Location")),
                Capacity = r.GetInt32(r.GetOrdinal("Capacity")),
                SuperintendentName = r.IsDBNull(r.GetOrdinal("SuperintendentName")) ? null : r.GetString(r.GetOrdinal("SuperintendentName")),
                ContactNumber = r.IsDBNull(r.GetOrdinal("ContactNumber")) ? null : r.GetString(r.GetOrdinal("ContactNumber")),
                IsActive = r.GetBoolean(r.GetOrdinal("IsActive"))
            });
        }

        public async Task<ExaminationCenter?> GetCenterByIdAsync(int centerId)
        {
            const string sql = "SELECT CenterId, CenterCode, CenterName, Location, Capacity, SuperintendentName, ContactNumber, IsActive FROM dbo.ExaminationCenters WHERE CenterId = @Id";
            return await _db.ExecuteReaderSingleAsync(sql, r => new ExaminationCenter
            {
                CenterId = r.GetInt32(r.GetOrdinal("CenterId")),
                CenterCode = r.GetString(r.GetOrdinal("CenterCode")),
                CenterName = r.GetString(r.GetOrdinal("CenterName")),
                Location = r.GetString(r.GetOrdinal("Location")),
                Capacity = r.GetInt32(r.GetOrdinal("Capacity")),
                SuperintendentName = r.IsDBNull(r.GetOrdinal("SuperintendentName")) ? null : r.GetString(r.GetOrdinal("SuperintendentName")),
                ContactNumber = r.IsDBNull(r.GetOrdinal("ContactNumber")) ? null : r.GetString(r.GetOrdinal("ContactNumber")),
                IsActive = r.GetBoolean(r.GetOrdinal("IsActive"))
            }, CommandType.Text, new[] { new SqlParameter("@Id", centerId) });
        }

        public async Task<int> AddCenterAsync(ExaminationCenter center)
        {
            const string sql = @"
                INSERT INTO dbo.ExaminationCenters (CenterCode, CenterName, Location, Capacity, SuperintendentName, ContactNumber, IsActive, CreatedDate)
                VALUES (@Code, @Name, @Location, @Capacity, @Superintendent, @Contact, @IsActive, GETDATE());
                SELECT SCOPE_IDENTITY();";

            var result = await _db.ExecuteScalarAsync(sql, CommandType.Text, new[]
            {
                new SqlParameter("@Code", center.CenterCode),
                new SqlParameter("@Name", center.CenterName),
                new SqlParameter("@Location", center.Location),
                new SqlParameter("@Capacity", center.Capacity),
                new SqlParameter("@Superintendent", (object?)center.SuperintendentName ?? DBNull.Value),
                new SqlParameter("@Contact", (object?)center.ContactNumber ?? DBNull.Value),
                new SqlParameter("@IsActive", center.IsActive)
            });
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateCenterAsync(ExaminationCenter center)
        {
            const string sql = @"
                UPDATE dbo.ExaminationCenters
                SET CenterCode = @Code, CenterName = @Name, Location = @Location,
                    Capacity = @Capacity, SuperintendentName = @Superintendent,
                    ContactNumber = @Contact, IsActive = @IsActive
                WHERE CenterId = @Id";

            var rows = await _db.ExecuteNonQueryAsync(sql, CommandType.Text, new[]
            {
                new SqlParameter("@Code", center.CenterCode),
                new SqlParameter("@Name", center.CenterName),
                new SqlParameter("@Location", center.Location),
                new SqlParameter("@Capacity", center.Capacity),
                new SqlParameter("@Superintendent", (object?)center.SuperintendentName ?? DBNull.Value),
                new SqlParameter("@Contact", (object?)center.ContactNumber ?? DBNull.Value),
                new SqlParameter("@IsActive", center.IsActive),
                new SqlParameter("@Id", center.CenterId)
            });
            return rows > 0;
        }
    }
}
