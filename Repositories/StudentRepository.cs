using System.Data;
using Microsoft.Data.SqlClient;
using UniversityExamSystem.Data;
using UniversityExamSystem.Models;

namespace UniversityExamSystem.Repositories
{
    public interface IStudentRepository
    {
        Task<(int studentId, string uniqueId, string appNumber)> RegisterStudentAsync(Student student, string passwordHash);
        Task<Student?> GetProfileAsync(int studentId);
        Task<Student?> GetByUserIdAsync(int userId);
        Task<Student?> GetByRegistrationNumberAsync(string registrationNumber);
        Task<Student?> GetByUniqueIdAsync(string studentUniqueId);
        Task<Student?> TrackRegistrationAsync(string applicationNumberOrEmail);
        Task<List<Student>> GetStudentsAsync(int? collegeId = null, int? programId = null, int? semesterId = null, string? status = null, string? search = null);
        Task<(bool success, string regNumber)> VerifyRegistrationAsync(int studentId, string status, string remarks, int verifiedBy);
        Task<bool> UpdateProfileAsync(Student student);
        Task<DataTable> ExportStudentsToDataTableAsync(int? collegeId = null, int? programId = null);
        Task<int> AddStudentDocumentAsync(StudentDocument doc);
        Task<List<StudentDocument>> GetStudentDocumentsAsync(int studentId);
    }

    public class StudentRepository : IStudentRepository
    {
        private readonly DatabaseHelper _db;

        public StudentRepository(DatabaseHelper db)
        {
            _db = db;
        }

