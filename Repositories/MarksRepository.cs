using System.Data;
using Microsoft.Data.SqlClient;
using UniversityExamSystem.Data;
using UniversityExamSystem.Models;

namespace UniversityExamSystem.Repositories
{
    public interface IMarksRepository
    {
        Task<List<Mark>> GetMarksAsync(int examId, int subjectId, int? collegeId = null);
        Task<List<Mark>> GetStudentMarksAsync(int studentId, int examId);
        Task<int> SaveMarkAsync(int examId, int studentId, int subjectId, decimal internalMarks, decimal externalMarks, decimal practicalMarks, bool isAbsent, int? enteredBy, string status = "Draft");
        Task<int> BatchSaveMarksAsync(List<MarkEntryRow> marks, int examId, int subjectId, int? enteredBy, string status = "Draft");
        Task<bool> SubmitMarksForApprovalAsync(int examId, int subjectId);
        Task<bool> ApproveAndLockMarksAsync(int examId, int subjectId, int approvedBy);
        Task<DataTable> ExportMarksTemplateAsync(int examId, int subjectId, int? collegeId = null);
        Task<int> ImportMarksFromDataTableAsync(DataTable dt, int examId, int subjectId, int? enteredBy);
    }

    public class MarksRepository : IMarksRepository
    {
        private readonly DatabaseHelper _db;

        public MarksRepository(DatabaseHelper db)
        {
            _db = db;
        }
        public async Task<List<Mark>> GetMarksAsync(int examId, int subjectId, int? collegeId = null)
        {
            // Join with ExaminationApplicationSubjects so all students who applied for this subject in this exam appear
            var sql = @"
               SELECT ISNULL(m.MarkId, 0) AS MarkId, @ExamId AS ExaminationId, e.ExamName,
                       s.StudentId, s.FullName AS StudentName, s.RollNumber, s.RegistrationNumber,
                       sub.SubjectId, sub.SubjectCode, sub.SubjectName, sub.CreditHours, sub.FullMarks, sub.PassMarks,
                       ISNULL(m.InternalMarks, 0.00) AS InternalMarks,
                       ISNULL(m.ExternalMarks, 0.00) AS ExternalMarks,
                       ISNULL(m.PracticalMarks, 0.00) AS PracticalMarks,
                       ISNULL(m.TotalMarks, 0.00) AS TotalMarks,
                       m.Grade, m.GradePoint,
                       ISNULL(m.Status, 'Draft') AS Status,
                       ISNULL(m.IsAbsent, 0) AS IsAbsent,
                       ISNULL(m.IsWithheld, 0) AS IsWithheld,
                       m.Remarks, m.EnteredBy,
                       ISNULL(m.EnteredDate, GETDATE()) AS EnteredDate,
                       m.ApprovedBy, m.ApprovedDate
                FROM dbo.ExaminationApplications ea
                JOIN dbo.ExaminationApplicationSubjects eas ON ea.ApplicationId = eas.ApplicationId
                JOIN dbo.Students s ON ea.StudentId = s.StudentId
                JOIN dbo.Examinations e ON ea.ExaminationId = e.ExaminationId
                JOIN dbo.Subjects sub ON eas.SubjectId = sub.SubjectId
                LEFT JOIN dbo.Marks m ON m.ExaminationId = ea.ExaminationId AND m.StudentId = s.StudentId AND m.SubjectId = eas.SubjectId
                WHERE ea.ExaminationId = @ExamId AND eas.SubjectId = @SubjectId";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@ExamId", examId),
                new SqlParameter("@SubjectId", subjectId)
            };

            if (collegeId.HasValue)
            {
                sql += " AND ea.CollegeId = @CollegeId";
                parameters.Add(new SqlParameter("@CollegeId", collegeId.Value));
            }

            sql += " ORDER BY s.RollNumber, s.FullName";

