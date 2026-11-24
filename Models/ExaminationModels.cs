namespace UniversityExamSystem.Models
{
    public class Examination
    {
        public int ExaminationId { get; set; }
        public string ExamCode { get; set; } = string.Empty;
        public string ExamName { get; set; } = string.Empty;
        public int AcademicYearId { get; set; }
        public string? AcademicYearName { get; set; }
        public int SemesterId { get; set; }
        public string? SemesterName { get; set; }
        public int? FacultyId { get; set; }
        public string? FacultyName { get; set; }
        public int? ProgramId { get; set; }
        public string? ProgramName { get; set; }
        public string ExamType { get; set; } = "Regular";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime ApplicationDeadline { get; set; }
        public DateTime? LateFeeDeadline { get; set; }
        public decimal FeeAmount { get; set; } = 1500.00m;
        public decimal LateFeeAmount { get; set; } = 500.00m;
        public int? DefaultCenterId { get; set; }
        public string? DefaultCenterName { get; set; }
        public string Status { get; set; } = "Upcoming"; // Upcoming, Ongoing, Closed, ResultProcessed, ResultPublished
        public bool IsActive { get; set; } = true;
        public int TotalApplicants { get; set; }
    }

    public class ExaminationApplication
    {
        public int ApplicationId { get; set; }
        public string ApplicationNumber { get; set; } = string.Empty;
        public int ExaminationId { get; set; }
        public string? ExamName { get; set; }
        public int StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? RegistrationNumber { get; set; }
        public int CollegeId { get; set; }
        public string? CollegeName { get; set; }
        public int ProgramId { get; set; }
        public string? ProgramName { get; set; }
        public int SemesterId { get; set; }
        public string? SemesterName { get; set; }
        public string ExamType { get; set; } = "Regular";
        public string? AssignedRollNumber { get; set; }
        public int? ExaminationCenterId { get; set; }
        public string? CenterName { get; set; }
        public decimal FeePaid { get; set; }
        public string? PaymentReference { get; set; }
        public string? PaymentVoucherUrl { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, CorrectionRequired
        public string? Remarks { get; set; }
        public DateTime SubmissionDate { get; set; }
        public int? ReviewedBy { get; set; }
        public DateTime? ReviewedDate { get; set; }

        public List<ExaminationApplicationSubject> SelectedSubjects { get; set; } = new();
    }

    public class ExaminationApplicationSubject
    {
        public int AppSubjectId { get; set; }
        public int ApplicationId { get; set; }
        public int SubjectId { get; set; }
        public string? SubjectCode { get; set; }
        public string? SubjectName { get; set; }
        public int CreditHours { get; set; }
        public string SubjectType { get; set; } = "Regular";
        public DateTime? ExamDate { get; set; }
        public string? ExamTime { get; set; }
    }

    public class AdmitCard
    {
        public int AdmitCardId { get; set; }
        public string AdmitCardNumber { get; set; } = string.Empty;
        public int ApplicationId { get; set; }
        public int StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? PhotoUrl { get; set; }
        public int ExaminationId { get; set; }
        public string? ExamName { get; set; }
        public string RollNumber { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public int ExaminationCenterId { get; set; }
        public string? CenterName { get; set; }
        public string? CenterLocation { get; set; }
        public string? ProgramName { get; set; }
        public string? FacultyName { get; set; }
        public string? CollegeName { get; set; }
        public string? SemesterName { get; set; }
        public string QrCodeHash { get; set; } = string.Empty;
        public string? QrCodeBase64 { get; set; }
        public DateTime GeneratedDate { get; set; }
        public bool IsDownloaded { get; set; }
        public int DownloadCount { get; set; }

        public List<ExaminationApplicationSubject> Subjects { get; set; } = new();
    }
}
