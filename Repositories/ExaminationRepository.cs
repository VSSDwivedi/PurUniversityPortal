using System.Data;
using Microsoft.Data.SqlClient;
using UniversityExamSystem.Data;
using UniversityExamSystem.Models;

namespace UniversityExamSystem.Repositories
{
    public interface IExaminationRepository
    {
        // Examinations
        Task<List<Examination>> GetExaminationsAsync(int? collegeId = null);
        Task<List<Examination>> GetActiveExaminationsAsync();
        Task<Examination?> GetExaminationByIdAsync(int examinationId);
        Task<int> AddExaminationAsync(Examination exam);
        Task<bool> UpdateExaminationAsync(Examination exam);
        Task<bool> UpdateExaminationStatusAsync(int examId, string status);

        // Examination Applications
        Task<(int applicationId, string appNumber)> SubmitApplicationAsync(int examId, int studentId, string examType, decimal feePaid, string? refNum, string? voucherUrl, List<int> subjectIds);
        Task<List<ExaminationApplication>> GetApplicationsAsync(int? examId = null, int? collegeId = null, int? programId = null, string? status = null, int? studentId = null);
        Task<ExaminationApplication?> GetApplicationByIdAsync(int applicationId);
        Task<ExaminationApplication?> GetApplicationByStudentAndExamAsync(int studentId, int examId);
        Task<bool> UpdateApplicationStatusAsync(int applicationId, string status, string? remarks, int? reviewedBy);

        // Admit Cards
        Task<(int admitCardId, string admitCardNumber)> GenerateAdmitCardAsync(int applicationId, string rollNumber, int centerId, int? generatedBy);
        Task<AdmitCard?> GetAdmitCardByIdAsync(int admitCardId);
        Task<AdmitCard?> GetAdmitCardByApplicationIdAsync(int applicationId);
        Task<AdmitCard?> GetAdmitCardByStudentAndExamAsync(int studentId, int examId);
        Task<List<AdmitCard>> GetAdmitCardsForExamAsync(int examId, int? collegeId = null);
        Task RecordAdmitCardDownloadAsync(int admitCardId);
        Task<int> BulkGenerateAdmitCardsAsync(int examId, int? collegeId = null, int? defaultCenterId = null, int? generatedBy = null);
    }

    public class ExaminationRepository : IExaminationRepository
    {
        private readonly DatabaseHelper _db;

        public ExaminationRepository(DatabaseHelper db)
        {
            _db = db;
        }

        // ======================== EXAMINATIONS ========================
        public async Task<List<Examination>> GetExaminationsAsync(int? collegeId = null)
        {
            var sql = @"
        SELECT e.ExaminationId, e.ExamCode, e.ExamName, e.AcademicYearId, ay.YearName AS AcademicYearName,
               e.SemesterId, sem.SemesterName, e.FacultyId, f.FacultyName, e.ProgramId, p.ProgramName,
               e.ExamType, e.StartDate, e.EndDate, e.ApplicationDeadline, e.LateFeeDeadline,
               e.FeeAmount, e.LateFeeAmount, e.DefaultCenterId, c.CenterName AS DefaultCenterName,
               e.Status, e.IsActive,
               (SELECT COUNT(*) FROM dbo.ExaminationApplications ea
                WHERE ea.ExaminationId = e.ExaminationId
                  AND (@CollegeId IS NULL OR ea.CollegeId = @CollegeId)) AS TotalApplicants
        FROM dbo.Examinations e
        JOIN dbo.AcademicYears ay ON e.AcademicYearId = ay.AcademicYearId
        JOIN dbo.Semesters sem ON e.SemesterId = sem.SemesterId
        LEFT JOIN dbo.Faculties f ON e.FacultyId = f.FacultyId
        LEFT JOIN dbo.Programs p ON e.ProgramId = p.ProgramId
        LEFT JOIN dbo.ExaminationCenters c ON e.DefaultCenterId = c.CenterId
        ORDER BY e.StartDate DESC";

            var parameters = new[]
            {
        new SqlParameter("@CollegeId", (object?)collegeId ?? DBNull.Value)
    };

            return await _db.ExecuteReaderListAsync(sql, MapExamination, CommandType.Text, parameters);
        }
        public async Task<List<Examination>> GetActiveExaminationsAsync()
        {
            const string sql = @"
                SELECT e.ExaminationId, e.ExamCode, e.ExamName, e.AcademicYearId, ay.YearName AS AcademicYearName,
                       e.SemesterId, sem.SemesterName, e.FacultyId, f.FacultyName, e.ProgramId, p.ProgramName,
                       e.ExamType, e.StartDate, e.EndDate, e.ApplicationDeadline, e.LateFeeDeadline,
                       e.FeeAmount, e.LateFeeAmount, e.DefaultCenterId, c.CenterName AS DefaultCenterName,
                       e.Status, e.IsActive, 0 AS TotalApplicants
                FROM dbo.Examinations e
                JOIN dbo.AcademicYears ay ON e.AcademicYearId = ay.AcademicYearId
                JOIN dbo.Semesters sem ON e.SemesterId = sem.SemesterId
                LEFT JOIN dbo.Faculties f ON e.FacultyId = f.FacultyId
                LEFT JOIN dbo.Programs p ON e.ProgramId = p.ProgramId
                LEFT JOIN dbo.ExaminationCenters c ON e.DefaultCenterId = c.CenterId
                WHERE e.IsActive = 1 AND e.Status IN ('Upcoming', 'Ongoing')
                ORDER BY e.StartDate ASC";

            return await _db.ExecuteReaderListAsync(sql, MapExamination);
        }

