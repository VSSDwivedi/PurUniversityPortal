namespace UniversityExamSystem.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string UserType { get; set; } = string.Empty; // SuperAdmin, Admin, Controller, CollegeAdmin, Examiner, Student
        public int? ReferenceId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Navigation / joined helpers
        public string RoleName { get; set; } = string.Empty;
        public string? CollegeName { get; set; }
        public string? ProgramName { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? StudentUniqueId { get; set; }
    }

    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}