            return await _db.ExecuteReaderListAsync(sql, MapMark, CommandType.Text, parameters.ToArray());
        }

        public async Task<List<Mark>> GetStudentMarksAsync(int studentId, int examId)
        {
            const string sql = @"
                SELECT m.MarkId, m.ExaminationId, e.ExamName, m.StudentId, s.FullName AS StudentName,
                       s.RollNumber, s.RegistrationNumber, m.SubjectId, sub.SubjectCode, sub.SubjectName,
                       sub.CreditHours, sub.FullMarks, sub.PassMarks, m.InternalMarks, m.ExternalMarks,
                       m.PracticalMarks, m.TotalMarks, m.Grade, m.GradePoint, m.Status, m.IsAbsent,
                       m.IsWithheld, m.Remarks, m.EnteredBy, m.EnteredDate, m.ApprovedBy, m.ApprovedDate
                FROM dbo.Marks m
                JOIN dbo.Examinations e ON m.ExaminationId = e.ExaminationId
                JOIN dbo.Students s ON m.StudentId = s.StudentId
                JOIN dbo.Subjects sub ON m.SubjectId = sub.SubjectId
                WHERE m.StudentId = @StudentId AND m.ExaminationId = @ExamId
                ORDER BY sub.SubjectCode";

            return await _db.ExecuteReaderListAsync(sql, MapMark, CommandType.Text, new[]
            {
                new SqlParameter("@StudentId", studentId),
                new SqlParameter("@ExamId", examId)
            });
        }

        public async Task<int> SaveMarkAsync(int examId, int studentId, int subjectId, decimal internalMarks, decimal externalMarks, decimal practicalMarks, bool isAbsent, int? enteredBy, string status = "Draft")
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            using var cmd = new SqlCommand("dbo.sp_SaveMarksEntry", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@ExaminationId", examId);
            cmd.Parameters.AddWithValue("@StudentId", studentId);
            cmd.Parameters.AddWithValue("@SubjectId", subjectId);
            cmd.Parameters.AddWithValue("@InternalMarks", internalMarks);
            cmd.Parameters.AddWithValue("@ExternalMarks", externalMarks);
            cmd.Parameters.AddWithValue("@PracticalMarks", practicalMarks);
            cmd.Parameters.AddWithValue("@IsAbsent", isAbsent);
            cmd.Parameters.AddWithValue("@EnteredBy", (object?)enteredBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", status);

            var pMarkId = new SqlParameter("@MarkId", SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(pMarkId);

            await cmd.ExecuteNonQueryAsync();

            return (int)pMarkId.Value;
        }

        public async Task<int> BatchSaveMarksAsync(List<MarkEntryRow> marks, int examId, int subjectId, int? enteredBy, string status = "Draft")
        {
            int updated = 0;
            foreach (var item in marks)
            {
                await SaveMarkAsync(examId, item.StudentId, subjectId, item.InternalMarks, item.ExternalMarks, item.PracticalMarks, item.IsAbsent, enteredBy, status);
                updated++;
            }
            return updated;
        }

        public async Task<bool> SubmitMarksForApprovalAsync(int examId, int subjectId)
        {
            const string sql = @"
                UPDATE dbo.Marks
                SET Status = 'Submitted'
                WHERE ExaminationId = @ExamId AND SubjectId = @SubjectId AND Status = 'Draft'";

            var rows = await _db.ExecuteNonQueryAsync(sql, CommandType.Text, new[]
            {
                new SqlParameter("@ExamId", examId),
                new SqlParameter("@SubjectId", subjectId)
            });
            return rows > 0;
        }

        public async Task<bool> ApproveAndLockMarksAsync(int examId, int subjectId, int approvedBy)
        {
            const string sql = @"
                UPDATE dbo.Marks
                SET Status = 'Locked', ApprovedBy = @ApprovedBy, ApprovedDate = GETDATE(), LockedDate = GETDATE()
                WHERE ExaminationId = @ExamId AND SubjectId = @SubjectId";

            var rows = await _db.ExecuteNonQueryAsync(sql, CommandType.Text, new[]
            {
                new SqlParameter("@ExamId", examId),
                new SqlParameter("@SubjectId", subjectId),
                new SqlParameter("@ApprovedBy", approvedBy)
            });
            return rows > 0;
        }

        public async Task<DataTable> ExportMarksTemplateAsync(int examId, int subjectId, int? collegeId = null)
        {
            var sql = @"
                SELECT s.StudentId AS [StudentId], s.RollNumber AS [RollNumber], s.RegistrationNumber AS [RegistrationNumber],
                       s.FullName AS [StudentName],
                       ISNULL(m.InternalMarks, 0) AS [InternalMarks],
                       ISNULL(m.ExternalMarks, 0) AS [ExternalMarks],
                       ISNULL(m.PracticalMarks, 0) AS [PracticalMarks],
                       CASE WHEN m.IsAbsent = 1 THEN 'YES' ELSE 'NO' END AS [IsAbsent]
                FROM dbo.ExaminationApplications ea
                JOIN dbo.ExaminationApplicationSubjects eas ON ea.ApplicationId = eas.ApplicationId
                JOIN dbo.Students s ON ea.StudentId = s.StudentId
                LEFT JOIN dbo.Marks m ON m.ExaminationId = ea.ExaminationId AND m.StudentId = s.StudentId AND m.SubjectId = eas.SubjectId
                WHERE ea.ExaminationId = @ExamId AND eas.SubjectId = @SubjectId";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@ExamId", examId),
                new SqlParameter("@SubjectId", subjectId)
            };

            if (collegeId.HasValue)
            {
                sql += " AND ea.CollegeId = @CollegeId";
                parameters.Add(new SqlParameter("@CollegeId", collegeId.Value));
            }

            sql += " ORDER BY s.RollNumber";

            return await _db.ExecuteDataTableAsync(sql, CommandType.Text, parameters.ToArray());
        }

        public async Task<int> ImportMarksFromDataTableAsync(DataTable dt, int examId, int subjectId, int? enteredBy)
        {
            int count = 0;
            foreach (DataRow row in dt.Rows)
            {
                int studentId = 0;
                if (row.Table.Columns.Contains("StudentId") && !row.IsNull("StudentId"))
                {
                    studentId = Convert.ToInt32(row["StudentId"]);
                }
                else if (row.Table.Columns.Contains("RollNumber") && !row.IsNull("RollNumber"))
                {
                    var roll = row["RollNumber"].ToString();
                    var sqlFind = "SELECT StudentId FROM dbo.Students WHERE RollNumber = @Roll";
                    var obj = await _db.ExecuteScalarAsync(sqlFind, CommandType.Text, new[] { new SqlParameter("@Roll", roll) });
                    if (obj != null) studentId = Convert.ToInt32(obj);
                }

                if (studentId == 0) continue;

                decimal internalM = Convert.ToDecimal(row.Table.Columns.Contains("InternalMarks") && !row.IsNull("InternalMarks") ? row["InternalMarks"] : 0);
                decimal externalM = Convert.ToDecimal(row.Table.Columns.Contains("ExternalMarks") && !row.IsNull("ExternalMarks") ? row["ExternalMarks"] : 0);
                decimal practicalM = Convert.ToDecimal(row.Table.Columns.Contains("PracticalMarks") && !row.IsNull("PracticalMarks") ? row["PracticalMarks"] : 0);
                bool isAbsent = row.Table.Columns.Contains("IsAbsent") && row["IsAbsent"]?.ToString()?.ToUpperInvariant() == "YES";

                await SaveMarkAsync(examId, studentId, subjectId, internalM, externalM, practicalM, isAbsent, enteredBy, "Draft");
                count++;
            }
            return count;
        }

        private static Mark MapMark(SqlDataReader r)
        {
            return new Mark
            {
                MarkId = r.GetInt32(r.GetOrdinal("MarkId")),
                ExaminationId = r.GetInt32(r.GetOrdinal("ExaminationId")),
                ExamName = r.IsDBNull(r.GetOrdinal("ExamName")) ? null : r.GetString(r.GetOrdinal("ExamName")),
                StudentId = r.GetInt32(r.GetOrdinal("StudentId")),
                StudentName = r.IsDBNull(r.GetOrdinal("StudentName")) ? null : r.GetString(r.GetOrdinal("StudentName")),
                RollNumber = r.IsDBNull(r.GetOrdinal("RollNumber")) ? null : r.GetString(r.GetOrdinal("RollNumber")),
                RegistrationNumber = r.IsDBNull(r.GetOrdinal("RegistrationNumber")) ? null : r.GetString(r.GetOrdinal("RegistrationNumber")),
                SubjectId = r.GetInt32(r.GetOrdinal("SubjectId")),
                SubjectCode = r.IsDBNull(r.GetOrdinal("SubjectCode")) ? null : r.GetString(r.GetOrdinal("SubjectCode")),
                SubjectName = r.IsDBNull(r.GetOrdinal("SubjectName")) ? null : r.GetString(r.GetOrdinal("SubjectName")),
                CreditHours = r.GetInt32(r.GetOrdinal("CreditHours")),
                FullMarks = r.GetInt32(r.GetOrdinal("FullMarks")),
                PassMarks = r.GetInt32(r.GetOrdinal("PassMarks")),
                InternalMarks = r.GetDecimal(r.GetOrdinal("InternalMarks")),
                ExternalMarks = r.GetDecimal(r.GetOrdinal("ExternalMarks")),
                PracticalMarks = r.GetDecimal(r.GetOrdinal("PracticalMarks")),
                TotalMarks = r.GetDecimal(r.GetOrdinal("TotalMarks")),
                Grade = r.IsDBNull(r.GetOrdinal("Grade")) ? null : r.GetString(r.GetOrdinal("Grade")),
                GradePoint = r.IsDBNull(r.GetOrdinal("GradePoint")) ? null : r.GetDecimal(r.GetOrdinal("GradePoint")),
                Status = r.GetString(r.GetOrdinal("Status")),
                IsAbsent = r.GetBoolean(r.GetOrdinal("IsAbsent")),
                IsWithheld = r.GetBoolean(r.GetOrdinal("IsWithheld")),
                Remarks = r.IsDBNull(r.GetOrdinal("Remarks")) ? null : r.GetString(r.GetOrdinal("Remarks")),
                EnteredBy = r.IsDBNull(r.GetOrdinal("EnteredBy")) ? null : r.GetInt32(r.GetOrdinal("EnteredBy")),
                EnteredDate = r.GetDateTime(r.GetOrdinal("EnteredDate")),
                ApprovedBy = r.IsDBNull(r.GetOrdinal("ApprovedBy")) ? null : r.GetInt32(r.GetOrdinal("ApprovedBy")),
                ApprovedDate = r.IsDBNull(r.GetOrdinal("ApprovedDate")) ? null : r.GetDateTime(r.GetOrdinal("ApprovedDate"))
            };
        }
    }
}
