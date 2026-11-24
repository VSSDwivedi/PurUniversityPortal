namespace UniversityExamSystem.Models
{
    public class Faculty
    {
        public int FacultyId { get; set; }
        public string FacultyCode { get; set; } = string.Empty;
        public string FacultyName { get; set; } = string.Empty;
        public string? DeanName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public int ProgramCount { get; set; }
    }

    public class College
    {
        public int CollegeId { get; set; }
        public string CollegeCode { get; set; } = string.Empty;
        public string CollegeName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PrincipalName { get; set; }
        public DateTime? AffiliationDate { get; set; }
        public string? Website { get; set; }
        public string? LogoUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public int StudentCount { get; set; }
    }

    public class ProgramModel
    {
        public int ProgramId { get; set; }
        public string ProgramCode { get; set; } = string.Empty;
        public string ProgramName { get; set; } = string.Empty;
        public int FacultyId { get; set; }
        public string? FacultyName { get; set; }
        public string DegreeLevel { get; set; } = "Bachelor";
        public decimal DurationYears { get; set; } = 4.0m;
        public int TotalSemesters { get; set; } = 8;
        public int TotalCredits { get; set; } = 120;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class Subject
    {
        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int ProgramId { get; set; }
        public string? ProgramName { get; set; }
        public int SemesterId { get; set; }
        public string? SemesterName { get; set; }
        public int CreditHours { get; set; } = 3;
        public int FullMarks { get; set; } = 100;
        public int PassMarks { get; set; } = 45;
        public int InternalMarks { get; set; } = 40;
        public int ExternalMarks { get; set; } = 60;
        public int PracticalMarks { get; set; } = 0;
        public bool HasPractical { get; set; }
        public bool IsElective { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class AcademicYear
    {
        public int AcademicYearId { get; set; }
        public string YearName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsCurrent { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class Semester
    {
        public int SemesterId { get; set; }
        public string SemesterName { get; set; } = string.Empty;
        public int SemesterNumber { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class GradeScale
    {
        public int GradeScaleId { get; set; }
        public string ProgramLevel { get; set; } = "All";
        public decimal MinPercentage { get; set; }
        public decimal MaxPercentage { get; set; }
        public string Grade { get; set; } = string.Empty;
        public decimal GradePoint { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class ExaminationCenter
    {
        public int CenterId { get; set; }
        public string CenterCode { get; set; } = string.Empty;
        public string CenterName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int Capacity { get; set; } = 500;
        public string? SuperintendentName { get; set; }
        public string? ContactNumber { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
