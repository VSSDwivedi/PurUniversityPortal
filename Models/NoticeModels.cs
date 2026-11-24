namespace UniversityExamSystem.Models
{
    public class Notice
    {
        public int NoticeId { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? BadgeClass { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime PublishDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? AttachmentUrl { get; set; }
        public string? AttachmentFileName { get; set; }
        public bool IsPinned { get; set; }
        public bool IsActive { get; set; } = true;
        public int ViewCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class NoticeCategory
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string BadgeClass { get; set; } = "badge bg-primary";
        public bool IsActive { get; set; } = true;
        public int NoticeCount { get; set; }
    }

    public class SystemSetting
    {
        public int SettingId { get; set; }
        public string SettingKey { get; set; } = string.Empty;
        public string SettingValue { get; set; } = string.Empty;
        public string SettingGroup { get; set; } = "General";
        public string? Description { get; set; }
    }

    public class AuditLog
    {
        public int LogId { get; set; }
        public int? UserId { get; set; }
        public string? Username { get; set; }
        public string Action { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public string? RecordId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? IpAddress { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
