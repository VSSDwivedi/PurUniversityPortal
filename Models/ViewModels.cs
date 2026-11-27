using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace UniversityExamSystem.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Please enter Registration Number, Student ID, Email, or Username")]
        [Display(Name = "Student ID / Registration No / Email")]
        public string LoginIdentifier { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your password")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }

    public class StudentRegistrationViewModel
    {
        [Required(ErrorMessage = "Full Name is required")]
        [Display(Name = "Full Name (as per certificates)")]
        [RegularExpression(@"^[A-Za-z ]+$", ErrorMessage = "Name can contain only letters and spaces")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of Birth is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; } = "Male";

        [Required(ErrorMessage = "Email Address is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile Number is required")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [RegularExpression(@"^[6-9][0-9]{9}$", ErrorMessage = "Enter a valid 10 digit mobile number")]
        [Display(Name = "Mobile Number")]
        public string MobileNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select Faculty")]
        [Display(Name = "Faculty")]
        public int FacultyId { get; set; }

        [Required(ErrorMessage = "Please select Program")]
        [Display(Name = "Program")]
        public int ProgramId { get; set; }

        [Required(ErrorMessage = "Please select College")]
        [Display(Name = "College")]
        public int CollegeId { get; set; }

        [Required(ErrorMessage = "Please select Academic Year")]
        [Display(Name = "Academic Year")]
        public int AcademicYearId { get; set; }

        [Required(ErrorMessage = "Please select Current Semester")]
        [Display(Name = "Semester")]
        public int CurrentSemesterId { get; set; }

        [Display(Name = "Previous Qualification (e.g. +2 Science, Intermediate)")]
        public string? PreviousQualification { get; set; }

        [Display(Name = "Student Passport Photograph")]
        public IFormFile? PhotoFile { get; set; }

        [Display(Name = "Citizenship / Identity Document (PDF or Image)")]
        public IFormFile? IdentityDocumentFile { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ExamApplicationViewModel
    {
        [Required]
        public int ExaminationId { get; set; }

        public int StudentId { get; set; }

        [Required]
        [Display(Name = "Examination Type")]
        public string ExamType { get; set; } = "Regular";

        [Display(Name = "Fee Amount (NPR)")]
        public decimal FeeAmount { get; set; }

        [Display(Name = "Payment Reference / Transaction ID")]
        public string? PaymentReference { get; set; }

        [Display(Name = "Payment Voucher / Receipt")]
        public IFormFile? VoucherFile { get; set; }

        [Required(ErrorMessage = "Please select at least one subject")]
        public List<int> SelectedSubjectIds { get; set; } = new();

        // Helper displays
        public Examination? Examination { get; set; }
        public Student? Student { get; set; }
        public List<Subject> AvailableSubjects { get; set; } = new();
    }

    public class ResultSearchViewModel
    {
        [Display(Name = "Registration Number / Roll Number / Student ID")]
        public string SearchTerm { get; set; } = string.Empty;

        [Display(Name = "Faculty")]
        public int? FacultyId { get; set; }

        [Display(Name = "Program")]
        public int? ProgramId { get; set; }

        [Display(Name = "Exam Type")]
        public string? ExamType { get; set; }

        [Display(Name = "Exam Year")]
        public string? ExamYear { get; set; }

        [Display(Name = "Symbol Number")]
        public string? SymbolNumber { get; set; }

        [Display(Name = "Registration Number")]
        public string? RegistrationNumber { get; set; }

        public string? Rn1 { get; set; }
        public string? Rn2 { get; set; }
        public string? Rn3 { get; set; }
        public string? Rn4 { get; set; }
        public string? Rn5 { get; set; }

        [Display(Name = "Semester / Year")]
        public int? SemesterYearId { get; set; }

        [Display(Name = "Exam Intake")]
        public string? ExamIntake { get; set; }

        [Display(Name = "Examination")]
        public int? ExaminationId { get; set; }

        public Result? Result { get; set; }
        public bool Searched { get; set; }
        public string? Message { get; set; }
        public List<GradeScale> GradeScales { get; set; } = new();
    }

    public class AdminDashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int RegisteredStudents { get; set; }
        public int PendingRegistrations { get; set; }
        public int TotalColleges { get; set; }
        public int TotalFaculties { get; set; }
        public int TotalPrograms { get; set; }
        public int TotalSubjects { get; set; }
        public int TotalExaminations { get; set; }
        public int TotalApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int AdmitCardsGenerated { get; set; }
        public int ResultsPublished { get; set; }

        public List<ExaminationApplication> RecentApplications { get; set; } = new();
        public List<Student> RecentStudents { get; set; } = new();
        public List<Notice> RecentNotices { get; set; } = new();

        // Chart data
        public List<string> CollegeNames { get; set; } = new();
        public List<int> CollegeStudentCounts { get; set; } = new();
        public List<string> ProgramNames { get; set; } = new();
        public List<int> ProgramStudentCounts { get; set; } = new();
    }

    public class MarksEntryViewModel
    {
        public int ExaminationId { get; set; }
        public int ProgramId { get; set; }
        public int SemesterId { get; set; }
        public int SubjectId { get; set; }
        public int? CollegeId { get; set; }

        public Subject? Subject { get; set; }
        public Examination? Examination { get; set; }
        public List<MarkEntryRow> Rows { get; set; } = new();
    }

    public class MarkEntryRow
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string RollNumber { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public decimal InternalMarks { get; set; }
        public decimal ExternalMarks { get; set; }
        public decimal PracticalMarks { get; set; }
        public decimal TotalMarks { get; set; }
        public string Grade { get; set; } = "F";
        public decimal GradePoint { get; set; }
        public bool IsAbsent { get; set; }
        public string Status { get; set; } = "Draft";
    }
}