        public async Task<Examination?> GetExaminationByIdAsync(int examinationId)
        {
            const string sql = @"
                SELECT e.ExaminationId, e.ExamCode, e.ExamName, e.AcademicYearId, ay.YearName AS AcademicYearName,
                       e.SemesterId, sem.SemesterName, e.FacultyId, f.FacultyName, e.ProgramId, p.ProgramName,
                       e.ExamType, e.StartDate, e.EndDate, e.ApplicationDeadline, e.LateFeeDeadline,
                       e.FeeAmount, e.LateFeeAmount, e.DefaultCenterId, c.CenterName AS DefaultCenterName,
                       e.Status, e.IsActive,
                       (SELECT COUNT(*) FROM dbo.ExaminationApplications ea WHERE ea.ExaminationId = e.ExaminationId) AS TotalApplicants
                FROM dbo.Examinations e
                JOIN dbo.AcademicYears ay ON e.AcademicYearId = ay.AcademicYearId
                JOIN dbo.Semesters sem ON e.SemesterId = sem.SemesterId
                LEFT JOIN dbo.Faculties f ON e.FacultyId = f.FacultyId
                LEFT JOIN dbo.Programs p ON e.ProgramId = p.ProgramId
                LEFT JOIN dbo.ExaminationCenters c ON e.DefaultCenterId = c.CenterId
                WHERE e.ExaminationId = @Id";

            return await _db.ExecuteReaderSingleAsync(sql, MapExamination, CommandType.Text, new[] { new SqlParameter("@Id", examinationId) });
        }

