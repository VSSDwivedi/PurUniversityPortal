namespace UniversityExamSystem.Models
{
    public class Student
    {
        public int StudentId { get; set; }
        public string StudentUniqueId { get; set; } = string.Empty;
        public string? RegistrationNumber { get; set; }
        public string? RollNumber { get; set; }
        public int? UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int FacultyId { get; set; }
        public int ProgramId { get; set; }
        public int CollegeId { get; set; }
        public int AcademicYearId { get; set; }
        public int CurrentSemesterId { get; set; }
        public string? PreviousQualification { get; set; }
        public string? PhotoUrl { get; set; }
        public string? IdentityDocumentUrl { get; set; }
        public string RegistrationStatus { get; set; } = "Pending"; // Pending, Approved, Rejected, CorrectionRequired
        public string? VerificationRemarks { get; set; }
        public int? VerifiedBy { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Joined view properties
        public string? FacultyName { get; set; }
        public string? ProgramName { get; set; }
        public string? ProgramCode { get; set; }
        public string? CollegeName { get; set; }
        public string? CollegeCode { get; set; }
        public string? AcademicYear { get; set; }
        public string? CurrentSemester { get; set; }
        public string? ApplicationNumber { get; set; }
        public string? RegistrationAppStatus { get; set; }
    }

    public class StudentRegistration
    {
        public int RegistrationId { get; set; }
        public int StudentId { get; set; }
        public string ApplicationNumber { get; set; } = string.Empty;
        public DateTime SubmissionDate { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime? ProcessedDate { get; set; }
        public int? ProcessedBy { get; set; }
        public string? Remarks { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class StudentDocument
    {
        public int DocumentId { get; set; }
        public int StudentId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long? FileSizeKB { get; set; }
        public DateTime UploadedDate { get; set; }
    }
}
