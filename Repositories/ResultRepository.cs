using System.Data;
using Microsoft.Data.SqlClient;
using UniversityExamSystem.Data;
using UniversityExamSystem.Models;

namespace UniversityExamSystem.Repositories
{
    public interface IResultRepository
    {
        Task<bool> ProcessResultsAsync(int examId, int? processedBy);
        Task<bool> PublishResultsAsync(int examId, int? publishedBy, bool isPublished = true);
        Task<Result?> SearchResultAsync(string searchTerm, int? examId = null);
        Task<Result?> GetResultByIdAsync(int resultId);
        Task<Result?> GetResultByStudentAndExamAsync(int studentId, int examId);
        Task<List<Result>> GetResultsByExamAsync(int examId, int? collegeId = null, int? programId = null);
        Task<List<Result>> GetStudentResultsHistoryAsync(int studentId);
        Task<DataTable> ExportResultsToDataTableAsync(int examId, int? collegeId = null, int? programId = null);
    }

    public class ResultRepository : IResultRepository
    {
        private readonly DatabaseHelper _db;

        public ResultRepository(DatabaseHelper db)
        {
            _db = db;
        }

        public async Task<bool> ProcessResultsAsync(int examId, int? processedBy)
        {
            var parameters = new[]
            {
                new SqlParameter("@ExaminationId", examId),
                new SqlParameter("@ProcessedBy", (object?)processedBy ?? DBNull.Value)
            };

            await _db.ExecuteNonQueryAsync("dbo.sp_ProcessExaminationResults", CommandType.StoredProcedure, parameters);
            return true;
        }

        public async Task<bool> PublishResultsAsync(int examId, int? publishedBy, bool isPublished = true)
        {
            var parameters = new[]
            {
                new SqlParameter("@ExaminationId", examId),
                new SqlParameter("@PublishedBy", (object?)publishedBy ?? DBNull.Value),
                new SqlParameter("@IsPublished", isPublished)
            };

            await _db.ExecuteNonQueryAsync("dbo.sp_PublishExaminationResults", CommandType.StoredProcedure, parameters);
            return true;
        }

        public async Task<Result?> SearchResultAsync(string searchTerm, int? examId = null)
        {
            var parameters = new[]
            {
                new SqlParameter("@SearchTerm", searchTerm.Trim()),
                new SqlParameter("@ExaminationId", (object?)examId ?? DBNull.Value)
            };

            var result = await _db.ExecuteReaderSingleAsync(
                "dbo.sp_SearchStudentResult",
                MapResult,
                CommandType.StoredProcedure,
                parameters
            );

            if (result != null)
            {
                result.Details = await GetResultDetailsAsync(result.ResultId);
            }

            return result;
        }

        public async Task<Result?> GetResultByIdAsync(int resultId)
        {
            const string sql = @"
                SELECT r.ResultId, r.ExaminationId, e.ExamName, e.ExamCode, e.StartDate AS ExamDate,
                       r.StudentId, s.FullName AS StudentName, r.RegistrationNumber, r.RollNumber,
                       s.PhotoUrl, p.ProgramName, p.ProgramCode, c.CollegeName, c.CollegeCode,
                       sem.SemesterName, r.TotalMarksObtained, r.TotalFullMarks, r.TotalCreditHours,
                       r.Percentage, r.SGPA, r.CGPA, r.ResultStatus, r.FailedSubjectCount,
                       r.IsPublished, r.PublishedDate, gs.GradeSheetNumber, gs.IssueDate AS GradeSheetIssueDate,
                       gs.QrCodeHash AS GradeSheetQrCode
                FROM dbo.Results r
                JOIN dbo.Examinations e ON r.ExaminationId = e.ExaminationId
                JOIN dbo.Students s ON r.StudentId = s.StudentId
                JOIN dbo.Programs p ON r.ProgramId = p.ProgramId
                JOIN dbo.Colleges c ON r.CollegeId = c.CollegeId
                JOIN dbo.Semesters sem ON r.SemesterId = sem.SemesterId
                LEFT JOIN dbo.StudentGradeSheets gs ON r.ResultId = gs.ResultId
                WHERE r.ResultId = @Id";

            var result = await _db.ExecuteReaderSingleAsync(sql, MapResult, CommandType.Text, new[] { new SqlParameter("@Id", resultId) });
            if (result != null)
            {
                result.Details = await GetResultDetailsAsync(result.ResultId);
            }
            return result;
        }