        public async Task<(int studentId, string uniqueId, string appNumber)> RegisterStudentAsync(Student student, string passwordHash)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            using var cmd = new SqlCommand("dbo.sp_RegisterStudent", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@FullName", student.FullName);
            cmd.Parameters.AddWithValue("@DateOfBirth", student.DateOfBirth);
            cmd.Parameters.AddWithValue("@Gender", student.Gender);
            cmd.Parameters.AddWithValue("@Email", student.Email);
            cmd.Parameters.AddWithValue("@MobileNumber", student.MobileNumber);
            cmd.Parameters.AddWithValue("@Address", student.Address);
            cmd.Parameters.AddWithValue("@FacultyId", student.FacultyId);
            cmd.Parameters.AddWithValue("@ProgramId", student.ProgramId);
            cmd.Parameters.AddWithValue("@CollegeId", student.CollegeId);
            cmd.Parameters.AddWithValue("@AcademicYearId", student.AcademicYearId);
            cmd.Parameters.AddWithValue("@CurrentSemesterId", student.CurrentSemesterId);
            cmd.Parameters.AddWithValue("@PreviousQualification", (object?)student.PreviousQualification ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PhotoUrl", (object?)student.PhotoUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IdentityDocumentUrl", (object?)student.IdentityDocumentUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);

            var pStudentId = new SqlParameter("@StudentId", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var pUniqueId = new SqlParameter("@StudentUniqueId", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
            var pAppNum = new SqlParameter("@ApplicationNumber", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };

            cmd.Parameters.Add(pStudentId);
            cmd.Parameters.Add(pUniqueId);
            cmd.Parameters.Add(pAppNum);

            await cmd.ExecuteNonQueryAsync();

            int id = (int)pStudentId.Value;
            string uid = (string)pUniqueId.Value;
            string appNo = (string)pAppNum.Value;

            return (id, uid, appNo);
        }

        public async Task<Student?> GetProfileAsync(int studentId)
        {
            var parameters = new[] { new SqlParameter("@StudentId", studentId) };
            return await _db.ExecuteReaderSingleAsync(
                "dbo.sp_GetStudentProfile",
                MapStudentProfile,
                CommandType.StoredProcedure,
                parameters
            );
        }

        public async Task<Student?> GetByUserIdAsync(int userId)
        {
            const string sql = "SELECT TOP 1 StudentId FROM dbo.Students WHERE UserId = @UserId";
            var studentIdObj = await _db.ExecuteScalarAsync(sql, CommandType.Text, new[] { new SqlParameter("@UserId", userId) });
            if (studentIdObj != null && studentIdObj != DBNull.Value)
            {
                return await GetProfileAsync(Convert.ToInt32(studentIdObj));
            }
            return null;
        }

        public async Task<Student?> GetByRegistrationNumberAsync(string registrationNumber)
        {
            const string sql = "SELECT TOP 1 StudentId FROM dbo.Students WHERE RegistrationNumber = @RegNo";
            var studentIdObj = await _db.ExecuteScalarAsync(sql, CommandType.Text, new[] { new SqlParameter("@RegNo", registrationNumber.Trim()) });
            if (studentIdObj != null && studentIdObj != DBNull.Value)
            {
                return await GetProfileAsync(Convert.ToInt32(studentIdObj));
            }
            return null;
        }

        public async Task<Student?> GetByUniqueIdAsync(string studentUniqueId)
        {
            const string sql = "SELECT TOP 1 StudentId FROM dbo.Students WHERE StudentUniqueId = @UniqueId";
            var studentIdObj = await _db.ExecuteScalarAsync(sql, CommandType.Text, new[] { new SqlParameter("@UniqueId", studentUniqueId.Trim()) });
            if (studentIdObj != null && studentIdObj != DBNull.Value)
            {
                return await GetProfileAsync(Convert.ToInt32(studentIdObj));
            }
            return null;
        }

        public async Task<Student?> TrackRegistrationAsync(string applicationNumberOrEmail)
        {
            const string sql = @"
                SELECT TOP 1 s.StudentId
                FROM dbo.Students s
                LEFT JOIN dbo.StudentRegistrations sr ON s.StudentId = sr.StudentId
                WHERE sr.ApplicationNumber = @Query OR s.Email = @Query OR s.StudentUniqueId = @Query OR s.MobileNumber = @Query";

            var studentIdObj = await _db.ExecuteScalarAsync(sql, CommandType.Text, new[] { new SqlParameter("@Query", applicationNumberOrEmail.Trim()) });
            if (studentIdObj != null && studentIdObj != DBNull.Value)
            {
                return await GetProfileAsync(Convert.ToInt32(studentIdObj));
            }
            return null;
        }

        public async Task<List<Student>> GetStudentsAsync(int? collegeId = null, int? programId = null, int? semesterId = null, string? status = null, string? search = null)
        {
            var sql = @"
                SELECT s.StudentId, s.StudentUniqueId, s.RegistrationNumber, s.RollNumber, s.FullName,
                       s.DateOfBirth, s.Gender, s.Email, s.MobileNumber, s.Address,
                       s.FacultyId, f.FacultyName, s.ProgramId, p.ProgramName, p.ProgramCode,
                       s.CollegeId, c.CollegeName, c.CollegeCode, s.AcademicYearId, ay.YearName AS AcademicYear,
                       s.CurrentSemesterId, sem.SemesterName AS CurrentSemester, s.PreviousQualification,
                       s.PhotoUrl, s.IdentityDocumentUrl, s.RegistrationStatus, s.VerificationRemarks,
                       s.VerifiedDate, s.CreatedDate, sr.ApplicationNumber, sr.Status AS RegistrationAppStatus
                FROM dbo.Students s
                JOIN dbo.Faculties f ON s.FacultyId = f.FacultyId
                JOIN dbo.Programs p ON s.ProgramId = p.ProgramId
                JOIN dbo.Colleges c ON s.CollegeId = c.CollegeId
                JOIN dbo.AcademicYears ay ON s.AcademicYearId = ay.AcademicYearId
                JOIN dbo.Semesters sem ON s.CurrentSemesterId = sem.SemesterId
                LEFT JOIN dbo.StudentRegistrations sr ON s.StudentId = sr.StudentId
                WHERE s.IsActive = 1";

            var parameters = new List<SqlParameter>();
            if (collegeId.HasValue)
            {
                sql += " AND s.CollegeId = @CollegeId";
                parameters.Add(new SqlParameter("@CollegeId", collegeId.Value));
            }
            if (programId.HasValue)
            {
                sql += " AND s.ProgramId = @ProgramId";
                parameters.Add(new SqlParameter("@ProgramId", programId.Value));
            }
            if (semesterId.HasValue)
            {
                sql += " AND s.CurrentSemesterId = @SemesterId";
                parameters.Add(new SqlParameter("@SemesterId", semesterId.Value));
            }
            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " AND s.RegistrationStatus = @Status";
                parameters.Add(new SqlParameter("@Status", status));
            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                sql += " AND (s.FullName LIKE @Search OR s.StudentUniqueId LIKE @Search OR s.RegistrationNumber LIKE @Search OR s.Email LIKE @Search)";
                parameters.Add(new SqlParameter("@Search", $"%{search}%"));
            }

            sql += " ORDER BY s.CreatedDate DESC";

            return await _db.ExecuteReaderListAsync(sql, MapStudentProfile, CommandType.Text, parameters.ToArray());
        }

        public async Task<(bool success, string regNumber)> VerifyRegistrationAsync(int studentId, string status, string remarks, int verifiedBy)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            using var cmd = new SqlCommand("dbo.sp_VerifyStudentRegistration", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@StudentId", studentId);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@Remarks", (object?)remarks ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@VerifiedBy", verifiedBy);

            var pReg = new SqlParameter("@GeneratedRegNumber", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(pReg);

            await cmd.ExecuteNonQueryAsync();

            string reg = pReg.Value != DBNull.Value ? (string)pReg.Value : string.Empty;
            return (true, reg);
        }

        public async Task<bool> UpdateProfileAsync(Student student)
        {
            const string sql = @"
                UPDATE dbo.Students
                SET FullName = @FullName, DateOfBirth = @DateOfBirth, Gender = @Gender,
                    MobileNumber = @MobileNumber, Address = @Address,
                    PreviousQualification = @PreviousQualification,
                    PhotoUrl = ISNULL(@PhotoUrl, PhotoUrl),
                    IdentityDocumentUrl = ISNULL(@IdentityDocUrl, IdentityDocumentUrl),
                    UpdatedDate = GETDATE()
                WHERE StudentId = @StudentId";

            var rows = await _db.ExecuteNonQueryAsync(sql, CommandType.Text, new[]
            {
                new SqlParameter("@FullName", student.FullName),
                new SqlParameter("@DateOfBirth", student.DateOfBirth),
                new SqlParameter("@Gender", student.Gender),
                new SqlParameter("@MobileNumber", student.MobileNumber),
                new SqlParameter("@Address", student.Address),
                new SqlParameter("@PreviousQualification", (object?)student.PreviousQualification ?? DBNull.Value),
                new SqlParameter("@PhotoUrl", (object?)student.PhotoUrl ?? DBNull.Value),
                new SqlParameter("@IdentityDocUrl", (object?)student.IdentityDocumentUrl ?? DBNull.Value),
                new SqlParameter("@StudentId", student.StudentId)
            });

            return rows > 0;
        }

        public async Task<DataTable> ExportStudentsToDataTableAsync(int? collegeId = null, int? programId = null)
        {
            var sql = @"
                SELECT s.StudentUniqueId AS [Student ID], s.RegistrationNumber AS [Registration No], s.RollNumber AS [Roll No],
                       s.FullName AS [Full Name], s.DateOfBirth AS [DOB], s.Gender, s.Email, s.MobileNumber AS [Mobile],
                       c.CollegeName AS [College], p.ProgramName AS [Program], sem.SemesterName AS [Semester],
                       s.RegistrationStatus AS [Status], s.CreatedDate AS [Registered Date]
                FROM dbo.Students s
                JOIN dbo.Colleges c ON s.CollegeId = c.CollegeId
                JOIN dbo.Programs p ON s.ProgramId = p.ProgramId
                JOIN dbo.Semesters sem ON s.CurrentSemesterId = sem.SemesterId
                WHERE s.IsActive = 1";

            var parameters = new List<SqlParameter>();
            if (collegeId.HasValue)
            {
                sql += " AND s.CollegeId = @CollegeId";
                parameters.Add(new SqlParameter("@CollegeId", collegeId.Value));
            }
            if (programId.HasValue)
            {
                sql += " AND s.ProgramId = @ProgramId";
                parameters.Add(new SqlParameter("@ProgramId", programId.Value));
            }
            sql += " ORDER BY s.FullName";

            return await _db.ExecuteDataTableAsync(sql, CommandType.Text, parameters.ToArray());
        }

        public async Task<int> AddStudentDocumentAsync(StudentDocument doc)
        {
            const string sql = @"
                INSERT INTO dbo.StudentDocuments (StudentId, DocumentType, DocumentName, FilePath, FileSizeKB, UploadedDate)
                VALUES (@StudentId, @DocumentType, @DocumentName, @FilePath, @FileSizeKB, GETDATE());
                SELECT SCOPE_IDENTITY();";

            var res = await _db.ExecuteScalarAsync(sql, CommandType.Text, new[]
            {
                new SqlParameter("@StudentId", doc.StudentId),
                new SqlParameter("@DocumentType", doc.DocumentType),
                new SqlParameter("@DocumentName", doc.DocumentName),
                new SqlParameter("@FilePath", doc.FilePath),
                new SqlParameter("@FileSizeKB", (object?)doc.FileSizeKB ?? DBNull.Value)
            });

            return Convert.ToInt32(res);
        }

        public async Task<List<StudentDocument>> GetStudentDocumentsAsync(int studentId)
        {
            const string sql = "SELECT DocumentId, StudentId, DocumentType, DocumentName, FilePath, FileSizeKB, UploadedDate FROM dbo.StudentDocuments WHERE StudentId = @StudentId ORDER BY UploadedDate DESC";
            return await _db.ExecuteReaderListAsync(sql, r => new StudentDocument
            {
                DocumentId = r.GetInt32(r.GetOrdinal("DocumentId")),
                StudentId = r.GetInt32(r.GetOrdinal("StudentId")),
                DocumentType = r.GetString(r.GetOrdinal("DocumentType")),
                DocumentName = r.GetString(r.GetOrdinal("DocumentName")),
                FilePath = r.GetString(r.GetOrdinal("FilePath")),
                FileSizeKB = r.IsDBNull(r.GetOrdinal("FileSizeKB")) ? null : r.GetInt64(r.GetOrdinal("FileSizeKB")),
                UploadedDate = r.GetDateTime(r.GetOrdinal("UploadedDate"))
            }, CommandType.Text, new[] { new SqlParameter("@StudentId", studentId) });
        }

        private static Student MapStudentProfile(SqlDataReader r)
        {
            return new Student
            {
                StudentId = r.GetInt32(r.GetOrdinal("StudentId")),
                StudentUniqueId = r.GetString(r.GetOrdinal("StudentUniqueId")),
                RegistrationNumber = r.IsDBNull(r.GetOrdinal("RegistrationNumber")) ? null : r.GetString(r.GetOrdinal("RegistrationNumber")),
                RollNumber = r.IsDBNull(r.GetOrdinal("RollNumber")) ? null : r.GetString(r.GetOrdinal("RollNumber")),
                FullName = r.GetString(r.GetOrdinal("FullName")),
                DateOfBirth = r.GetDateTime(r.GetOrdinal("DateOfBirth")),
                Gender = r.GetString(r.GetOrdinal("Gender")),
                Email = r.GetString(r.GetOrdinal("Email")),
                MobileNumber = r.GetString(r.GetOrdinal("MobileNumber")),
                Address = r.GetString(r.GetOrdinal("Address")),
                FacultyId = r.GetInt32(r.GetOrdinal("FacultyId")),
                FacultyName = r.IsDBNull(r.GetOrdinal("FacultyName")) ? null : r.GetString(r.GetOrdinal("FacultyName")),
                ProgramId = r.GetInt32(r.GetOrdinal("ProgramId")),
                ProgramName = r.IsDBNull(r.GetOrdinal("ProgramName")) ? null : r.GetString(r.GetOrdinal("ProgramName")),
                ProgramCode = r.IsDBNull(r.GetOrdinal("ProgramCode")) ? null : r.GetString(r.GetOrdinal("ProgramCode")),
                CollegeId = r.GetInt32(r.GetOrdinal("CollegeId")),
                CollegeName = r.IsDBNull(r.GetOrdinal("CollegeName")) ? null : r.GetString(r.GetOrdinal("CollegeName")),
                CollegeCode = r.IsDBNull(r.GetOrdinal("CollegeCode")) ? null : r.GetString(r.GetOrdinal("CollegeCode")),
                AcademicYearId = r.GetInt32(r.GetOrdinal("AcademicYearId")),
                AcademicYear = r.IsDBNull(r.GetOrdinal("AcademicYear")) ? null : r.GetString(r.GetOrdinal("AcademicYear")),
                CurrentSemesterId = r.GetInt32(r.GetOrdinal("CurrentSemesterId")),
                CurrentSemester = r.IsDBNull(r.GetOrdinal("CurrentSemester")) ? null : r.GetString(r.GetOrdinal("CurrentSemester")),
                PreviousQualification = r.IsDBNull(r.GetOrdinal("PreviousQualification")) ? null : r.GetString(r.GetOrdinal("PreviousQualification")),
                PhotoUrl = r.IsDBNull(r.GetOrdinal("PhotoUrl")) ? null : r.GetString(r.GetOrdinal("PhotoUrl")),
                IdentityDocumentUrl = r.IsDBNull(r.GetOrdinal("IdentityDocumentUrl")) ? null : r.GetString(r.GetOrdinal("IdentityDocumentUrl")),
                RegistrationStatus = r.GetString(r.GetOrdinal("RegistrationStatus")),
                VerificationRemarks = r.IsDBNull(r.GetOrdinal("VerificationRemarks")) ? null : r.GetString(r.GetOrdinal("VerificationRemarks")),
                VerifiedDate = r.IsDBNull(r.GetOrdinal("VerifiedDate")) ? null : r.GetDateTime(r.GetOrdinal("VerifiedDate")),
                CreatedDate = r.GetDateTime(r.GetOrdinal("CreatedDate")),
                ApplicationNumber = r.IsDBNull(r.GetOrdinal("ApplicationNumber")) ? null : r.GetString(r.GetOrdinal("ApplicationNumber")),
                RegistrationAppStatus = r.IsDBNull(r.GetOrdinal("RegistrationAppStatus")) ? null : r.GetString(r.GetOrdinal("RegistrationAppStatus"))
            };
        }
    }
}