        public async Task<int> AddExaminationAsync(Examination exam)
        {
            const string sql = @"
                INSERT INTO dbo.Examinations (
                    ExamCode, ExamName, AcademicYearId, SemesterId, FacultyId, ProgramId, ExamType,
                    StartDate, EndDate, ApplicationDeadline, LateFeeDeadline, FeeAmount, LateFeeAmount,
                    DefaultCenterId, Status, IsActive, CreatedDate
                )
                VALUES (
                    @Code, @Name, @AcadYear, @Semester, @Faculty, @Program, @ExamType,
                    @StartDate, @EndDate, @AppDeadline, @LateDeadline, @Fee, @LateFee,
                    @Center, @Status, @IsActive, GETDATE()
                );
                SELECT SCOPE_IDENTITY();";

            var result = await _db.ExecuteScalarAsync(sql, CommandType.Text, new[]
            {
                new SqlParameter("@Code", exam.ExamCode),
                new SqlParameter("@Name", exam.ExamName),
                new SqlParameter("@AcadYear", exam.AcademicYearId),
                new SqlParameter("@Semester", exam.SemesterId),
                new SqlParameter("@Faculty", (object?)exam.FacultyId ?? DBNull.Value),
                new SqlParameter("@Program", (object?)exam.ProgramId ?? DBNull.Value),
                new SqlParameter("@ExamType", exam.ExamType),
                new SqlParameter("@StartDate", exam.StartDate),
                new SqlParameter("@EndDate", exam.EndDate),
                new SqlParameter("@AppDeadline", exam.ApplicationDeadline),
                new SqlParameter("@LateDeadline", (object?)exam.LateFeeDeadline ?? DBNull.Value),
                new SqlParameter("@Fee", exam.FeeAmount),
                new SqlParameter("@LateFee", exam.LateFeeAmount),
                new SqlParameter("@Center", (object?)exam.DefaultCenterId ?? DBNull.Value),
                new SqlParameter("@Status", exam.Status),
                new SqlParameter("@IsActive", exam.IsActive)
            });
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateExaminationAsync(Examination exam)
        {
            const string sql = @"
                UPDATE dbo.Examinations
                SET ExamCode = @Code, ExamName = @Name, AcademicYearId = @AcadYear, SemesterId = @Semester,
                    FacultyId = @Faculty, ProgramId = @Program, ExamType = @ExamType,
                    StartDate = @StartDate, EndDate = @EndDate, ApplicationDeadline = @AppDeadline,
                    LateFeeDeadline = @LateDeadline, FeeAmount = @Fee, LateFeeAmount = @LateFee,
                    DefaultCenterId = @Center, Status = @Status, IsActive = @IsActive, UpdatedDate = GETDATE()
                WHERE ExaminationId = @Id";

            var rows = await _db.ExecuteNonQueryAsync(sql, CommandType.Text, new[]
            {
                new SqlParameter("@Code", exam.ExamCode),
                new SqlParameter("@Name", exam.ExamName),
                new SqlParameter("@AcadYear", exam.AcademicYearId),
                new SqlParameter("@Semester", exam.SemesterId),
                new SqlParameter("@Faculty", (object?)exam.FacultyId ?? DBNull.Value),
                new SqlParameter("@Program", (object?)exam.ProgramId ?? DBNull.Value),
                new SqlParameter("@ExamType", exam.ExamType),
                new SqlParameter("@StartDate", exam.StartDate),
                new SqlParameter("@EndDate", exam.EndDate),
                new SqlParameter("@AppDeadline", exam.ApplicationDeadline),
                new SqlParameter("@LateDeadline", (object?)exam.LateFeeDeadline ?? DBNull.Value),
                new SqlParameter("@Fee", exam.FeeAmount),
                new SqlParameter("@LateFee", exam.LateFeeAmount),
                new SqlParameter("@Center", (object?)exam.DefaultCenterId ?? DBNull.Value),
                new SqlParameter("@Status", exam.Status),
                new SqlParameter("@IsActive", exam.IsActive),
                new SqlParameter("@Id", exam.ExaminationId)
            });
            return rows > 0;
        }

        public async Task<bool> UpdateExaminationStatusAsync(int examId, string status)
        {
            const string sql = "UPDATE dbo.Examinations SET Status = @Status, UpdatedDate = GETDATE() WHERE ExaminationId = @Id";
            var rows = await _db.ExecuteNonQueryAsync(sql, CommandType.Text, new[]
            {
                new SqlParameter("@Status", status),
                new SqlParameter("@Id", examId)
            });
            return rows > 0;
        }

        // ======================== APPLICATIONS ========================
        public async Task<(int applicationId, string appNumber)> SubmitApplicationAsync(int examId, int studentId, string examType, decimal feePaid, string? refNum, string? voucherUrl, List<int> subjectIds)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            using var cmd = new SqlCommand("dbo.sp_SubmitExaminationApplication", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@ExaminationId", examId);
            cmd.Parameters.AddWithValue("@StudentId", studentId);
            cmd.Parameters.AddWithValue("@ExamType", examType);
            cmd.Parameters.AddWithValue("@FeePaid", feePaid);
            cmd.Parameters.AddWithValue("@PaymentReference", (object?)refNum ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PaymentVoucherUrl", (object?)voucherUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SubjectIdsList", string.Join(",", subjectIds));

            var pAppId = new SqlParameter("@ApplicationId", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var pAppNum = new SqlParameter("@ApplicationNumber", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(pAppId);
            cmd.Parameters.Add(pAppNum);

            await cmd.ExecuteNonQueryAsync();

            return ((int)pAppId.Value, (string)pAppNum.Value);
        }

        public async Task<List<ExaminationApplication>> GetApplicationsAsync(int? examId = null, int? collegeId = null, int? programId = null, string? status = null, int? studentId = null)
        {
            var sql = @"
                SELECT ea.ApplicationId, ea.ApplicationNumber, ea.ExaminationId, e.ExamName,
                       ea.StudentId, s.FullName AS StudentName, s.RegistrationNumber,
                       ea.CollegeId, c.CollegeName, ea.ProgramId, p.ProgramName,
                       ea.SemesterId, sem.SemesterName, ea.ExamType, ea.AssignedRollNumber,
                       ea.ExaminationCenterId, ctr.CenterName, ea.FeePaid, ea.PaymentReference,
                       ea.PaymentVoucherUrl, ea.Status, ea.Remarks, ea.SubmissionDate,
                       ea.ReviewedBy, ea.ReviewedDate
                FROM dbo.ExaminationApplications ea
                JOIN dbo.Examinations e ON ea.ExaminationId = e.ExaminationId
                JOIN dbo.Students s ON ea.StudentId = s.StudentId
                JOIN dbo.Colleges c ON ea.CollegeId = c.CollegeId
                JOIN dbo.Programs p ON ea.ProgramId = p.ProgramId
                JOIN dbo.Semesters sem ON ea.SemesterId = sem.SemesterId
                LEFT JOIN dbo.ExaminationCenters ctr ON ea.ExaminationCenterId = ctr.CenterId
                WHERE 1=1";

            var parameters = new List<SqlParameter>();
            if (examId.HasValue)
            {
                sql += " AND ea.ExaminationId = @ExamId";
                parameters.Add(new SqlParameter("@ExamId", examId.Value));
            }
            if (collegeId.HasValue)
            {
                sql += " AND ea.CollegeId = @CollegeId";
                parameters.Add(new SqlParameter("@CollegeId", collegeId.Value));
            }
            if (programId.HasValue)
            {
                sql += " AND ea.ProgramId = @ProgramId";
                parameters.Add(new SqlParameter("@ProgramId", programId.Value));
            }
            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " AND ea.Status = @Status";
                parameters.Add(new SqlParameter("@Status", status));
            }
            if (studentId.HasValue)
            {
                sql += " AND ea.StudentId = @StudentId";
                parameters.Add(new SqlParameter("@StudentId", studentId.Value));
            }

            sql += " ORDER BY ea.SubmissionDate DESC";

            return await _db.ExecuteReaderListAsync(sql, MapApplication, CommandType.Text, parameters.ToArray());
        }

        public async Task<ExaminationApplication?> GetApplicationByIdAsync(int applicationId)
        {
            const string sql = @"
                SELECT ea.ApplicationId, ea.ApplicationNumber, ea.ExaminationId, e.ExamName,
                       ea.StudentId, s.FullName AS StudentName, s.RegistrationNumber,
                       ea.CollegeId, c.CollegeName, ea.ProgramId, p.ProgramName,
                       ea.SemesterId, sem.SemesterName, ea.ExamType, ea.AssignedRollNumber,
                       ea.ExaminationCenterId, ctr.CenterName, ea.FeePaid, ea.PaymentReference,
                       ea.PaymentVoucherUrl, ea.Status, ea.Remarks, ea.SubmissionDate,
                       ea.ReviewedBy, ea.ReviewedDate
                FROM dbo.ExaminationApplications ea
                JOIN dbo.Examinations e ON ea.ExaminationId = e.ExaminationId
                JOIN dbo.Students s ON ea.StudentId = s.StudentId
                JOIN dbo.Colleges c ON ea.CollegeId = c.CollegeId
                JOIN dbo.Programs p ON ea.ProgramId = p.ProgramId
                JOIN dbo.Semesters sem ON ea.SemesterId = sem.SemesterId
                LEFT JOIN dbo.ExaminationCenters ctr ON ea.ExaminationCenterId = ctr.CenterId
                WHERE ea.ApplicationId = @Id";

            var app = await _db.ExecuteReaderSingleAsync(sql, MapApplication, CommandType.Text, new[] { new SqlParameter("@Id", applicationId) });
            if (app != null)
            {
                app.SelectedSubjects = await GetApplicationSubjectsAsync(app.ApplicationId);
            }
            return app;
        }

        public async Task<ExaminationApplication?> GetApplicationByStudentAndExamAsync(int studentId, int examId)
        {
            const string sql = @"
                SELECT ea.ApplicationId, ea.ApplicationNumber, ea.ExaminationId, e.ExamName,
                       ea.StudentId, s.FullName AS StudentName, s.RegistrationNumber,
                       ea.CollegeId, c.CollegeName, ea.ProgramId, p.ProgramName,
                       ea.SemesterId, sem.SemesterName, ea.ExamType, ea.AssignedRollNumber,
                       ea.ExaminationCenterId, ctr.CenterName, ea.FeePaid, ea.PaymentReference,
                       ea.PaymentVoucherUrl, ea.Status, ea.Remarks, ea.SubmissionDate,
                       ea.ReviewedBy, ea.ReviewedDate
                FROM dbo.ExaminationApplications ea
                JOIN dbo.Examinations e ON ea.ExaminationId = e.ExaminationId
                JOIN dbo.Students s ON ea.StudentId = s.StudentId
                JOIN dbo.Colleges c ON ea.CollegeId = c.CollegeId
                JOIN dbo.Programs p ON ea.ProgramId = p.ProgramId
                JOIN dbo.Semesters sem ON ea.SemesterId = sem.SemesterId
                LEFT JOIN dbo.ExaminationCenters ctr ON ea.ExaminationCenterId = ctr.CenterId
                WHERE ea.StudentId = @StudentId AND ea.ExaminationId = @ExamId";

            var app = await _db.ExecuteReaderSingleAsync(sql, MapApplication, CommandType.Text, new[]
            {
                new SqlParameter("@StudentId", studentId),
                new SqlParameter("@ExamId", examId)
            });

            if (app != null)
            {
                app.SelectedSubjects = await GetApplicationSubjectsAsync(app.ApplicationId);
            }
            return app;
        }

        public async Task<bool> UpdateApplicationStatusAsync(int applicationId, string status, string? remarks, int? reviewedBy)
        {
            const string sql = @"
                UPDATE dbo.ExaminationApplications
                SET Status = @Status, Remarks = @Remarks, ReviewedBy = @ReviewedBy,
                    ReviewedDate = GETDATE(), UpdatedDate = GETDATE()
                WHERE ApplicationId = @Id";

            var rows = await _db.ExecuteNonQueryAsync(sql, CommandType.Text, new[]
            {
                new SqlParameter("@Status", status),
                new SqlParameter("@Remarks", (object?)remarks ?? DBNull.Value),
                new SqlParameter("@ReviewedBy", (object?)reviewedBy ?? DBNull.Value),
                new SqlParameter("@Id", applicationId)
            });
            return rows > 0;
        }

        private async Task<List<ExaminationApplicationSubject>> GetApplicationSubjectsAsync(int applicationId)
        {
            const string sql = @"
                SELECT eas.AppSubjectId, eas.ApplicationId, eas.SubjectId, sub.SubjectCode, sub.SubjectName,
                       sub.CreditHours, eas.SubjectType, eas.ExamDate, eas.ExamTime
                FROM dbo.ExaminationApplicationSubjects eas
                JOIN dbo.Subjects sub ON eas.SubjectId = sub.SubjectId
                WHERE eas.ApplicationId = @AppId";

            return await _db.ExecuteReaderListAsync(sql, r => new ExaminationApplicationSubject
            {
                AppSubjectId = r.GetInt32(r.GetOrdinal("AppSubjectId")),
                ApplicationId = r.GetInt32(r.GetOrdinal("ApplicationId")),
                SubjectId = r.GetInt32(r.GetOrdinal("SubjectId")),
                SubjectCode = r.GetString(r.GetOrdinal("SubjectCode")),
                SubjectName = r.GetString(r.GetOrdinal("SubjectName")),
                CreditHours = r.GetInt32(r.GetOrdinal("CreditHours")),
                SubjectType = r.GetString(r.GetOrdinal("SubjectType")),
                ExamDate = r.IsDBNull(r.GetOrdinal("ExamDate")) ? null : r.GetDateTime(r.GetOrdinal("ExamDate")),
                ExamTime = r.IsDBNull(r.GetOrdinal("ExamTime")) ? null : r.GetString(r.GetOrdinal("ExamTime"))
            }, CommandType.Text, new[] { new SqlParameter("@AppId", applicationId) });
        }

        // ======================== ADMIT CARDS ========================
        public async Task<(int admitCardId, string admitCardNumber)> GenerateAdmitCardAsync(int applicationId, string rollNumber, int centerId, int? generatedBy)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            using var cmd = new SqlCommand("dbo.sp_GenerateAdmitCard", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@ApplicationId", applicationId);
            cmd.Parameters.AddWithValue("@AssignedRollNumber", rollNumber);
            cmd.Parameters.AddWithValue("@ExaminationCenterId", centerId);
            cmd.Parameters.AddWithValue("@GeneratedBy", (object?)generatedBy ?? DBNull.Value);

            var pCardId = new SqlParameter("@AdmitCardId", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var pCardNum = new SqlParameter("@AdmitCardNumber", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(pCardId);
            cmd.Parameters.Add(pCardNum);

            await cmd.ExecuteNonQueryAsync();

            return ((int)pCardId.Value, (string)pCardNum.Value);
        }

        public async Task<AdmitCard?> GetAdmitCardByIdAsync(int admitCardId)
        {
            const string sql = @"
                SELECT ac.AdmitCardId, ac.AdmitCardNumber, ac.ApplicationId, ac.StudentId, s.FullName AS StudentName,
                       s.PhotoUrl, ac.ExaminationId, e.ExamName, ac.RollNumber, ac.RegistrationNumber,
                       ac.ExaminationCenterId, ctr.CenterName, ctr.Location AS CenterLocation,
                       p.ProgramName, f.FacultyName, col.CollegeName, sem.SemesterName,
                       ac.QrCodeHash, ac.GeneratedDate, ac.IsDownloaded, ac.DownloadCount
                FROM dbo.AdmitCards ac
                JOIN dbo.ExaminationApplications ea ON ac.ApplicationId = ea.ApplicationId
                JOIN dbo.Students s ON ac.StudentId = s.StudentId
                JOIN dbo.Examinations e ON ac.ExaminationId = e.ExaminationId
                JOIN dbo.ExaminationCenters ctr ON ac.ExaminationCenterId = ctr.CenterId
                JOIN dbo.Programs p ON s.ProgramId = p.ProgramId
                JOIN dbo.Faculties f ON p.FacultyId = f.FacultyId
                JOIN dbo.Colleges col ON s.CollegeId = col.CollegeId
                JOIN dbo.Semesters sem ON ea.SemesterId = sem.SemesterId
                WHERE ac.AdmitCardId = @Id";

            var card = await _db.ExecuteReaderSingleAsync(sql, MapAdmitCard, CommandType.Text, new[] { new SqlParameter("@Id", admitCardId) });
            if (card != null)
            {
                card.Subjects = await GetApplicationSubjectsAsync(card.ApplicationId);
            }
            return card;
        }

        public async Task<AdmitCard?> GetAdmitCardByApplicationIdAsync(int applicationId)
        {
            const string sql = "SELECT TOP 1 AdmitCardId FROM dbo.AdmitCards WHERE ApplicationId = @AppId";
            var idObj = await _db.ExecuteScalarAsync(sql, CommandType.Text, new[] { new SqlParameter("@AppId", applicationId) });
            if (idObj != null && idObj != DBNull.Value)
            {
                return await GetAdmitCardByIdAsync(Convert.ToInt32(idObj));
            }
            return null;
        }

        public async Task<AdmitCard?> GetAdmitCardByStudentAndExamAsync(int studentId, int examId)
        {
            const string sql = "SELECT TOP 1 AdmitCardId FROM dbo.AdmitCards WHERE StudentId = @StudentId AND ExaminationId = @ExamId";
            var idObj = await _db.ExecuteScalarAsync(sql, CommandType.Text, new[]
            {
                new SqlParameter("@StudentId", studentId),
                new SqlParameter("@ExamId", examId)
            });

            if (idObj != null && idObj != DBNull.Value)
            {
                return await GetAdmitCardByIdAsync(Convert.ToInt32(idObj));
            }
            return null;
        }

        public async Task<List<AdmitCard>> GetAdmitCardsForExamAsync(int examId, int? collegeId = null)
        {
            var sql = @"
                SELECT ac.AdmitCardId, ac.AdmitCardNumber, ac.ApplicationId, ac.StudentId, s.FullName AS StudentName,
                       s.PhotoUrl, ac.ExaminationId, e.ExamName, ac.RollNumber, ac.RegistrationNumber,
                       ac.ExaminationCenterId, ctr.CenterName, ctr.Location AS CenterLocation,
                       p.ProgramName, f.FacultyName, col.CollegeName, sem.SemesterName,
                       ac.QrCodeHash, ac.GeneratedDate, ac.IsDownloaded, ac.DownloadCount
                FROM dbo.AdmitCards ac
                JOIN dbo.ExaminationApplications ea ON ac.ApplicationId = ea.ApplicationId
                JOIN dbo.Students s ON ac.StudentId = s.StudentId
                JOIN dbo.Examinations e ON ac.ExaminationId = e.ExaminationId
                JOIN dbo.ExaminationCenters ctr ON ac.ExaminationCenterId = ctr.CenterId
                JOIN dbo.Programs p ON s.ProgramId = p.ProgramId
                JOIN dbo.Faculties f ON p.FacultyId = f.FacultyId
                JOIN dbo.Colleges col ON s.CollegeId = col.CollegeId
                JOIN dbo.Semesters sem ON ea.SemesterId = sem.SemesterId
                WHERE ac.ExaminationId = @ExamId";

            var parameters = new List<SqlParameter> { new SqlParameter("@ExamId", examId) };
            if (collegeId.HasValue)
            {
                sql += " AND s.CollegeId = @CollegeId";
                parameters.Add(new SqlParameter("@CollegeId", collegeId.Value));
            }
            sql += " ORDER BY ac.RollNumber";

            return await _db.ExecuteReaderListAsync(sql, MapAdmitCard, CommandType.Text, parameters.ToArray());
        }

        public async Task RecordAdmitCardDownloadAsync(int admitCardId)
        {
            const string sql = @"
                UPDATE dbo.AdmitCards
                SET IsDownloaded = 1, DownloadCount = DownloadCount + 1, LastDownloadedDate = GETDATE()
                WHERE AdmitCardId = @Id";

            await _db.ExecuteNonQueryAsync(sql, CommandType.Text, new[] { new SqlParameter("@Id", admitCardId) });
        }

        public async Task<int> BulkGenerateAdmitCardsAsync(int examId, int? collegeId = null, int? defaultCenterId = null, int? generatedBy = null)
        {
            var sql = @"
                SELECT ea.ApplicationId, ea.StudentId, ISNULL(s.RollNumber, '') AS RollNumber
                FROM dbo.ExaminationApplications ea
                JOIN dbo.Students s ON ea.StudentId = s.StudentId
                WHERE ea.ExaminationId = @ExamId
                  AND ea.ApplicationId NOT IN (SELECT ApplicationId FROM dbo.AdmitCards)";

            var parameters = new List<SqlParameter> { new SqlParameter("@ExamId", examId) };
            if (collegeId.HasValue)
            {
                sql += " AND ea.CollegeId = @CollegeId";
                parameters.Add(new SqlParameter("@CollegeId", collegeId.Value));
            }

            var centerId = defaultCenterId ?? 1; // Default to Center 1 if none provided

            var applications = await _db.ExecuteReaderListAsync(sql, r => new
            {
                AppId = r.GetInt32(r.GetOrdinal("ApplicationId")),
                StudentId = r.GetInt32(r.GetOrdinal("StudentId")),
                RollNo = r.GetString(r.GetOrdinal("RollNumber"))
            }, CommandType.Text, parameters.ToArray());

            int count = 0;
            int rollSeq = 1000 + applications.Count;
            foreach (var app in applications)
            {
                var roll = string.IsNullOrWhiteSpace(app.RollNo) ? $"24{examId}{app.StudentId:D4}" : app.RollNo;
                await GenerateAdmitCardAsync(app.AppId, roll, centerId, generatedBy);
                count++;
            }

            return count;
        }

        private static Examination MapExamination(SqlDataReader r)
        {
            return new Examination
            {
                ExaminationId = r.GetInt32(r.GetOrdinal("ExaminationId")),
                ExamCode = r.GetString(r.GetOrdinal("ExamCode")),
                ExamName = r.GetString(r.GetOrdinal("ExamName")),
                AcademicYearId = r.GetInt32(r.GetOrdinal("AcademicYearId")),
                AcademicYearName = r.IsDBNull(r.GetOrdinal("AcademicYearName")) ? null : r.GetString(r.GetOrdinal("AcademicYearName")),
                SemesterId = r.GetInt32(r.GetOrdinal("SemesterId")),
                SemesterName = r.IsDBNull(r.GetOrdinal("SemesterName")) ? null : r.GetString(r.GetOrdinal("SemesterName")),
                FacultyId = r.IsDBNull(r.GetOrdinal("FacultyId")) ? null : r.GetInt32(r.GetOrdinal("FacultyId")),
                FacultyName = r.IsDBNull(r.GetOrdinal("FacultyName")) ? null : r.GetString(r.GetOrdinal("FacultyName")),
                ProgramId = r.IsDBNull(r.GetOrdinal("ProgramId")) ? null : r.GetInt32(r.GetOrdinal("ProgramId")),
                ProgramName = r.IsDBNull(r.GetOrdinal("ProgramName")) ? null : r.GetString(r.GetOrdinal("ProgramName")),
                ExamType = r.GetString(r.GetOrdinal("ExamType")),
                StartDate = r.GetDateTime(r.GetOrdinal("StartDate")),
                EndDate = r.GetDateTime(r.GetOrdinal("EndDate")),
                ApplicationDeadline = r.GetDateTime(r.GetOrdinal("ApplicationDeadline")),
                LateFeeDeadline = r.IsDBNull(r.GetOrdinal("LateFeeDeadline")) ? null : r.GetDateTime(r.GetOrdinal("LateFeeDeadline")),
                FeeAmount = r.GetDecimal(r.GetOrdinal("FeeAmount")),
                LateFeeAmount = r.GetDecimal(r.GetOrdinal("LateFeeAmount")),
                DefaultCenterId = r.IsDBNull(r.GetOrdinal("DefaultCenterId")) ? null : r.GetInt32(r.GetOrdinal("DefaultCenterId")),
                DefaultCenterName = r.IsDBNull(r.GetOrdinal("DefaultCenterName")) ? null : r.GetString(r.GetOrdinal("DefaultCenterName")),
                Status = r.GetString(r.GetOrdinal("Status")),
                IsActive = r.GetBoolean(r.GetOrdinal("IsActive")),
                TotalApplicants = r.GetInt32(r.GetOrdinal("TotalApplicants"))
            };
        }

        private static ExaminationApplication MapApplication(SqlDataReader r)
        {
            return new ExaminationApplication
            {
                ApplicationId = r.GetInt32(r.GetOrdinal("ApplicationId")),
                ApplicationNumber = r.GetString(r.GetOrdinal("ApplicationNumber")),
                ExaminationId = r.GetInt32(r.GetOrdinal("ExaminationId")),
                ExamName = r.IsDBNull(r.GetOrdinal("ExamName")) ? null : r.GetString(r.GetOrdinal("ExamName")),
                StudentId = r.GetInt32(r.GetOrdinal("StudentId")),
                StudentName = r.IsDBNull(r.GetOrdinal("StudentName")) ? null : r.GetString(r.GetOrdinal("StudentName")),
                RegistrationNumber = r.IsDBNull(r.GetOrdinal("RegistrationNumber")) ? null : r.GetString(r.GetOrdinal("RegistrationNumber")),
                CollegeId = r.GetInt32(r.GetOrdinal("CollegeId")),
                CollegeName = r.IsDBNull(r.GetOrdinal("CollegeName")) ? null : r.GetString(r.GetOrdinal("CollegeName")),
                ProgramId = r.GetInt32(r.GetOrdinal("ProgramId")),
                ProgramName = r.IsDBNull(r.GetOrdinal("ProgramName")) ? null : r.GetString(r.GetOrdinal("ProgramName")),
                SemesterId = r.GetInt32(r.GetOrdinal("SemesterId")),
                SemesterName = r.IsDBNull(r.GetOrdinal("SemesterName")) ? null : r.GetString(r.GetOrdinal("SemesterName")),
                ExamType = r.GetString(r.GetOrdinal("ExamType")),
                AssignedRollNumber = r.IsDBNull(r.GetOrdinal("AssignedRollNumber")) ? null : r.GetString(r.GetOrdinal("AssignedRollNumber")),
                ExaminationCenterId = r.IsDBNull(r.GetOrdinal("ExaminationCenterId")) ? null : r.GetInt32(r.GetOrdinal("ExaminationCenterId")),
                CenterName = r.IsDBNull(r.GetOrdinal("CenterName")) ? null : r.GetString(r.GetOrdinal("CenterName")),
                FeePaid = r.GetDecimal(r.GetOrdinal("FeePaid")),
                PaymentReference = r.IsDBNull(r.GetOrdinal("PaymentReference")) ? null : r.GetString(r.GetOrdinal("PaymentReference")),
                PaymentVoucherUrl = r.IsDBNull(r.GetOrdinal("PaymentVoucherUrl")) ? null : r.GetString(r.GetOrdinal("PaymentVoucherUrl")),
                Status = r.GetString(r.GetOrdinal("Status")),
                Remarks = r.IsDBNull(r.GetOrdinal("Remarks")) ? null : r.GetString(r.GetOrdinal("Remarks")),
                SubmissionDate = r.GetDateTime(r.GetOrdinal("SubmissionDate")),
                ReviewedBy = r.IsDBNull(r.GetOrdinal("ReviewedBy")) ? null : r.GetInt32(r.GetOrdinal("ReviewedBy")),
                ReviewedDate = r.IsDBNull(r.GetOrdinal("ReviewedDate")) ? null : r.GetDateTime(r.GetOrdinal("ReviewedDate"))
            };
        }

        private static AdmitCard MapAdmitCard(SqlDataReader r)
        {
            return new AdmitCard
            {
                AdmitCardId = r.GetInt32(r.GetOrdinal("AdmitCardId")),
                AdmitCardNumber = r.GetString(r.GetOrdinal("AdmitCardNumber")),
                ApplicationId = r.GetInt32(r.GetOrdinal("ApplicationId")),
                StudentId = r.GetInt32(r.GetOrdinal("StudentId")),
                StudentName = r.IsDBNull(r.GetOrdinal("StudentName")) ? null : r.GetString(r.GetOrdinal("StudentName")),
                PhotoUrl = r.IsDBNull(r.GetOrdinal("PhotoUrl")) ? null : r.GetString(r.GetOrdinal("PhotoUrl")),
                ExaminationId = r.GetInt32(r.GetOrdinal("ExaminationId")),
                ExamName = r.IsDBNull(r.GetOrdinal("ExamName")) ? null : r.GetString(r.GetOrdinal("ExamName")),
                RollNumber = r.GetString(r.GetOrdinal("RollNumber")),
                RegistrationNumber = r.GetString(r.GetOrdinal("RegistrationNumber")),
                ExaminationCenterId = r.GetInt32(r.GetOrdinal("ExaminationCenterId")),
                CenterName = r.IsDBNull(r.GetOrdinal("CenterName")) ? null : r.GetString(r.GetOrdinal("CenterName")),
                CenterLocation = r.IsDBNull(r.GetOrdinal("CenterLocation")) ? null : r.GetString(r.GetOrdinal("CenterLocation")),
                ProgramName = r.IsDBNull(r.GetOrdinal("ProgramName")) ? null : r.GetString(r.GetOrdinal("ProgramName")),
                FacultyName = r.IsDBNull(r.GetOrdinal("FacultyName")) ? null : r.GetString(r.GetOrdinal("FacultyName")),
                CollegeName = r.IsDBNull(r.GetOrdinal("CollegeName")) ? null : r.GetString(r.GetOrdinal("CollegeName")),
                SemesterName = r.IsDBNull(r.GetOrdinal("SemesterName")) ? null : r.GetString(r.GetOrdinal("SemesterName")),
                QrCodeHash = r.GetString(r.GetOrdinal("QrCodeHash")),
                GeneratedDate = r.GetDateTime(r.GetOrdinal("GeneratedDate")),
                IsDownloaded = r.GetBoolean(r.GetOrdinal("IsDownloaded")),
                DownloadCount = r.GetInt32(r.GetOrdinal("DownloadCount"))
            };
        }
    }
}