        public async Task<Result?> GetResultByStudentAndExamAsync(int studentId, int examId)
        {
            const string sql = @"
                SELECT r.ResultId, r.ExaminationId, e.ExamName, e.ExamCode, e.StartDate AS ExamDate,
                       r.StudentId, s.FullName AS StudentName, r.RegistrationNumber, r.RollNumber,
                       s.PhotoUrl, p.ProgramName, p.ProgramCode, c.CollegeName, c.CollegeCode,
                       sem.SemesterName, r.TotalMarksObtained, r.TotalFullMarks, r.TotalCreditHours,
                       r.Percentage, r.SGPA, r.CGPA, r.ResultStatus, r.FailedSubjectCount,
                       r.IsPublished, r.PublishedDate, gs.GradeSheetNumber, gs.IssueDate AS GradeSheetIssueDate,
                       gs.QrCodeHash AS GradeSheetQrCode
                FROM dbo.Results r
                JOIN dbo.Examinations e ON r.ExaminationId = e.ExaminationId
                JOIN dbo.Students s ON r.StudentId = s.StudentId
                JOIN dbo.Programs p ON r.ProgramId = p.ProgramId
                JOIN dbo.Colleges c ON r.CollegeId = c.CollegeId
                JOIN dbo.Semesters sem ON r.SemesterId = sem.SemesterId
                LEFT JOIN dbo.StudentGradeSheets gs ON r.ResultId = gs.ResultId
                WHERE r.StudentId = @StudentId AND r.ExaminationId = @ExamId";

            var result = await _db.ExecuteReaderSingleAsync(sql, MapResult, CommandType.Text, new[]
            {
                new SqlParameter("@StudentId", studentId),
                new SqlParameter("@ExamId", examId)
            });

            if (result != null)
            {
                result.Details = await GetResultDetailsAsync(result.ResultId);
            }
            return result;
        }

        public async Task<List<Result>> GetResultsByExamAsync(int examId, int? collegeId = null, int? programId = null)
        {
            var sql = @"
                SELECT r.ResultId, r.ExaminationId, e.ExamName, e.ExamCode, e.StartDate AS ExamDate,
                       r.StudentId, s.FullName AS StudentName, r.RegistrationNumber, r.RollNumber,
                       s.PhotoUrl, p.ProgramName, p.ProgramCode, c.CollegeName, c.CollegeCode,
                       sem.SemesterName, r.TotalMarksObtained, r.TotalFullMarks, r.TotalCreditHours,
                       r.Percentage, r.SGPA, r.CGPA, r.ResultStatus, r.FailedSubjectCount,
                       r.IsPublished, r.PublishedDate, gs.GradeSheetNumber, gs.IssueDate AS GradeSheetIssueDate,
                       gs.QrCodeHash AS GradeSheetQrCode
                FROM dbo.Results r
                JOIN dbo.Examinations e ON r.ExaminationId = e.ExaminationId
                JOIN dbo.Students s ON r.StudentId = s.StudentId
                JOIN dbo.Programs p ON r.ProgramId = p.ProgramId
                JOIN dbo.Colleges c ON r.CollegeId = c.CollegeId
                JOIN dbo.Semesters sem ON r.SemesterId = sem.SemesterId
                LEFT JOIN dbo.StudentGradeSheets gs ON r.ResultId = gs.ResultId
                WHERE r.ExaminationId = @ExamId";

            var parameters = new List<SqlParameter> { new SqlParameter("@ExamId", examId) };
            if (collegeId.HasValue)
            {
                sql += " AND r.CollegeId = @CollegeId";
                parameters.Add(new SqlParameter("@CollegeId", collegeId.Value));
            }
            if (programId.HasValue)
            {
                sql += " AND r.ProgramId = @ProgramId";
                parameters.Add(new SqlParameter("@ProgramId", programId.Value));
            }
            sql += " ORDER BY r.RollNumber";

            return await _db.ExecuteReaderListAsync(sql, MapResult, CommandType.Text, parameters.ToArray());
        }

