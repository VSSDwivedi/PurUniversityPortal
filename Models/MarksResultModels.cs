namespace UniversityExamSystem.Models
{
    public class Mark
    {
        public int MarkId { get; set; }
        public int ExaminationId { get; set; }
        public string? ExamName { get; set; }
        public int StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? RollNumber { get; set; }
        public string? RegistrationNumber { get; set; }
        public int SubjectId { get; set; }
        public string? SubjectCode { get; set; }
        public string? SubjectName { get; set; }
        public int CreditHours { get; set; }
        public int FullMarks { get; set; } = 100;
        public int PassMarks { get; set; } = 45;
        public decimal InternalMarks { get; set; }
        public decimal ExternalMarks { get; set; }
        public decimal PracticalMarks { get; set; }
        public decimal TotalMarks { get; set; }
        public string? Grade { get; set; }
        public decimal? GradePoint { get; set; }
        public string Status { get; set; } = "Draft"; // Draft, Submitted, Approved, Locked
        public bool IsAbsent { get; set; }
        public bool IsWithheld { get; set; }
        public string? Remarks { get; set; }
        public int? EnteredBy { get; set; }
        public DateTime EnteredDate { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
    }

    public class Result
    {
        public int ResultId { get; set; }
        public int ExaminationId { get; set; }
        public string? ExamName { get; set; }
        public string? ExamCode { get; set; }
        public DateTime? ExamDate { get; set; }
        public int StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? PhotoUrl { get; set; }
        public int ProgramId { get; set; }
        public string? ProgramName { get; set; }
        public string? ProgramCode { get; set; }
        public int SemesterId { get; set; }
        public string? SemesterName { get; set; }
        public int CollegeId { get; set; }
        public string? CollegeName { get; set; }
        public string? CollegeCode { get; set; }
        public string RollNumber { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public decimal TotalMarksObtained { get; set; }
        public decimal TotalFullMarks { get; set; }
        public int TotalCreditHours { get; set; }
        public decimal Percentage { get; set; }
        public decimal SGPA { get; set; }
        public decimal CGPA { get; set; }
        public string ResultStatus { get; set; } = "Passed"; // Passed, Failed, Withheld, Absent
        public int FailedSubjectCount { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? PublishedDate { get; set; }
        public DateTime CalculatedDate { get; set; }

        public string? GradeSheetNumber { get; set; }
        public DateTime? GradeSheetIssueDate { get; set; }
        public string? GradeSheetQrCode { get; set; }
        public string? GradeSheetQrCodeBase64 { get; set; }

        public List<ResultDetail> Details { get; set; } = new();
    }

    public class ResultDetail
    {
        public int ResultDetailId { get; set; }
        public int ResultId { get; set; }
        public int SubjectId { get; set; }
        public string? SubjectCode { get; set; }
        public string? SubjectName { get; set; }
        public int CreditHours { get; set; }
        public int FullMarks { get; set; }
        public int PassMarks { get; set; }
        public decimal InternalMarks { get; set; }
        public decimal ExternalMarks { get; set; }
        public decimal PracticalMarks { get; set; }
        public decimal TotalMarks { get; set; }
        public string Grade { get; set; } = "F";
        public decimal GradePoint { get; set; }
        public bool IsPass { get; set; } = true;
        public bool IsAbsent { get; set; }
        public string? Remarks { get; set; }
    }

    public class StudentGradeSheet
    {
        public int GradeSheetId { get; set; }
        public string GradeSheetNumber { get; set; } = string.Empty;
        public int ResultId { get; set; }
        public int StudentId { get; set; }
        public DateTime IssueDate { get; set; }
        public string QrCodeHash { get; set; } = string.Empty;
        public string? ControllerSignatureUrl { get; set; }
        public bool IsPrinted { get; set; }
    }
}