        public async Task<List<Result>> GetStudentResultsHistoryAsync(int studentId)
        {
            const string sql = @"
                SELECT r.ResultId, r.ExaminationId, e.ExamName, e.ExamCode, e.StartDate AS ExamDate,
                       r.StudentId, s.FullName AS StudentName, r.RegistrationNumber, r.RollNumber,
                       s.PhotoUrl, p.ProgramName, p.ProgramCode, c.CollegeName, c.CollegeCode,
                       sem.SemesterName, r.TotalMarksObtained, r.TotalFullMarks, r.TotalCreditHours,
                       r.Percentage, r.SGPA, r.CGPA, r.ResultStatus, r.FailedSubjectCount,
                       r.IsPublished, r.PublishedDate, gs.GradeSheetNumber, gs.IssueDate AS GradeSheetIssueDate,
                       gs.QrCodeHash AS GradeSheetQrCode
                FROM dbo.Results r
                JOIN dbo.Examinations e ON r.ExaminationId = e.ExaminationId
                JOIN dbo.Students s ON r.StudentId = s.StudentId
                JOIN dbo.Programs p ON r.ProgramId = p.ProgramId
                JOIN dbo.Colleges c ON r.CollegeId = c.CollegeId
                JOIN dbo.Semesters sem ON r.SemesterId = sem.SemesterId
                LEFT JOIN dbo.StudentGradeSheets gs ON r.ResultId = gs.ResultId
                WHERE r.StudentId = @StudentId AND r.IsPublished = 1
                ORDER BY e.StartDate DESC";

            var results = await _db.ExecuteReaderListAsync(sql, MapResult, CommandType.Text, new[] { new SqlParameter("@StudentId", studentId) });
            foreach (var res in results)
            {
                res.Details = await GetResultDetailsAsync(res.ResultId);
            }
            return results;
        }

        public async Task<DataTable> ExportResultsToDataTableAsync(int examId, int? collegeId = null, int? programId = null)
        {
            var sql = @"
                SELECT r.RollNumber AS [Roll No], r.RegistrationNumber AS [Registration No],
                       s.FullName AS [Student Name], c.CollegeName AS [College],
                       p.ProgramName AS [Program], sem.SemesterName AS [Semester],
                       r.TotalMarksObtained AS [Marks Obtained], r.TotalFullMarks AS [Full Marks],
                       r.Percentage AS [Percentage], r.SGPA, r.ResultStatus AS [Status],
                       CASE WHEN r.IsPublished = 1 THEN 'Published' ELSE 'Draft' END AS [Publication State]
                FROM dbo.Results r
                JOIN dbo.Students s ON r.StudentId = s.StudentId
                JOIN dbo.Colleges c ON r.CollegeId = c.CollegeId
                JOIN dbo.Programs p ON r.ProgramId = p.ProgramId
                JOIN dbo.Semesters sem ON r.SemesterId = sem.SemesterId
                WHERE r.ExaminationId = @ExamId";

            var parameters = new List<SqlParameter> { new SqlParameter("@ExamId", examId) };
            if (collegeId.HasValue)
            {
                sql += " AND r.CollegeId = @CollegeId";
                parameters.Add(new SqlParameter("@CollegeId", collegeId.Value));
            }
            if (programId.HasValue)
            {
                sql += " AND r.ProgramId = @ProgramId";
                parameters.Add(new SqlParameter("@ProgramId", programId.Value));
            }
            sql += " ORDER BY r.RollNumber";

            return await _db.ExecuteDataTableAsync(sql, CommandType.Text, parameters.ToArray());
        }

        private async Task<List<ResultDetail>> GetResultDetailsAsync(int resultId)
        {
            const string sql = @"
                SELECT rd.ResultDetailId, rd.ResultId, rd.SubjectId, sub.SubjectCode, sub.SubjectName,
                       sub.CreditHours, sub.FullMarks, sub.PassMarks, rd.InternalMarks, rd.ExternalMarks,
                       rd.PracticalMarks, rd.TotalMarks, rd.Grade, rd.GradePoint, rd.IsPass, rd.IsAbsent, rd.Remarks
                FROM dbo.ResultDetails rd
                JOIN dbo.Subjects sub ON rd.SubjectId = sub.SubjectId
                WHERE rd.ResultId = @ResultId
                ORDER BY sub.SubjectCode";

            return await _db.ExecuteReaderListAsync(sql, r => new ResultDetail
            {
                ResultDetailId = r.GetInt32(r.GetOrdinal("ResultDetailId")),
                ResultId = r.GetInt32(r.GetOrdinal("ResultId")),
                SubjectId = r.GetInt32(r.GetOrdinal("SubjectId")),
                SubjectCode = r.GetString(r.GetOrdinal("SubjectCode")),
                SubjectName = r.GetString(r.GetOrdinal("SubjectName")),
                CreditHours = r.GetInt32(r.GetOrdinal("CreditHours")),
                FullMarks = r.GetInt32(r.GetOrdinal("FullMarks")),
                PassMarks = r.GetInt32(r.GetOrdinal("PassMarks")),
                InternalMarks = r.GetDecimal(r.GetOrdinal("InternalMarks")),
                ExternalMarks = r.GetDecimal(r.GetOrdinal("ExternalMarks")),
                PracticalMarks = r.GetDecimal(r.GetOrdinal("PracticalMarks")),
                TotalMarks = r.GetDecimal(r.GetOrdinal("TotalMarks")),
                Grade = r.GetString(r.GetOrdinal("Grade")),
                GradePoint = r.GetDecimal(r.GetOrdinal("GradePoint")),
                IsPass = r.GetBoolean(r.GetOrdinal("IsPass")),
                IsAbsent = r.GetBoolean(r.GetOrdinal("IsAbsent")),
                Remarks = r.IsDBNull(r.GetOrdinal("Remarks")) ? null : r.GetString(r.GetOrdinal("Remarks"))
            }, CommandType.Text, new[] { new SqlParameter("@ResultId", resultId) });
        }

        private static Result MapResult(SqlDataReader r)
        {
            return new Result
            {
                ResultId = r.GetInt32(r.GetOrdinal("ResultId")),
                ExaminationId = r.GetInt32(r.GetOrdinal("ExaminationId")),
                ExamName = r.IsDBNull(r.GetOrdinal("ExamName")) ? null : r.GetString(r.GetOrdinal("ExamName")),
                ExamCode = r.IsDBNull(r.GetOrdinal("ExamCode")) ? null : r.GetString(r.GetOrdinal("ExamCode")),
                ExamDate = r.IsDBNull(r.GetOrdinal("ExamDate")) ? null : r.GetDateTime(r.GetOrdinal("ExamDate")),
                StudentId = r.GetInt32(r.GetOrdinal("StudentId")),
                StudentName = r.IsDBNull(r.GetOrdinal("StudentName")) ? null : r.GetString(r.GetOrdinal("StudentName")),
                RegistrationNumber = r.GetString(r.GetOrdinal("RegistrationNumber")),
                RollNumber = r.GetString(r.GetOrdinal("RollNumber")),
                PhotoUrl = r.IsDBNull(r.GetOrdinal("PhotoUrl")) ? null : r.GetString(r.GetOrdinal("PhotoUrl")),
                ProgramName = r.IsDBNull(r.GetOrdinal("ProgramName")) ? null : r.GetString(r.GetOrdinal("ProgramName")),
                ProgramCode = r.IsDBNull(r.GetOrdinal("ProgramCode")) ? null : r.GetString(r.GetOrdinal("ProgramCode")),
                CollegeName = r.IsDBNull(r.GetOrdinal("CollegeName")) ? null : r.GetString(r.GetOrdinal("CollegeName")),
                CollegeCode = r.IsDBNull(r.GetOrdinal("CollegeCode")) ? null : r.GetString(r.GetOrdinal("CollegeCode")),
                SemesterName = r.IsDBNull(r.GetOrdinal("SemesterName")) ? null : r.GetString(r.GetOrdinal("SemesterName")),
                TotalMarksObtained = r.GetDecimal(r.GetOrdinal("TotalMarksObtained")),
                TotalFullMarks = r.GetDecimal(r.GetOrdinal("TotalFullMarks")),
                TotalCreditHours = r.GetInt32(r.GetOrdinal("TotalCreditHours")),
                Percentage = r.GetDecimal(r.GetOrdinal("Percentage")),
                SGPA = r.GetDecimal(r.GetOrdinal("SGPA")),
                CGPA = r.GetDecimal(r.GetOrdinal("CGPA")),
                ResultStatus = r.GetString(r.GetOrdinal("ResultStatus")),
                FailedSubjectCount = r.GetInt32(r.GetOrdinal("FailedSubjectCount")),
                IsPublished = r.GetBoolean(r.GetOrdinal("IsPublished")),
                PublishedDate = r.IsDBNull(r.GetOrdinal("PublishedDate")) ? null : r.GetDateTime(r.GetOrdinal("PublishedDate")),
                GradeSheetNumber = r.IsDBNull(r.GetOrdinal("GradeSheetNumber")) ? null : r.GetString(r.GetOrdinal("GradeSheetNumber")),
                GradeSheetIssueDate = r.IsDBNull(r.GetOrdinal("GradeSheetIssueDate")) ? null : r.GetDateTime(r.GetOrdinal("GradeSheetIssueDate")),
                GradeSheetQrCode = r.IsDBNull(r.GetOrdinal("GradeSheetQrCode")) ? null : r.GetString(r.GetOrdinal("GradeSheetQrCode"))
            };
        }
    }
}
