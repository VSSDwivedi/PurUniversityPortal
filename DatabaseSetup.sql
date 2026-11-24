-- ============================================================================
-- PURBANCHAL UNIVERSITY EXAMINATION MANAGEMENT SYSTEM (PurvanchalExamDB)
-- Reference: puexam.edu.np (Office of the Examination Management, Biratnagar)
-- Database Setup Script: Tables, Constraints, Stored Procedures, and Seed Data
-- ============================================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'PurvanchalExamDB')
BEGIN
    CREATE DATABASE PurvanchalExamDB;
END
GO

USE PurvanchalExamDB;
GO

-- Drop tables in reverse dependency order if re-running
IF OBJECT_ID('dbo.AuditLogs', 'U') IS NOT NULL DROP TABLE dbo.AuditLogs;
IF OBJECT_ID('dbo.PasswordResetTokens', 'U') IS NOT NULL DROP TABLE dbo.PasswordResetTokens;
IF OBJECT_ID('dbo.SystemSettings', 'U') IS NOT NULL DROP TABLE dbo.SystemSettings;
IF OBJECT_ID('dbo.UploadedFiles', 'U') IS NOT NULL DROP TABLE dbo.UploadedFiles;
IF OBJECT_ID('dbo.StudentGradeSheets', 'U') IS NOT NULL DROP TABLE dbo.StudentGradeSheets;
IF OBJECT_ID('dbo.ResultDetails', 'U') IS NOT NULL DROP TABLE dbo.ResultDetails;
IF OBJECT_ID('dbo.Results', 'U') IS NOT NULL DROP TABLE dbo.Results;
IF OBJECT_ID('dbo.Marks', 'U') IS NOT NULL DROP TABLE dbo.Marks;
IF OBJECT_ID('dbo.AdmitCards', 'U') IS NOT NULL DROP TABLE dbo.AdmitCards;
IF OBJECT_ID('dbo.ExaminationApplicationSubjects', 'U') IS NOT NULL DROP TABLE dbo.ExaminationApplicationSubjects;
IF OBJECT_ID('dbo.ExaminationApplications', 'U') IS NOT NULL DROP TABLE dbo.ExaminationApplications;
IF OBJECT_ID('dbo.ExaminationCenters', 'U') IS NOT NULL DROP TABLE dbo.ExaminationCenters;
IF OBJECT_ID('dbo.Examinations', 'U') IS NOT NULL DROP TABLE dbo.Examinations;
IF OBJECT_ID('dbo.StudentDocuments', 'U') IS NOT NULL DROP TABLE dbo.StudentDocuments;
IF OBJECT_ID('dbo.StudentRegistrations', 'U') IS NOT NULL DROP TABLE dbo.StudentRegistrations;
IF OBJECT_ID('dbo.Students', 'U') IS NOT NULL DROP TABLE dbo.Students;
IF OBJECT_ID('dbo.GradeScales', 'U') IS NOT NULL DROP TABLE dbo.GradeScales;
IF OBJECT_ID('dbo.Subjects', 'U') IS NOT NULL DROP TABLE dbo.Subjects;
IF OBJECT_ID('dbo.Semesters', 'U') IS NOT NULL DROP TABLE dbo.Semesters;
IF OBJECT_ID('dbo.Programs', 'U') IS NOT NULL DROP TABLE dbo.Programs;
IF OBJECT_ID('dbo.Colleges', 'U') IS NOT NULL DROP TABLE dbo.Colleges;
IF OBJECT_ID('dbo.Faculties', 'U') IS NOT NULL DROP TABLE dbo.Faculties;
IF OBJECT_ID('dbo.AcademicYears', 'U') IS NOT NULL DROP TABLE dbo.AcademicYears;
IF OBJECT_ID('dbo.Notices', 'U') IS NOT NULL DROP TABLE dbo.Notices;
IF OBJECT_ID('dbo.NoticeCategories', 'U') IS NOT NULL DROP TABLE dbo.NoticeCategories;
IF OBJECT_ID('dbo.UserRoles', 'U') IS NOT NULL DROP TABLE dbo.UserRoles;
IF OBJECT_ID('dbo.Roles', 'U') IS NOT NULL DROP TABLE dbo.Roles;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO

-- 1. Roles Table
CREATE TABLE dbo.Roles (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(250) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedDate DATETIME2 NULL
);
GO

-- 2. Users Table
CREATE TABLE dbo.Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(150) NOT NULL,
    PhoneNumber NVARCHAR(20) NULL,
    UserType NVARCHAR(50) NOT NULL, -- SuperAdmin, Admin, Controller, CollegeAdmin, Examiner, Student
    ReferenceId INT NULL, -- CollegeId or StudentId depending on UserType
    IsActive BIT NOT NULL DEFAULT 1,
    LastLoginDate DATETIME2 NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedDate DATETIME2 NULL
);
GO

-- 3. UserRoles Table
CREATE TABLE dbo.UserRoles (
    UserRoleId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    AssignedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES dbo.Roles(RoleId) ON DELETE CASCADE,
    CONSTRAINT UQ_User_Role UNIQUE (UserId, RoleId)
);
GO

-- 4. Faculties Table
CREATE TABLE dbo.Faculties (
    FacultyId INT IDENTITY(1,1) PRIMARY KEY,
    FacultyCode NVARCHAR(20) NOT NULL UNIQUE,
    FacultyName NVARCHAR(150) NOT NULL,
    DeanName NVARCHAR(100) NULL,
    Description NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedDate DATETIME2 NULL
);
GO

-- 5. Colleges Table
CREATE TABLE dbo.Colleges (
    CollegeId INT IDENTITY(1,1) PRIMARY KEY,
    CollegeCode NVARCHAR(20) NOT NULL UNIQUE,
    CollegeName NVARCHAR(200) NOT NULL,
    Address NVARCHAR(300) NOT NULL,
    ContactNumber NVARCHAR(50) NOT NULL,
    Email NVARCHAR(150) NOT NULL,
    PrincipalName NVARCHAR(150) NULL,
    AffiliationDate DATE NULL,
    Website NVARCHAR(200) NULL,
    LogoUrl NVARCHAR(300) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedDate DATETIME2 NULL
);
GO

-- 6. Programs Table
CREATE TABLE dbo.Programs (
    ProgramId INT IDENTITY(1,1) PRIMARY KEY,
    ProgramCode NVARCHAR(20) NOT NULL UNIQUE,
    ProgramName NVARCHAR(150) NOT NULL,
    FacultyId INT NOT NULL,
    DegreeLevel NVARCHAR(50) NOT NULL, -- Bachelor, Master, PhD, Diploma
    DurationYears DECIMAL(3,1) NOT NULL DEFAULT 4.0,
    TotalSemesters INT NOT NULL DEFAULT 8,
    TotalCredits INT NOT NULL DEFAULT 120,
    Description NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedDate DATETIME2 NULL,
    CONSTRAINT FK_Programs_Faculties FOREIGN KEY (FacultyId) REFERENCES dbo.Faculties(FacultyId)
);
GO

-- 7. AcademicYears Table
CREATE TABLE dbo.AcademicYears (
    AcademicYearId INT IDENTITY(1,1) PRIMARY KEY,
    YearName NVARCHAR(50) NOT NULL UNIQUE, -- e.g. 2024-2025, 2025-2026
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    IsCurrent BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedDate DATETIME2 NULL
);
GO

-- 8. Semesters Table
CREATE TABLE dbo.Semesters (
    SemesterId INT IDENTITY(1,1) PRIMARY KEY,
    SemesterName NVARCHAR(50) NOT NULL, -- 1st Semester, 2nd Semester, etc.
    SemesterNumber INT NOT NULL,
    Description NVARCHAR(100) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- 9. Subjects Table
CREATE TABLE dbo.Subjects (
    SubjectId INT IDENTITY(1,1) PRIMARY KEY,
    SubjectCode NVARCHAR(30) NOT NULL,
    SubjectName NVARCHAR(200) NOT NULL,
    ProgramId INT NOT NULL,
    SemesterId INT NOT NULL,
    CreditHours INT NOT NULL DEFAULT 3,
    FullMarks INT NOT NULL DEFAULT 100,
    PassMarks INT NOT NULL DEFAULT 45,
    InternalMarks INT NOT NULL DEFAULT 40,
    ExternalMarks INT NOT NULL DEFAULT 60,
    PracticalMarks INT NOT NULL DEFAULT 0,
    HasPractical BIT NOT NULL DEFAULT 0,
    IsElective BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedDate DATETIME2 NULL,
    CONSTRAINT FK_Subjects_Programs FOREIGN KEY (ProgramId) REFERENCES dbo.Programs(ProgramId),
    CONSTRAINT FK_Subjects_Semesters FOREIGN KEY (SemesterId) REFERENCES dbo.Semesters(SemesterId),
    CONSTRAINT UQ_Subject_Program_Code UNIQUE (ProgramId, SubjectCode)
);
GO

-- 10. GradeScales Table (Configurable Grading System)
CREATE TABLE dbo.GradeScales (
    GradeScaleId INT IDENTITY(1,1) PRIMARY KEY,
    ProgramLevel NVARCHAR(50) NOT NULL DEFAULT 'All', -- All, Bachelor, Master
    MinPercentage DECIMAL(5,2) NOT NULL,
    MaxPercentage DECIMAL(5,2) NOT NULL,
    Grade NVARCHAR(10) NOT NULL,
    GradePoint DECIMAL(3,2) NOT NULL,
    Description NVARCHAR(50) NOT NULL, -- Outstanding, Excellent, Very Good, etc.
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- 11. Students Table
CREATE TABLE dbo.Students (
    StudentId INT IDENTITY(1,1) PRIMARY KEY,
    StudentUniqueId NVARCHAR(50) NOT NULL UNIQUE, -- e.g. AU2024-00101
    RegistrationNumber NVARCHAR(50) NULL, -- Generated on approval, e.g. 2024-1-01-0001
    RollNumber NVARCHAR(50) NULL,
    UserId INT NULL,
    FullName NVARCHAR(150) NOT NULL,
    DateOfBirth DATE NOT NULL,
    Gender NVARCHAR(20) NOT NULL,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    MobileNumber NVARCHAR(20) NOT NULL,
    Address NVARCHAR(300) NOT NULL,
    FacultyId INT NOT NULL,
    ProgramId INT NOT NULL,
    CollegeId INT NOT NULL,
    AcademicYearId INT NOT NULL,
    CurrentSemesterId INT NOT NULL,
    PreviousQualification NVARCHAR(200) NULL,
    PhotoUrl NVARCHAR(300) NULL,
    IdentityDocumentUrl NVARCHAR(300) NULL,
    RegistrationStatus NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Approved, Rejected, CorrectionRequired
    VerificationRemarks NVARCHAR(500) NULL,
    VerifiedBy INT NULL,
    VerifiedDate DATETIME2 NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedDate DATETIME2 NULL,
    CONSTRAINT FK_Students_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_Students_Faculties FOREIGN KEY (FacultyId) REFERENCES dbo.Faculties(FacultyId),
    CONSTRAINT FK_Students_Programs FOREIGN KEY (ProgramId) REFERENCES dbo.Programs(ProgramId),
    CONSTRAINT FK_Students_Colleges FOREIGN KEY (CollegeId) REFERENCES dbo.Colleges(CollegeId),
    CONSTRAINT FK_Students_AcademicYears FOREIGN KEY (AcademicYearId) REFERENCES dbo.AcademicYears(AcademicYearId),
    CONSTRAINT FK_Students_Semesters FOREIGN KEY (CurrentSemesterId) REFERENCES dbo.Semesters(SemesterId)
);
GO

-- 12. StudentRegistrations Table (Tracking history and application log)
CREATE TABLE dbo.StudentRegistrations (
    RegistrationId INT IDENTITY(1,1) PRIMARY KEY,
    StudentId INT NOT NULL,
    ApplicationNumber NVARCHAR(50) NOT NULL UNIQUE,
    SubmissionDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Under Review, Approved, Rejected, Correction
    ProcessedDate DATETIME2 NULL,
    ProcessedBy INT NULL,
    Remarks NVARCHAR(500) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_StudentRegistrations_Students FOREIGN KEY (StudentId) REFERENCES dbo.Students(StudentId) ON DELETE CASCADE
);
GO

-- 13. StudentDocuments Table
CREATE TABLE dbo.StudentDocuments (
    DocumentId INT IDENTITY(1,1) PRIMARY KEY,
    StudentId INT NOT NULL,
    DocumentType NVARCHAR(100) NOT NULL, -- Citizenship, Transcript, Photo, Character Certificate, Migration
    DocumentName NVARCHAR(200) NOT NULL,
    FilePath NVARCHAR(300) NOT NULL,
    FileSizeKB BIGINT NULL,
    UploadedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_StudentDocuments_Students FOREIGN KEY (StudentId) REFERENCES dbo.Students(StudentId) ON DELETE CASCADE
);
GO

-- 14. ExaminationCenters Table
CREATE TABLE dbo.ExaminationCenters (
    CenterId INT IDENTITY(1,1) PRIMARY KEY,
    CenterCode NVARCHAR(20) NOT NULL UNIQUE,
    CenterName NVARCHAR(150) NOT NULL,
    Location NVARCHAR(200) NOT NULL,
    Capacity INT NOT NULL DEFAULT 500,
    SuperintendentName NVARCHAR(100) NULL,
    ContactNumber NVARCHAR(50) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- 15. Examinations Table
CREATE TABLE dbo.Examinations (
    ExaminationId INT IDENTITY(1,1) PRIMARY KEY,
    ExamCode NVARCHAR(50) NOT NULL UNIQUE,
    ExamName NVARCHAR(200) NOT NULL, -- e.g. Spring 2026 Semester Examination
    AcademicYearId INT NOT NULL,
    SemesterId INT NOT NULL,
    FacultyId INT NULL,
    ProgramId INT NULL, -- NULL means all programs under semester
    ExamType NVARCHAR(50) NOT NULL DEFAULT 'Regular', -- Regular, Back, Chance, Make-up
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    ApplicationDeadline DATE NOT NULL,
    LateFeeDeadline DATE NULL,
    FeeAmount DECIMAL(10,2) NOT NULL DEFAULT 1500.00,
    LateFeeAmount DECIMAL(10,2) NOT NULL DEFAULT 500.00,
    DefaultCenterId INT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Upcoming', -- Upcoming, Ongoing, Closed, ResultProcessed, ResultPublished
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedDate DATETIME2 NULL,
    CONSTRAINT FK_Examinations_AcademicYears FOREIGN KEY (AcademicYearId) REFERENCES dbo.AcademicYears(AcademicYearId),
    CONSTRAINT FK_Examinations_Semesters FOREIGN KEY (SemesterId) REFERENCES dbo.Semesters(SemesterId),
    CONSTRAINT FK_Examinations_Centers FOREIGN KEY (DefaultCenterId) REFERENCES dbo.ExaminationCenters(CenterId)
);
GO

-- 16. ExaminationApplications Table
CREATE TABLE dbo.ExaminationApplications (
    ApplicationId INT IDENTITY(1,1) PRIMARY KEY,
    ApplicationNumber NVARCHAR(50) NOT NULL UNIQUE,
    ExaminationId INT NOT NULL,
    StudentId INT NOT NULL,
    CollegeId INT NOT NULL,
    ProgramId INT NOT NULL,
    SemesterId INT NOT NULL,
    ExamType NVARCHAR(50) NOT NULL DEFAULT 'Regular', -- Regular, Back
    AssignedRollNumber NVARCHAR(50) NULL,
    ExaminationCenterId INT NULL,
    FeePaid DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    PaymentReference NVARCHAR(100) NULL,
    PaymentVoucherUrl NVARCHAR(300) NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Approved, Rejected, CorrectionRequired
    Remarks NVARCHAR(500) NULL,
    SubmissionDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    ReviewedBy INT NULL,
    ReviewedDate DATETIME2 NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedDate DATETIME2 NULL,
    CONSTRAINT FK_ExamApps_Examinations FOREIGN KEY (ExaminationId) REFERENCES dbo.Examinations(ExaminationId),
    CONSTRAINT FK_ExamApps_Students FOREIGN KEY (StudentId) REFERENCES dbo.Students(StudentId),
    CONSTRAINT FK_ExamApps_Colleges FOREIGN KEY (CollegeId) REFERENCES dbo.Colleges(CollegeId),
    CONSTRAINT FK_ExamApps_Programs FOREIGN KEY (ProgramId) REFERENCES dbo.Programs(ProgramId),
    CONSTRAINT FK_ExamApps_Semesters FOREIGN KEY (SemesterId) REFERENCES dbo.Semesters(SemesterId),
    CONSTRAINT FK_ExamApps_Centers FOREIGN KEY (ExaminationCenterId) REFERENCES dbo.ExaminationCenters(CenterId)
);
GO

-- 17. ExaminationApplicationSubjects Table
CREATE TABLE dbo.ExaminationApplicationSubjects (
    AppSubjectId INT IDENTITY(1,1) PRIMARY KEY,
    ApplicationId INT NOT NULL,
    SubjectId INT NOT NULL,
    SubjectType NVARCHAR(50) NOT NULL DEFAULT 'Regular', -- Regular, Backlog
    ExamDate DATE NULL,
    ExamTime NVARCHAR(50) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_AppSubjects_Apps FOREIGN KEY (ApplicationId) REFERENCES dbo.ExaminationApplications(ApplicationId) ON DELETE CASCADE,
    CONSTRAINT FK_AppSubjects_Subjects FOREIGN KEY (SubjectId) REFERENCES dbo.Subjects(SubjectId),
    CONSTRAINT UQ_App_Subject UNIQUE (ApplicationId, SubjectId)
);
GO

-- 18. AdmitCards Table
CREATE TABLE dbo.AdmitCards (
    AdmitCardId INT IDENTITY(1,1) PRIMARY KEY,
    AdmitCardNumber NVARCHAR(50) NOT NULL UNIQUE,
    ApplicationId INT NOT NULL UNIQUE,
    StudentId INT NOT NULL,
    ExaminationId INT NOT NULL,
    RollNumber NVARCHAR(50) NOT NULL,
    RegistrationNumber NVARCHAR(50) NOT NULL,
    ExaminationCenterId INT NOT NULL,
    QrCodeHash NVARCHAR(255) NOT NULL,
    GeneratedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    GeneratedBy INT NULL,
    IsDownloaded BIT NOT NULL DEFAULT 0,
    DownloadCount INT NOT NULL DEFAULT 0,
    LastDownloadedDate DATETIME2 NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_AdmitCards_Applications FOREIGN KEY (ApplicationId) REFERENCES dbo.ExaminationApplications(ApplicationId),
    CONSTRAINT FK_AdmitCards_Students FOREIGN KEY (StudentId) REFERENCES dbo.Students(StudentId),
    CONSTRAINT FK_AdmitCards_Examinations FOREIGN KEY (ExaminationId) REFERENCES dbo.Examinations(ExaminationId),
    CONSTRAINT FK_AdmitCards_Centers FOREIGN KEY (ExaminationCenterId) REFERENCES dbo.ExaminationCenters(CenterId)
);
GO

-- 19. Marks Table
CREATE TABLE dbo.Marks (
    MarkId INT IDENTITY(1,1) PRIMARY KEY,
    ExaminationId INT NOT NULL,
    StudentId INT NOT NULL,
    SubjectId INT NOT NULL,
    InternalMarks DECIMAL(5,2) NOT NULL DEFAULT 0.00,
    ExternalMarks DECIMAL(5,2) NOT NULL DEFAULT 0.00,
    PracticalMarks DECIMAL(5,2) NOT NULL DEFAULT 0.00,
    TotalMarks DECIMAL(5,2) NOT NULL DEFAULT 0.00,
    Grade NVARCHAR(10) NULL,
    GradePoint DECIMAL(3,2) NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Draft', -- Draft, Submitted, Approved, Locked
    IsAbsent BIT NOT NULL DEFAULT 0,
    IsWithheld BIT NOT NULL DEFAULT 0,
    Remarks NVARCHAR(200) NULL,
    EnteredBy INT NULL,
    EnteredDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    ApprovedBy INT NULL,
    ApprovedDate DATETIME2 NULL,
    LockedDate DATETIME2 NULL,
    CONSTRAINT FK_Marks_Examinations FOREIGN KEY (ExaminationId) REFERENCES dbo.Examinations(ExaminationId),
    CONSTRAINT FK_Marks_Students FOREIGN KEY (StudentId) REFERENCES dbo.Students(StudentId),
    CONSTRAINT FK_Marks_Subjects FOREIGN KEY (SubjectId) REFERENCES dbo.Subjects(SubjectId),
    CONSTRAINT UQ_Exam_Student_Subject UNIQUE (ExaminationId, StudentId, SubjectId)
);
GO

-- 20. Results Table (Header for student semester result)
CREATE TABLE dbo.Results (
    ResultId INT IDENTITY(1,1) PRIMARY KEY,
    ExaminationId INT NOT NULL,
    StudentId INT NOT NULL,
    ProgramId INT NOT NULL,
    SemesterId INT NOT NULL,
    CollegeId INT NOT NULL,
    RollNumber NVARCHAR(50) NOT NULL,
    RegistrationNumber NVARCHAR(50) NOT NULL,
    TotalMarksObtained DECIMAL(6,2) NOT NULL DEFAULT 0.00,
    TotalFullMarks DECIMAL(6,2) NOT NULL DEFAULT 0.00,
    TotalCreditHours INT NOT NULL DEFAULT 0,
    Percentage DECIMAL(5,2) NOT NULL DEFAULT 0.00,
    SGPA DECIMAL(3,2) NOT NULL DEFAULT 0.00,
    CGPA DECIMAL(3,2) NOT NULL DEFAULT 0.00,
    ResultStatus NVARCHAR(50) NOT NULL DEFAULT 'Passed', -- Passed, Failed, Withheld, Incomplete, Absent
    FailedSubjectCount INT NOT NULL DEFAULT 0,
    IsPublished BIT NOT NULL DEFAULT 0,
    PublishedDate DATETIME2 NULL,
    PublishedBy INT NULL,
    CalculatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Results_Examinations FOREIGN KEY (ExaminationId) REFERENCES dbo.Examinations(ExaminationId),
    CONSTRAINT FK_Results_Students FOREIGN KEY (StudentId) REFERENCES dbo.Students(StudentId),
    CONSTRAINT FK_Results_Programs FOREIGN KEY (ProgramId) REFERENCES dbo.Programs(ProgramId),
    CONSTRAINT FK_Results_Semesters FOREIGN KEY (SemesterId) REFERENCES dbo.Semesters(SemesterId),
    CONSTRAINT FK_Results_Colleges FOREIGN KEY (CollegeId) REFERENCES dbo.Colleges(CollegeId),
    CONSTRAINT UQ_Result_Exam_Student UNIQUE (ExaminationId, StudentId)
);
GO

-- 21. ResultDetails Table (Subject breakdown of result)
CREATE TABLE dbo.ResultDetails (
    ResultDetailId INT IDENTITY(1,1) PRIMARY KEY,
    ResultId INT NOT NULL,
    SubjectId INT NOT NULL,
    InternalMarks DECIMAL(5,2) NOT NULL DEFAULT 0.00,
    ExternalMarks DECIMAL(5,2) NOT NULL DEFAULT 0.00,
    PracticalMarks DECIMAL(5,2) NOT NULL DEFAULT 0.00,
    TotalMarks DECIMAL(5,2) NOT NULL DEFAULT 0.00,
    Grade NVARCHAR(10) NOT NULL,
    GradePoint DECIMAL(3,2) NOT NULL,
    IsPass BIT NOT NULL DEFAULT 1,
    IsAbsent BIT NOT NULL DEFAULT 0,
    Remarks NVARCHAR(100) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_ResultDetails_Results FOREIGN KEY (ResultId) REFERENCES dbo.Results(ResultId) ON DELETE CASCADE,
    CONSTRAINT FK_ResultDetails_Subjects FOREIGN KEY (SubjectId) REFERENCES dbo.Subjects(SubjectId)
);
GO

-- 22. StudentGradeSheets Table
CREATE TABLE dbo.StudentGradeSheets (
    GradeSheetId INT IDENTITY(1,1) PRIMARY KEY,
    GradeSheetNumber NVARCHAR(50) NOT NULL UNIQUE,
    ResultId INT NOT NULL UNIQUE,
    StudentId INT NOT NULL,
    IssueDate DATE NOT NULL,
    QrCodeHash NVARCHAR(255) NOT NULL,
    ControllerSignatureUrl NVARCHAR(300) NULL,
    IsPrinted BIT NOT NULL DEFAULT 0,
    PrintedDate DATETIME2 NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_GradeSheets_Results FOREIGN KEY (ResultId) REFERENCES dbo.Results(ResultId),
    CONSTRAINT FK_GradeSheets_Students FOREIGN KEY (StudentId) REFERENCES dbo.Students(StudentId)
);
GO

-- 23. NoticeCategories Table
CREATE TABLE dbo.NoticeCategories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL UNIQUE,
    BadgeClass NVARCHAR(50) NOT NULL DEFAULT 'badge bg-primary',
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- 24. Notices Table
CREATE TABLE dbo.Notices (
    NoticeId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryId INT NOT NULL,
    Title NVARCHAR(300) NOT NULL,
    Slug NVARCHAR(350) NULL,
    Description NVARCHAR(MAX) NOT NULL,
    PublishDate DATE NOT NULL,
    ExpiryDate DATE NULL,
    AttachmentUrl NVARCHAR(300) NULL,
    AttachmentFileName NVARCHAR(200) NULL,
    IsPinned BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    ViewCount INT NOT NULL DEFAULT 0,
    CreatedBy INT NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedDate DATETIME2 NULL,
    CONSTRAINT FK_Notices_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.NoticeCategories(CategoryId)
);
GO

-- 25. UploadedFiles Table
CREATE TABLE dbo.UploadedFiles (
    FileId INT IDENTITY(1,1) PRIMARY KEY,
    OriginalFileName NVARCHAR(255) NOT NULL,
    StoredFileName NVARCHAR(255) NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    ContentType NVARCHAR(100) NOT NULL,
    FileSizeKB BIGINT NOT NULL,
    UploadedBy INT NULL,
    Module NVARCHAR(100) NOT NULL, -- StudentDoc, NoticeAttachment, FeeReceipt, AdmitCardPhoto
    UploadedDate DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- 26. AuditLogs Table
CREATE TABLE dbo.AuditLogs (
    LogId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NULL,
    Action NVARCHAR(100) NOT NULL,
    TableName NVARCHAR(100) NOT NULL,
    RecordId NVARCHAR(50) NULL,
    OldValues NVARCHAR(MAX) NULL,
    NewValues NVARCHAR(MAX) NULL,
    IpAddress NVARCHAR(50) NULL,
    Timestamp DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- 27. PasswordResetTokens Table
CREATE TABLE dbo.PasswordResetTokens (
    TokenId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Token NVARCHAR(255) NOT NULL UNIQUE,
    ExpiryDate DATETIME2 NOT NULL,
    IsUsed BIT NOT NULL DEFAULT 0,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Tokens_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId) ON DELETE CASCADE
);
GO

-- 28. SystemSettings Table
CREATE TABLE dbo.SystemSettings (
    SettingId INT IDENTITY(1,1) PRIMARY KEY,
    SettingKey NVARCHAR(100) NOT NULL UNIQUE,
    SettingValue NVARCHAR(MAX) NOT NULL,
    SettingGroup NVARCHAR(100) NOT NULL DEFAULT 'General',
    Description NVARCHAR(250) NULL,
    UpdatedDate DATETIME2 NULL
);
GO

-- ============================================================================
-- INDEXES FOR HIGH-PERFORMANCE SEARCH & FILTERING
-- ============================================================================
CREATE UNIQUE NONCLUSTERED INDEX UQ_Students_RegistrationNumber ON dbo.Students(RegistrationNumber) WHERE RegistrationNumber IS NOT NULL;
CREATE INDEX IX_Students_RegNo ON dbo.Students(RegistrationNumber);
CREATE INDEX IX_Students_RollNo ON dbo.Students(RollNumber);
CREATE INDEX IX_Students_College ON dbo.Students(CollegeId);
CREATE INDEX IX_Students_Program ON dbo.Students(ProgramId);
CREATE INDEX IX_ExamApps_ExamStudent ON dbo.ExaminationApplications(ExaminationId, StudentId);
CREATE INDEX IX_Marks_ExamSubject ON dbo.Marks(ExaminationId, SubjectId);
CREATE INDEX IX_Results_RollExam ON dbo.Results(RollNumber, ExaminationId);
CREATE INDEX IX_Notices_PublishDate ON dbo.Notices(PublishDate DESC);
GO

-- ============================================================================
-- STORED PROCEDURES (ADO.NET REQUIRED)
-- ============================================================================

-- SP: Register Student
IF OBJECT_ID('dbo.sp_RegisterStudent', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_RegisterStudent;
GO
CREATE PROCEDURE dbo.sp_RegisterStudent
    @FullName NVARCHAR(150),
    @DateOfBirth DATE,
    @Gender NVARCHAR(20),
    @Email NVARCHAR(150),
    @MobileNumber NVARCHAR(20),
    @Address NVARCHAR(300),
    @FacultyId INT,
    @ProgramId INT,
    @CollegeId INT,
    @AcademicYearId INT,
    @CurrentSemesterId INT,
    @PreviousQualification NVARCHAR(200),
    @PhotoUrl NVARCHAR(300) = NULL,
    @IdentityDocumentUrl NVARCHAR(300) = NULL,
    @PasswordHash NVARCHAR(255),
    @StudentId INT OUTPUT,
    @StudentUniqueId NVARCHAR(50) OUTPUT,
    @ApplicationNumber NVARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        -- Check if email already exists
        IF EXISTS (SELECT 1 FROM dbo.Users WHERE Email = @Email) OR EXISTS (SELECT 1 FROM dbo.Students WHERE Email = @Email)
        BEGIN
            THROW 50001, 'Email address is already registered in the system.', 1;
        END

        -- Generate unique Student ID e.g. AU2026-XXXXX
        DECLARE @YearPrefix NVARCHAR(10) = CAST(YEAR(GETDATE()) AS NVARCHAR(10));
        DECLARE @Seq INT;
        SELECT @Seq = ISNULL(MAX(StudentId), 0) + 1 FROM dbo.Students;
        SET @StudentUniqueId = 'AU' + @YearPrefix + '-' + RIGHT('00000' + CAST(@Seq AS NVARCHAR(10)), 5);
        SET @ApplicationNumber = 'REG' + @YearPrefix + RIGHT('000000' + CAST(@Seq AS NVARCHAR(10)), 6);

        -- Create User Account
        INSERT INTO dbo.Users (Username, Email, PasswordHash, FullName, PhoneNumber, UserType, IsActive, CreatedDate)
        VALUES (@StudentUniqueId, @Email, @PasswordHash, @FullName, @MobileNumber, 'Student', 1, GETDATE());

        DECLARE @NewUserId INT = SCOPE_IDENTITY();

        -- Assign Student Role
        DECLARE @StudentRoleId INT;
        SELECT @StudentRoleId = RoleId FROM dbo.Roles WHERE RoleName = 'Student';
        IF @StudentRoleId IS NOT NULL
        BEGIN
            INSERT INTO dbo.UserRoles (UserId, RoleId) VALUES (@NewUserId, @StudentRoleId);
        END

        -- Insert Student Profile
        INSERT INTO dbo.Students (
            StudentUniqueId, RegistrationNumber, UserId, FullName, DateOfBirth, Gender,
            Email, MobileNumber, Address, FacultyId, ProgramId, CollegeId,
            AcademicYearId, CurrentSemesterId, PreviousQualification, PhotoUrl,
            IdentityDocumentUrl, RegistrationStatus, IsActive, CreatedDate
        )
        VALUES (
            @StudentUniqueId, NULL, @NewUserId, @FullName, @DateOfBirth, @Gender,
            @Email, @MobileNumber, @Address, @FacultyId, @ProgramId, @CollegeId,
            @AcademicYearId, @CurrentSemesterId, @PreviousQualification, @PhotoUrl,
            @IdentityDocumentUrl, 'Pending', 1, GETDATE()
        );

        SET @StudentId = SCOPE_IDENTITY();

        -- Update Users ReferenceId to StudentId
        UPDATE dbo.Users SET ReferenceId = @StudentId WHERE UserId = @NewUserId;

        -- Create Student Registration Tracking Record
        INSERT INTO dbo.StudentRegistrations (StudentId, ApplicationNumber, SubmissionDate, Status, Remarks, CreatedDate)
        VALUES (@StudentId, @ApplicationNumber, GETDATE(), 'Pending', 'Registration submitted by student. Pending college/university verification.', GETDATE());

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- SP: Authenticate User
IF OBJECT_ID('dbo.sp_AuthenticateUser', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_AuthenticateUser;
GO
CREATE PROCEDURE dbo.sp_AuthenticateUser
    @LoginIdentifier NVARCHAR(150) -- Can be Email, Username, or Student RegistrationNumber
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP 1
        u.UserId,
        u.Username,
        u.Email,
        u.PasswordHash,
        u.FullName,
        u.PhoneNumber,
        u.UserType,
        u.ReferenceId,
        u.IsActive,
        r.RoleName,
        s.StudentId,
        s.StudentUniqueId,
        s.RegistrationNumber,
        s.RollNumber,
        s.ProgramId,
        s.CollegeId,
        c.CollegeName,
        p.ProgramName
    FROM dbo.Users u
    LEFT JOIN dbo.UserRoles ur ON u.UserId = ur.UserId
    LEFT JOIN dbo.Roles r ON ur.RoleId = r.RoleId
    LEFT JOIN dbo.Students s ON (u.ReferenceId = s.StudentId AND u.UserType = 'Student') OR (s.UserId = u.UserId)
    LEFT JOIN dbo.Colleges c ON (u.ReferenceId = c.CollegeId AND u.UserType = 'CollegeAdmin') OR (s.CollegeId = c.CollegeId)
    LEFT JOIN dbo.Programs p ON s.ProgramId = p.ProgramId
    WHERE (u.Email = @LoginIdentifier OR u.Username = @LoginIdentifier OR s.RegistrationNumber = @LoginIdentifier OR s.StudentUniqueId = @LoginIdentifier)
      AND u.IsActive = 1;
END;
GO

-- SP: Get Student Profile
IF OBJECT_ID('dbo.sp_GetStudentProfile', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_GetStudentProfile;
GO
CREATE PROCEDURE dbo.sp_GetStudentProfile
    @StudentId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        s.StudentId,
        s.StudentUniqueId,
        s.RegistrationNumber,
        s.RollNumber,
        s.FullName,
        s.DateOfBirth,
        s.Gender,
        s.Email,
        s.MobileNumber,
        s.Address,
        s.FacultyId,
        f.FacultyName,
        s.ProgramId,
        p.ProgramName,
        p.ProgramCode,
        s.CollegeId,
        c.CollegeName,
        c.CollegeCode,
        s.AcademicYearId,
        ay.YearName AS AcademicYear,
        s.CurrentSemesterId,
        sem.SemesterName AS CurrentSemester,
        s.PreviousQualification,
        s.PhotoUrl,
        s.IdentityDocumentUrl,
        s.RegistrationStatus,
        s.VerificationRemarks,
        s.VerifiedDate,
        s.CreatedDate,
        sr.ApplicationNumber,
        sr.Status AS RegistrationAppStatus
    FROM dbo.Students s
    INNER JOIN dbo.Faculties f ON s.FacultyId = f.FacultyId
    INNER JOIN dbo.Programs p ON s.ProgramId = p.ProgramId
    INNER JOIN dbo.Colleges c ON s.CollegeId = c.CollegeId
    INNER JOIN dbo.AcademicYears ay ON s.AcademicYearId = ay.AcademicYearId
    INNER JOIN dbo.Semesters sem ON s.CurrentSemesterId = sem.SemesterId
    LEFT JOIN dbo.StudentRegistrations sr ON s.StudentId = sr.StudentId
    WHERE s.StudentId = @StudentId;
END;
GO

-- SP: Verify Student Registration (Approve or Reject)
IF OBJECT_ID('dbo.sp_VerifyStudentRegistration', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_VerifyStudentRegistration;
GO
CREATE PROCEDURE dbo.sp_VerifyStudentRegistration
    @StudentId INT,
    @Status NVARCHAR(50), -- Approved, Rejected, CorrectionRequired
    @Remarks NVARCHAR(500) = NULL,
    @VerifiedBy INT = NULL,
    @GeneratedRegNumber NVARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        IF @Status = 'Approved'
        BEGIN
            -- Check if already has reg number
            SELECT @GeneratedRegNumber = RegistrationNumber FROM dbo.Students WHERE StudentId = @StudentId;
            IF @GeneratedRegNumber IS NULL OR @GeneratedRegNumber = ''
            BEGIN
                -- Generate Registration Number: puexam format [xxx]-[x]-[xx]-[xxxxx]-[yyyy]
                DECLARE @YearVal NVARCHAR(10) = CAST(YEAR(GETDATE()) AS NVARCHAR(10));
                DECLARE @FId INT, @PId INT, @CId INT;
                SELECT @FId = s.FacultyId, @PId = s.ProgramId, @CId = s.CollegeId
                FROM dbo.Students s
                WHERE s.StudentId = @StudentId;

                SET @GeneratedRegNumber = RIGHT('000' + CAST(ISNULL(@CId, 1) AS NVARCHAR(10)), 3) + '-' + 
                                          CAST(ISNULL(@FId, 1) AS NVARCHAR(10)) + '-' + 
                                          RIGHT('00' + CAST(ISNULL(@PId, 1) AS NVARCHAR(10)), 2) + '-' + 
                                          RIGHT('00000' + CAST(@StudentId AS NVARCHAR(10)), 5) + '-' + 
                                          @YearVal;
            END

            UPDATE dbo.Students
            SET RegistrationStatus = 'Approved',
                RegistrationNumber = @GeneratedRegNumber,
                VerificationRemarks = @Remarks,
                VerifiedBy = @VerifiedBy,
                VerifiedDate = GETDATE(),
                UpdatedDate = GETDATE()
            WHERE StudentId = @StudentId;

            UPDATE dbo.StudentRegistrations
            SET Status = 'Approved',
                ProcessedDate = GETDATE(),
                ProcessedBy = @VerifiedBy,
                Remarks = @Remarks
            WHERE StudentId = @StudentId;
        END
        ELSE
        BEGIN
            SET @GeneratedRegNumber = NULL;
            UPDATE dbo.Students
            SET RegistrationStatus = @Status,
                VerificationRemarks = @Remarks,
                VerifiedBy = @VerifiedBy,
                VerifiedDate = GETDATE(),
                UpdatedDate = GETDATE()
            WHERE StudentId = @StudentId;

            UPDATE dbo.StudentRegistrations
            SET Status = @Status,
                ProcessedDate = GETDATE(),
                ProcessedBy = @VerifiedBy,
                Remarks = @Remarks
            WHERE StudentId = @StudentId;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- SP: Submit Examination Application
IF OBJECT_ID('dbo.sp_SubmitExaminationApplication', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_SubmitExaminationApplication;
GO
CREATE PROCEDURE dbo.sp_SubmitExaminationApplication
    @ExaminationId INT,
    @StudentId INT,
    @ExamType NVARCHAR(50),
    @FeePaid DECIMAL(10,2),
    @PaymentReference NVARCHAR(100) = NULL,
    @PaymentVoucherUrl NVARCHAR(300) = NULL,
    @SubjectIdsList NVARCHAR(MAX), -- Comma separated subject IDs e.g. '1,2,3,4'
    @ApplicationId INT OUTPUT,
    @ApplicationNumber NVARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        -- Check if student is approved
        DECLARE @RegStatus NVARCHAR(50), @CollegeId INT, @ProgramId INT, @SemesterId INT;
        SELECT @RegStatus = RegistrationStatus, @CollegeId = CollegeId, @ProgramId = ProgramId, @SemesterId = CurrentSemesterId
        FROM dbo.Students WHERE StudentId = @StudentId;

        IF @RegStatus <> 'Approved'
        BEGIN
            THROW 50002, 'Student registration must be approved before applying for examination.', 1;
        END

        -- Check if application already exists for this examination
        IF EXISTS (SELECT 1 FROM dbo.ExaminationApplications WHERE ExaminationId = @ExaminationId AND StudentId = @StudentId)
        BEGIN
            THROW 50003, 'You have already submitted an application for this examination.', 1;
        END

        -- Generate Application Number e.g. EX2026-XXXXXX
        DECLARE @ExamYear NVARCHAR(10) = CAST(YEAR(GETDATE()) AS NVARCHAR(10));
        DECLARE @Seq INT;
        SELECT @Seq = ISNULL(MAX(ApplicationId), 0) + 1 FROM dbo.ExaminationApplications;
        SET @ApplicationNumber = 'EX' + @ExamYear + RIGHT('000000' + CAST(@Seq AS NVARCHAR(10)), 6);

        -- Insert Application
        INSERT INTO dbo.ExaminationApplications (
            ApplicationNumber, ExaminationId, StudentId, CollegeId, ProgramId, SemesterId,
            ExamType, FeePaid, PaymentReference, PaymentVoucherUrl, Status, SubmissionDate, CreatedDate
        )
        VALUES (
            @ApplicationNumber, @ExaminationId, @StudentId, @CollegeId, @ProgramId, @SemesterId,
            @ExamType, @FeePaid, @PaymentReference, @PaymentVoucherUrl, 'Pending', GETDATE(), GETDATE()
        );

        SET @ApplicationId = SCOPE_IDENTITY();

        -- Insert Selected Subjects from comma-delimited string
        INSERT INTO dbo.ExaminationApplicationSubjects (ApplicationId, SubjectId, SubjectType)
        SELECT @ApplicationId, CAST(value AS INT), @ExamType
        FROM STRING_SPLIT(@SubjectIdsList, ',')
        WHERE RTRIM(LTRIM(value)) <> '';

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- SP: Generate Admit Card
IF OBJECT_ID('dbo.sp_GenerateAdmitCard', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_GenerateAdmitCard;
GO
CREATE PROCEDURE dbo.sp_GenerateAdmitCard
    @ApplicationId INT,
    @AssignedRollNumber NVARCHAR(50),
    @ExaminationCenterId INT,
    @GeneratedBy INT = NULL,
    @AdmitCardId INT OUTPUT,
    @AdmitCardNumber NVARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        -- Check if application is approved
        DECLARE @Status NVARCHAR(50), @StudentId INT, @ExaminationId INT, @RegNumber NVARCHAR(50);
        SELECT @Status = ea.Status, @StudentId = ea.StudentId, @ExaminationId = ea.ExaminationId, @RegNumber = s.RegistrationNumber
        FROM dbo.ExaminationApplications ea
        JOIN dbo.Students s ON ea.StudentId = s.StudentId
        WHERE ea.ApplicationId = @ApplicationId;

        -- Update Application with RollNumber and Center, set Approved
        UPDATE dbo.ExaminationApplications
        SET Status = 'Approved',
            AssignedRollNumber = @AssignedRollNumber,
            ExaminationCenterId = @ExaminationCenterId,
            ReviewedBy = @GeneratedBy,
            ReviewedDate = GETDATE(),
            UpdatedDate = GETDATE()
        WHERE ApplicationId = @ApplicationId;

        -- Update Student Roll Number as well
        UPDATE dbo.Students SET RollNumber = @AssignedRollNumber WHERE StudentId = @StudentId;

        -- Check if admit card already generated
        SELECT @AdmitCardId = AdmitCardId, @AdmitCardNumber = AdmitCardNumber
        FROM dbo.AdmitCards WHERE ApplicationId = @ApplicationId;

        IF @AdmitCardId IS NULL
        BEGIN
            DECLARE @YearVal NVARCHAR(10) = CAST(YEAR(GETDATE()) AS NVARCHAR(10));
            DECLARE @Seq INT;
            SELECT @Seq = ISNULL(MAX(AdmitCardId), 0) + 1 FROM dbo.AdmitCards;
            SET @AdmitCardNumber = 'AC' + @YearVal + RIGHT('000000' + CAST(@Seq AS NVARCHAR(10)), 6);

            DECLARE @QrHash NVARCHAR(255) = 'VERIFY-AC:' + @AdmitCardNumber + ':' + @AssignedRollNumber + ':' + ISNULL(@RegNumber, '');

            INSERT INTO dbo.AdmitCards (
                AdmitCardNumber, ApplicationId, StudentId, ExaminationId, RollNumber,
                RegistrationNumber, ExaminationCenterId, QrCodeHash, GeneratedDate, GeneratedBy, IsActive
            )
            VALUES (
                @AdmitCardNumber, @ApplicationId, @StudentId, @ExaminationId, @AssignedRollNumber,
                ISNULL(@RegNumber, ''), @ExaminationCenterId, @QrHash, GETDATE(), @GeneratedBy, 1
            );

            SET @AdmitCardId = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            UPDATE dbo.AdmitCards
            SET RollNumber = @AssignedRollNumber,
                ExaminationCenterId = @ExaminationCenterId,
                GeneratedDate = GETDATE(),
                GeneratedBy = @GeneratedBy
            WHERE AdmitCardId = @AdmitCardId;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- SP: Save Marks Entry
IF OBJECT_ID('dbo.sp_SaveMarksEntry', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_SaveMarksEntry;
GO
CREATE PROCEDURE dbo.sp_SaveMarksEntry
    @ExaminationId INT,
    @StudentId INT,
    @SubjectId INT,
    @InternalMarks DECIMAL(5,2),
    @ExternalMarks DECIMAL(5,2),
    @PracticalMarks DECIMAL(5,2) = 0.00,
    @IsAbsent BIT = 0,
    @EnteredBy INT = NULL,
    @Status NVARCHAR(50) = 'Draft', -- Draft or Submitted
    @MarkId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        DECLARE @TotalMarks DECIMAL(5,2) = 0.00;
        DECLARE @Grade NVARCHAR(10) = 'F';
        DECLARE @GradePoint DECIMAL(3,2) = 0.00;

        IF @IsAbsent = 1
        BEGIN
            SET @InternalMarks = 0.00;
            SET @ExternalMarks = 0.00;
            SET @PracticalMarks = 0.00;
            SET @TotalMarks = 0.00;
            SET @Grade = 'ABS';
            SET @GradePoint = 0.00;
        END
        ELSE
        BEGIN
            SET @TotalMarks = @InternalMarks + @ExternalMarks + @PracticalMarks;

            -- Calculate Grade from GradeScales
            SELECT TOP 1 @Grade = Grade, @GradePoint = GradePoint
            FROM dbo.GradeScales
            WHERE @TotalMarks >= MinPercentage AND @TotalMarks <= MaxPercentage
            ORDER BY GradePoint DESC;

            IF @Grade IS NULL
            BEGIN
                SET @Grade = 'F';
                SET @GradePoint = 0.00;
            END
        END

        IF EXISTS (SELECT 1 FROM dbo.Marks WHERE ExaminationId = @ExaminationId AND StudentId = @StudentId AND SubjectId = @SubjectId)
        BEGIN
            -- Check if locked
            DECLARE @CurrentStatus NVARCHAR(50);
            SELECT @CurrentStatus = Status FROM dbo.Marks WHERE ExaminationId = @ExaminationId AND StudentId = @StudentId AND SubjectId = @SubjectId;
            IF @CurrentStatus = 'Locked'
            BEGIN
                THROW 50004, 'Cannot edit marks because they have been approved and locked.', 1;
            END

            UPDATE dbo.Marks
            SET InternalMarks = @InternalMarks,
                ExternalMarks = @ExternalMarks,
                PracticalMarks = @PracticalMarks,
                TotalMarks = @TotalMarks,
                Grade = @Grade,
                GradePoint = @GradePoint,
                IsAbsent = @IsAbsent,
                Status = @Status,
                EnteredBy = @EnteredBy,
                EnteredDate = GETDATE()
            WHERE ExaminationId = @ExaminationId AND StudentId = @StudentId AND SubjectId = @SubjectId;

            SELECT @MarkId = MarkId FROM dbo.Marks WHERE ExaminationId = @ExaminationId AND StudentId = @StudentId AND SubjectId = @SubjectId;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Marks (
                ExaminationId, StudentId, SubjectId, InternalMarks, ExternalMarks, PracticalMarks,
                TotalMarks, Grade, GradePoint, Status, IsAbsent, EnteredBy, EnteredDate
            )
            VALUES (
                @ExaminationId, @StudentId, @SubjectId, @InternalMarks, @ExternalMarks, @PracticalMarks,
                @TotalMarks, @Grade, @GradePoint, @Status, @IsAbsent, @EnteredBy, GETDATE()
            );

            SET @MarkId = SCOPE_IDENTITY();
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- SP: Process and Calculate Examination Results
IF OBJECT_ID('dbo.sp_ProcessExaminationResults', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_ProcessExaminationResults;
GO
CREATE PROCEDURE dbo.sp_ProcessExaminationResults
    @ExaminationId INT,
    @ProcessedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        -- Find distinct students who have marks in this examination
        DECLARE @StudentList TABLE (StudentId INT);
        INSERT INTO @StudentList (StudentId)
        SELECT DISTINCT StudentId FROM dbo.Marks WHERE ExaminationId = @ExaminationId;

        DECLARE @CurStudentId INT;
        DECLARE student_cursor CURSOR FOR SELECT StudentId FROM @StudentList;
        OPEN student_cursor;
        FETCH NEXT FROM student_cursor INTO @CurStudentId;

        WHILE @@FETCH_STATUS = 0
        BEGIN
            DECLARE @ProgramId INT, @SemesterId INT, @CollegeId INT, @RollNo NVARCHAR(50), @RegNo NVARCHAR(50);
            SELECT @ProgramId = ProgramId, @SemesterId = CurrentSemesterId, @CollegeId = CollegeId,
                   @RollNo = ISNULL(RollNumber, ''), @RegNo = ISNULL(RegistrationNumber, '')
            FROM dbo.Students WHERE StudentId = @CurStudentId;

            -- Aggregate Marks
            DECLARE @TotalObtained DECIMAL(6,2), @TotalFull DECIMAL(6,2), @TotalCredits INT, @WeightedPoints DECIMAL(8,2), @FailedCount INT, @IsAbsentAll BIT;
            
            SELECT 
                @TotalObtained = SUM(m.TotalMarks),
                @TotalFull = SUM(sub.FullMarks),
                @TotalCredits = SUM(sub.CreditHours),
                @WeightedPoints = SUM(m.GradePoint * sub.CreditHours),
                @FailedCount = SUM(CASE WHEN m.Grade = 'F' OR m.TotalMarks < sub.PassMarks THEN 1 ELSE 0 END),
                @IsAbsentAll = CASE WHEN MIN(CASE WHEN m.IsAbsent = 1 THEN 1 ELSE 0 END) = 1 THEN 1 ELSE 0 END
            FROM dbo.Marks m
            JOIN dbo.Subjects sub ON m.SubjectId = sub.SubjectId
            WHERE m.ExaminationId = @ExaminationId AND m.StudentId = @CurStudentId;

            DECLARE @Percentage DECIMAL(5,2) = 0.00;
            IF @TotalFull > 0 SET @Percentage = (@TotalObtained / @TotalFull) * 100.00;

            DECLARE @SGPA DECIMAL(3,2) = 0.00;
            IF @TotalCredits > 0 SET @SGPA = @WeightedPoints / @TotalCredits;

            DECLARE @ResultStatus NVARCHAR(50) = 'Passed';
            IF @IsAbsentAll = 1 SET @ResultStatus = 'Absent';
            ELSE IF @FailedCount > 0 SET @ResultStatus = 'Failed';

            -- Upsert into Results table
            DECLARE @ResultId INT;
            SELECT @ResultId = ResultId FROM dbo.Results WHERE ExaminationId = @ExaminationId AND StudentId = @CurStudentId;

            IF @ResultId IS NULL
            BEGIN
                INSERT INTO dbo.Results (
                    ExaminationId, StudentId, ProgramId, SemesterId, CollegeId, RollNumber,
                    RegistrationNumber, TotalMarksObtained, TotalFullMarks, TotalCreditHours,
                    Percentage, SGPA, CGPA, ResultStatus, FailedSubjectCount, IsPublished, CalculatedDate
                )
                VALUES (
                    @ExaminationId, @CurStudentId, @ProgramId, @SemesterId, @CollegeId, @RollNo,
                    @RegNo, @TotalObtained, @TotalFull, @TotalCredits,
                    @Percentage, @SGPA, @SGPA, @ResultStatus, @FailedCount, 0, GETDATE()
                );
                SET @ResultId = SCOPE_IDENTITY();
            END
            ELSE
            BEGIN
                UPDATE dbo.Results
                SET TotalMarksObtained = @TotalObtained,
                    TotalFullMarks = @TotalFull,
                    TotalCreditHours = @TotalCredits,
                    Percentage = @Percentage,
                    SGPA = @SGPA,
                    CGPA = @SGPA,
                    ResultStatus = @ResultStatus,
                    FailedSubjectCount = @FailedCount,
                    CalculatedDate = GETDATE()
                WHERE ResultId = @ResultId;

                DELETE FROM dbo.ResultDetails WHERE ResultId = @ResultId;
            END

            -- Populate ResultDetails
            INSERT INTO dbo.ResultDetails (
                ResultId, SubjectId, InternalMarks, ExternalMarks, PracticalMarks,
                TotalMarks, Grade, GradePoint, IsPass, IsAbsent, Remarks
            )
            SELECT 
                @ResultId, m.SubjectId, m.InternalMarks, m.ExternalMarks, m.PracticalMarks,
                m.TotalMarks, m.Grade, m.GradePoint,
                CASE WHEN m.Grade = 'F' OR m.TotalMarks < sub.PassMarks THEN 0 ELSE 1 END,
                m.IsAbsent,
                CASE WHEN m.IsAbsent = 1 THEN 'Absent' WHEN m.Grade = 'F' THEN 'Back' ELSE 'Passed' END
            FROM dbo.Marks m
            JOIN dbo.Subjects sub ON m.SubjectId = sub.SubjectId
            WHERE m.ExaminationId = @ExaminationId AND m.StudentId = @CurStudentId;

            FETCH NEXT FROM student_cursor INTO @CurStudentId;
        END

        CLOSE student_cursor;
        DEALLOCATE student_cursor;

        UPDATE dbo.Examinations SET Status = 'ResultProcessed', UpdatedDate = GETDATE() WHERE ExaminationId = @ExaminationId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- SP: Publish Examination Results
IF OBJECT_ID('dbo.sp_PublishExaminationResults', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_PublishExaminationResults;
GO
CREATE PROCEDURE dbo.sp_PublishExaminationResults
    @ExaminationId INT,
    @PublishedBy INT = NULL,
    @IsPublished BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        UPDATE dbo.Results
        SET IsPublished = @IsPublished,
            PublishedDate = CASE WHEN @IsPublished = 1 THEN GETDATE() ELSE NULL END,
            PublishedBy = CASE WHEN @IsPublished = 1 THEN @PublishedBy ELSE NULL END
        WHERE ExaminationId = @ExaminationId;

        UPDATE dbo.Examinations
        SET Status = CASE WHEN @IsPublished = 1 THEN 'ResultPublished' ELSE 'ResultProcessed' END,
            UpdatedDate = GETDATE()
        WHERE ExaminationId = @ExaminationId;

        -- Create/Ensure StudentGradeSheets records
        IF @IsPublished = 1
        BEGIN
            DECLARE @ResId INT, @StudId INT, @RNo NVARCHAR(50);
            DECLARE res_cursor CURSOR FOR 
                SELECT ResultId, StudentId, RollNumber FROM dbo.Results 
                WHERE ExaminationId = @ExaminationId AND ResultId NOT IN (SELECT ResultId FROM dbo.StudentGradeSheets);
            OPEN res_cursor;
            FETCH NEXT FROM res_cursor INTO @ResId, @StudId, @RNo;
            WHILE @@FETCH_STATUS = 0
            BEGIN
                DECLARE @GSNumber NVARCHAR(50) = 'GS' + CAST(YEAR(GETDATE()) AS NVARCHAR(10)) + RIGHT('000000' + CAST(@ResId AS NVARCHAR(10)), 6);
                DECLARE @QHash NVARCHAR(255) = 'VERIFY-GS:' + @GSNumber + ':' + @RNo;

                INSERT INTO dbo.StudentGradeSheets (GradeSheetNumber, ResultId, StudentId, IssueDate, QrCodeHash, CreatedDate)
                VALUES (@GSNumber, @ResId, @StudId, CAST(GETDATE() AS DATE), @QHash, GETDATE());

                FETCH NEXT FROM res_cursor INTO @ResId, @StudId, @RNo;
            END
            CLOSE res_cursor;
            DEALLOCATE res_cursor;
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- SP: Search Student Result
IF OBJECT_ID('dbo.sp_SearchStudentResult', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_SearchStudentResult;
GO
CREATE PROCEDURE dbo.sp_SearchStudentResult
    @SearchTerm NVARCHAR(100), -- Registration Number or Roll Number
    @ExaminationId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        r.ResultId,
        r.ExaminationId,
        e.ExamName,
        e.ExamCode,
        e.StartDate AS ExamDate,
        r.StudentId,
        s.FullName AS StudentName,
        r.RegistrationNumber,
        r.RollNumber,
        s.PhotoUrl,
        p.ProgramName,
        p.ProgramCode,
        c.CollegeName,
        c.CollegeCode,
        sem.SemesterName,
        r.TotalMarksObtained,
        r.TotalFullMarks,
        r.TotalCreditHours,
        r.Percentage,
        r.SGPA,
        r.CGPA,
        r.ResultStatus,
        r.FailedSubjectCount,
        r.IsPublished,
        r.PublishedDate,
        gs.GradeSheetNumber,
        gs.IssueDate AS GradeSheetIssueDate,
        gs.QrCodeHash AS GradeSheetQrCode
    FROM dbo.Results r
    JOIN dbo.Examinations e ON r.ExaminationId = e.ExaminationId
    JOIN dbo.Students s ON r.StudentId = s.StudentId
    JOIN dbo.Programs p ON r.ProgramId = p.ProgramId
    JOIN dbo.Colleges c ON r.CollegeId = c.CollegeId
    JOIN dbo.Semesters sem ON r.SemesterId = sem.SemesterId
    LEFT JOIN dbo.StudentGradeSheets gs ON r.ResultId = gs.ResultId
    WHERE (r.RegistrationNumber = @SearchTerm OR r.RollNumber = @SearchTerm OR s.StudentUniqueId = @SearchTerm)
      AND (@ExaminationId IS NULL OR r.ExaminationId = @ExaminationId)
      AND r.IsPublished = 1;
END;
GO

-- SP: Get Admin Dashboard Stats
IF OBJECT_ID('dbo.sp_GetAdminDashboardStats', 'P') IS NOT NULL DROP PROCEDURE dbo.sp_GetAdminDashboardStats;
GO
CREATE PROCEDURE dbo.sp_GetAdminDashboardStats
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        (SELECT COUNT(*) FROM dbo.Students) AS TotalStudents,
        (SELECT COUNT(*) FROM dbo.Students WHERE RegistrationStatus = 'Approved') AS RegisteredStudents,
        (SELECT COUNT(*) FROM dbo.Students WHERE RegistrationStatus = 'Pending') AS PendingRegistrations,
        (SELECT COUNT(*) FROM dbo.Colleges WHERE IsActive = 1) AS TotalColleges,
        (SELECT COUNT(*) FROM dbo.Faculties WHERE IsActive = 1) AS TotalFaculties,
        (SELECT COUNT(*) FROM dbo.Programs WHERE IsActive = 1) AS TotalPrograms,
        (SELECT COUNT(*) FROM dbo.Subjects WHERE IsActive = 1) AS TotalSubjects,
        (SELECT COUNT(*) FROM dbo.Examinations WHERE IsActive = 1) AS TotalExaminations,
        (SELECT COUNT(*) FROM dbo.ExaminationApplications) AS TotalApplications,
        (SELECT COUNT(*) FROM dbo.ExaminationApplications WHERE Status = 'Approved') AS ApprovedApplications,
        (SELECT COUNT(*) FROM dbo.AdmitCards WHERE IsActive = 1) AS AdmitCardsGenerated,
        (SELECT COUNT(*) FROM dbo.Results WHERE IsPublished = 1) AS ResultsPublished;
END;
GO

-- ============================================================================
-- SEED DATA POPULATION
-- ============================================================================

-- Seed Roles
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE RoleName = 'Super Admin')
BEGIN
    INSERT INTO dbo.Roles (RoleName, Description) VALUES
    ('Super Admin', 'Full system control, administrative oversight and database configuration'),
    ('Admin', 'Examination administration, student management, program setups'),
    ('Examination Controller', 'Official examination controller with result approval & publish authority'),
    ('College Admin', 'College-level coordinator managing affiliated students and exam applications'),
    ('Examiner', 'Examiner and subject evaluator authorized for marks entry and grading'),
    ('Student', 'Registered university student with access to admit cards, routines and results');
END
GO

-- Seed System Settings
IF NOT EXISTS (SELECT 1 FROM dbo.SystemSettings WHERE SettingKey = 'UniversityName')
BEGIN
    INSERT INTO dbo.SystemSettings (SettingKey, SettingValue, SettingGroup, Description) VALUES
    ('UniversityName', 'Purbanchal University', 'General', 'Full legal university name'),
    ('OfficeName', 'Office of the Examination Management', 'General', 'Examination branch name'),
    ('UniversityAddress', 'Puspalal Chowk, Biratnagar, Morang, Nepal', 'General', 'Official university examination office address'),
    ('ContactEmail', 'puoemgt@puexam.edu.np', 'General', 'Primary support and inquiry email'),
    ('ContactPhone', '+977-21-465240, 590937', 'General', 'Office reception phone numbers'),
    ('ExamOfficeEmail', 'puoemgt@puexam.edu.np', 'General', 'Examination branch official email'),
    ('ExamOfficePhone', '+977-21-465240', 'General', 'Direct exam controller telephone'),
    ('WebsiteUrl', 'https://www.puexam.edu.np', 'General', 'Official university exam portal'),
    ('ControllerSignatureName', 'Prof. Dr. Suryanath Adhikari', 'Examination', 'Name on official admit cards and transcripts'),
    ('ControllerDesignation', 'Controller of Examinations', 'Examination', 'Official controller title');
END
GO

-- Seed Notice Categories (Matching puexam.edu.np Tabs)
IF NOT EXISTS (SELECT 1 FROM dbo.NoticeCategories WHERE CategoryName = 'ALL')
BEGIN
    INSERT INTO dbo.NoticeCategories (CategoryName, BadgeClass) VALUES
    ('ALL', 'badge bg-dark'),
    ('REGISTRATION', 'badge bg-primary'),
    ('EXAM FORM', 'badge bg-warning text-dark'),
    ('EXAM ROUTINE', 'badge bg-info text-dark'),
    ('EXAM CENTER', 'badge bg-secondary'),
    ('INTERNAL', 'badge bg-danger'),
    ('RESULT', 'badge bg-success'),
    ('RETOTALING', 'badge bg-dark');
END
GO

-- Seed Faculties (Official 7 Faculties of Purbanchal University)
IF NOT EXISTS (SELECT 1 FROM dbo.Faculties WHERE FacultyName = 'SCIENCE AND TECHNOLOGY')
BEGIN
    INSERT INTO dbo.Faculties (FacultyCode, FacultyName, DeanName, Description) VALUES
    ('FST', 'SCIENCE AND TECHNOLOGY', 'Prof. Dr. Pawan Kumar Jha', 'Computer Application, IT, Biotechnology, Agriculture, Dairy and Food Technology'),
    ('FOM', 'MANAGEMENT', 'Prof. Dr. Sanjeev Kumar Mishra', 'Business Administration, BHM, BHCM, MBA, EMBA and Tourism Studies'),
    ('FOA', 'ARTS', 'Prof. Dr. Ram Prasad Yadav', 'Media Technology, Social Work, Interior Design, Mass Communication, Journalism and English'),
    ('FOE', 'EDUCATION', 'Prof. Dr. Bimal Koirala', 'One Year B.Ed, Distance Education, B.Ed and M.Ed Programs'),
    ('FOL', 'LAW', 'Prof. Dr. Khem Raj Regmi', 'LL.B., B.A. LL.B., LL.M., Human Rights and Conflict Law'),
    ('FMAS', 'MEDICAL AND ALLIED SCIENCES', 'Prof. Dr. Shailesh Mani Pokharel', 'Public Health, B.Sc. MLT, MBBS, Pharmacy, Nursing and Health Sciences'),
    ('FOENG', 'ENGINEERING', 'Prof. Dr. Subash Pokharel', 'Computer, Civil, Electrical, Electronics, Biomedical, Architecture and Information System Engineering');
END
GO

-- Seed Colleges (Purbanchal University Constituent & Affiliated Colleges)
IF NOT EXISTS (SELECT 1 FROM dbo.Colleges WHERE CollegeCode = 'PUSET')
BEGIN
    INSERT INTO dbo.Colleges (CollegeCode, CollegeName, Address, ContactNumber, Email, PrincipalName, AffiliationDate, Website) VALUES
    ('PUSET', 'PU School of Engineering & Technology (PUSET)', 'Puspalal Chowk, Biratnagar-5', '+977-21-464964', 'puset@puexam.edu.np', 'Prof. Er. Pradeep Shrestha', '2000-01-01', 'https://puset.edu.np'),
    ('PUSOM', 'PU School of Management (PUSOM)', 'Biratnagar-12, Morang', '+977-21-464965', 'pusom@puexam.edu.np', 'Prof. Dr. Manoj Shah', '2000-01-01', 'https://pusom.edu.np'),
    ('KCC', 'Kantipur City College', 'Putalisadak, Kathmandu', '+977-1-4430239', 'info@kcc.edu.np', 'Er. Rabin Shrestha', '2000-05-10', 'https://kcc.edu.np'),
    ('WHITEHOUSE', 'Himalayan WhiteHouse International College', 'Khumaltar, Lalitpur', '+977-1-5553364', 'info@whitehouse.edu.np', 'Dr. Yubraj Sharma', '2001-03-15', 'https://whitehouse.edu.np'),
    ('BMC', 'Birat Medical College & Teaching Hospital', 'Tankisinuwari, Biratnagar', '+977-21-421182', 'info@biratmedical.edu.np', 'Dr. Hemant Mishra', '2014-06-01', 'https://biratmedical.edu.np'),
    ('ACME', 'Acme Engineering College', 'Sitapaila, Kathmandu', '+977-1-4280445', 'acme@acme.edu.np', 'Er. P. K. Singh', '2002-08-20', 'https://acme.edu.np'),
    ('JMC', 'Janata Multiple Campus', 'Itahari-8, Sunsari', '+977-25-580123', 'janatamc@gmail.com', 'Mr. Gita Ram Dahal', '2003-04-12', 'https://janatamc.edu.np');
END
GO

-- Seed Programs
IF NOT EXISTS (SELECT 1 FROM dbo.Programs WHERE ProgramCode = 'BCA')
BEGIN
    DECLARE @FstId INT = (SELECT FacultyId FROM dbo.Faculties WHERE FacultyName = 'SCIENCE AND TECHNOLOGY');
    DECLARE @FmsId INT = (SELECT FacultyId FROM dbo.Faculties WHERE FacultyName = 'MANAGEMENT');
    DECLARE @FartId INT = (SELECT FacultyId FROM dbo.Faculties WHERE FacultyName = 'ARTS');
    DECLARE @FmedId INT = (SELECT FacultyId FROM dbo.Faculties WHERE FacultyName = 'MEDICAL AND ALLIED SCIENCES');
    DECLARE @FengId INT = (SELECT FacultyId FROM dbo.Faculties WHERE FacultyName = 'ENGINEERING');
    DECLARE @FlawId INT = (SELECT FacultyId FROM dbo.Faculties WHERE FacultyName = 'LAW');

    INSERT INTO dbo.Programs (ProgramCode, ProgramName, FacultyId, DegreeLevel, DurationYears, TotalSemesters, TotalCredits, Description) VALUES
    ('BCA', 'BACHELOR OF COMPUTER APPLICATION (BCA)', @FstId, 'Bachelor', 4.0, 8, 128, 'Software development, database management and web systems'),
    ('BIT', 'BACHELOR OF INFORMATION TECHNOLOGY (B.I.T.)', @FstId, 'Bachelor', 4.0, 8, 130, 'Enterprise network architecture, cloud computing, cybersecurity'),
    ('BE-CMP', 'BACHELOR IN COMPUTER ENGINEERING', @FengId, 'Bachelor', 4.0, 8, 140, 'Hardware architecture, microprocessor systems, embedded design'),
    ('BE-CIVIL', 'BACHELOR IN CIVIL ENGINEERING', @FengId, 'Bachelor', 4.0, 8, 142, 'Structural design, construction technology, transportation engineering'),
    ('BBA', 'BACHELOR OF BUSINESS ADMINISTRATION', @FmsId, 'Bachelor', 4.0, 8, 126, 'Corporate finance, marketing, organizational leadership, entrepreneurship'),
    ('BHM', 'BACHELOR OF HOTEL MANAGEMENT (BHM)', @FmsId, 'Bachelor', 4.0, 8, 132, 'Hospitality operations, culinary arts, tourism management'),
    ('BPH', 'BACHELOR OF PUBLIC HEALTH (BPH)', @FmedId, 'Bachelor', 4.0, 8, 140, 'Epidemiology, biostatistics, community medicine and public healthcare'),
    ('B-PHARM', 'BACHELOR IN PHARMACY', @FmedId, 'Bachelor', 4.0, 8, 144, 'Pharmaceutical formulations, pharmacology, clinical toxicology'),
    ('BA-LLB', 'B.A. LL.B.', @FlawId, 'Bachelor', 5.0, 10, 160, 'Integrated 5-year constitutional, criminal and international jurisprudence'),
    ('BMT', 'BACHELOR IN MEDIA TECHNOLOGY (BMT)', @FartId, 'Bachelor', 4.0, 8, 128, 'Broadcast production, digital journalism, multimedia technologies'),
    ('MBA', 'MASTER OF BUSINESS ADMINISTRATION (M.B.A.)', @FmsId, 'Master', 2.0, 4, 60, 'Advanced executive management, strategic consulting, data analysis'),
    ('MCA', 'MASTER OF COMPUTER APPLICATION (M.C.A.)', @FstId, 'Master', 3.0, 6, 90, 'Advanced computing, algorithms, distributed systems architecture');
END
GO
END
GO

-- Seed Academic Years
IF NOT EXISTS (SELECT 1 FROM dbo.AcademicYears WHERE YearName = '2025-2026')
BEGIN
    INSERT INTO dbo.AcademicYears (YearName, StartDate, EndDate, IsCurrent) VALUES
    ('2024-2025', '2024-08-01', '2025-07-31', 0),
    ('2025-2026', '2025-08-01', '2026-07-31', 1),
    ('2026-2027', '2026-08-01', '2027-07-31', 0);
END
GO

-- Seed Semesters
IF NOT EXISTS (SELECT 1 FROM dbo.Semesters WHERE SemesterNumber = 1)
BEGIN
    INSERT INTO dbo.Semesters (SemesterName, SemesterNumber, Description) VALUES
    ('1st Semester', 1, 'First Semester (Fall/Spring)'),
    ('2nd Semester', 2, 'Second Semester'),
    ('3rd Semester', 3, 'Third Semester'),
    ('4th Semester', 4, 'Fourth Semester'),
    ('5th Semester', 5, 'Fifth Semester'),
    ('6th Semester', 6, 'Sixth Semester'),
    ('7th Semester', 7, 'Seventh Semester'),
    ('8th Semester', 8, 'Eighth Semester (Graduating)');
END
GO

-- Seed GradeScales
IF NOT EXISTS (SELECT 1 FROM dbo.GradeScales WHERE Grade = 'A')
BEGIN
    INSERT INTO dbo.GradeScales (ProgramLevel, MinPercentage, MaxPercentage, Grade, GradePoint, Description) VALUES
    ('All', 90.00, 100.00, 'A',  4.00, 'Outstanding'),
    ('All', 85.00, 89.99,  'A-', 3.70, 'Excellent'),
    ('All', 80.00, 84.99,  'B+', 3.30, 'Very Good'),
    ('All', 75.00, 79.99,  'B',  3.00, 'Good'),
    ('All', 70.00, 74.99,  'B-', 2.70, 'Above Average'),
    ('All', 65.00, 69.99,  'C+', 2.30, 'Average'),
    ('All', 60.00, 64.99,  'C',  2.00, 'Below Average'),
    ('All', 55.00, 59.99,  'C-', 1.70, 'Marginal'),
    ('All', 50.00, 54.99,  'D+', 1.30, 'Poor'),
    ('All', 45.00, 49.99,  'D',  1.00, 'Pass'),
    ('All', 0.00,  44.99,  'F',  0.00, 'Fail');
END
GO

-- Seed Subjects for BE Computer 1st, 2nd, 3rd Semesters and BBA
IF NOT EXISTS (SELECT 1 FROM dbo.Subjects WHERE SubjectCode = 'MTH111')
BEGIN
    DECLARE @BeCompId INT = (SELECT ProgramId FROM dbo.Programs WHERE ProgramCode = 'BE-CMP');
    DECLARE @BbaId INT = (SELECT ProgramId FROM dbo.Programs WHERE ProgramCode = 'BBA');
    DECLARE @Sem1Id INT = (SELECT SemesterId FROM dbo.Semesters WHERE SemesterNumber = 1);
    DECLARE @Sem2Id INT = (SELECT SemesterId FROM dbo.Semesters WHERE SemesterNumber = 2);
    DECLARE @Sem3Id INT = (SELECT SemesterId FROM dbo.Semesters WHERE SemesterNumber = 3);

    -- BE Computer Sem 1
    INSERT INTO dbo.Subjects (SubjectCode, SubjectName, ProgramId, SemesterId, CreditHours, FullMarks, PassMarks, InternalMarks, ExternalMarks, PracticalMarks, HasPractical) VALUES
    ('MTH111', 'Engineering Mathematics I', @BeCompId, @Sem1Id, 3, 100, 45, 40, 60, 0, 0),
    ('PHY112', 'Physics for Engineers', @BeCompId, @Sem1Id, 4, 100, 45, 30, 50, 20, 1),
    ('CMP113', 'Computer Programming (C)', @BeCompId, @Sem1Id, 4, 100, 45, 30, 50, 20, 1),
    ('ELE114', 'Basic Electrical Engineering', @BeCompId, @Sem1Id, 3, 100, 45, 40, 60, 0, 0),
    ('ENG115', 'Communication Techniques', @BeCompId, @Sem1Id, 2, 50,  23, 20, 30, 0, 0);

    -- BE Computer Sem 2
    INSERT INTO dbo.Subjects (SubjectCode, SubjectName, ProgramId, SemesterId, CreditHours, FullMarks, PassMarks, InternalMarks, ExternalMarks, PracticalMarks, HasPractical) VALUES
    ('MTH121', 'Engineering Mathematics II', @BeCompId, @Sem2Id, 3, 100, 45, 40, 60, 0, 0),
    ('CMP122', 'Object Oriented Programming in C++', @BeCompId, @Sem2Id, 4, 100, 45, 30, 50, 20, 1),
    ('ELX123', 'Basic Electronics Engineering', @BeCompId, @Sem2Id, 3, 100, 45, 40, 60, 0, 0),
    ('CMP124', 'Digital Logic Systems', @BeCompId, @Sem2Id, 3, 100, 45, 40, 60, 0, 0),
    ('CHM125', 'Applied Chemistry', @BeCompId, @Sem2Id, 3, 100, 45, 30, 50, 20, 1);

    -- BE Computer Sem 3
    INSERT INTO dbo.Subjects (SubjectCode, SubjectName, ProgramId, SemesterId, CreditHours, FullMarks, PassMarks, InternalMarks, ExternalMarks, PracticalMarks, HasPractical) VALUES
    ('MTH211', 'Engineering Mathematics III', @BeCompId, @Sem3Id, 3, 100, 45, 40, 60, 0, 0),
    ('CMP212', 'Data Structures and Algorithms', @BeCompId, @Sem3Id, 4, 100, 45, 30, 50, 20, 1),
    ('CMP213', 'Microprocessors & Interfacing', @BeCompId, @Sem3Id, 4, 100, 45, 30, 50, 20, 1),
    ('ELX214', 'Electronic Devices & Circuits', @BeCompId, @Sem3Id, 3, 100, 45, 40, 60, 0, 0),
    ('CMP215', 'Discrete Mathematical Structures', @BeCompId, @Sem3Id, 3, 100, 45, 40, 60, 0, 0);

    -- BBA Sem 1
    INSERT INTO dbo.Subjects (SubjectCode, SubjectName, ProgramId, SemesterId, CreditHours, FullMarks, PassMarks, InternalMarks, ExternalMarks, PracticalMarks, HasPractical) VALUES
    ('MGT111', 'Principles of Management', @BbaId, @Sem1Id, 3, 100, 45, 40, 60, 0, 0),
    ('ECO112', 'Microeconomics for Business', @BbaId, @Sem1Id, 3, 100, 45, 40, 60, 0, 0),
    ('ACC113', 'Financial Accounting I', @BbaId, @Sem1Id, 3, 100, 45, 40, 60, 0, 0),
    ('ENG114', 'English Composition & Business Writing', @BbaId, @Sem1Id, 3, 100, 45, 40, 60, 0, 0),
    ('MTH115', 'Business Mathematics I', @BbaId, @Sem1Id, 3, 100, 45, 40, 60, 0, 0);
END
GO

-- Seed Examination Centers
IF NOT EXISTS (SELECT 1 FROM dbo.ExaminationCenters WHERE CenterCode = 'CTR-01')
BEGIN
    INSERT INTO dbo.ExaminationCenters (CenterCode, CenterName, Location, Capacity, SuperintendentName, ContactNumber) VALUES
    ('CTR-01', 'PU Central Examination Complex (Hall A & B)', 'Puspalal Chowk, Biratnagar', 800, 'Prof. Dr. Suryanath Adhikari', '+977-21-465240'),
    ('CTR-02', 'Kantipur City College Examination Center', 'Putalisadak, Kathmandu', 500, 'Er. Rabin Shrestha', '+977-1-4430239'),
    ('CTR-03', 'Janata Multiple Campus Examination Center', 'Itahari, Sunsari', 450, 'Mr. Gita Ram Dahal', '+977-25-580123'),
    ('CTR-04', 'WhiteHouse International Center', 'Khumaltar, Lalitpur', 400, 'Dr. Yubraj Sharma', '+977-1-5553364');
END
GO

-- Seed Examinations
IF NOT EXISTS (SELECT 1 FROM dbo.Examinations WHERE ExamCode = 'EXAM-SPRING-2026')
BEGIN
    DECLARE @AcadYearCurrentId INT = (SELECT AcademicYearId FROM dbo.AcademicYears WHERE YearName = '2025-2026');
    DECLARE @Sem1Id INT = (SELECT SemesterId FROM dbo.Semesters WHERE SemesterNumber = 1);
    DECLARE @Sem2Id INT = (SELECT SemesterId FROM dbo.Semesters WHERE SemesterNumber = 2);
    DECLARE @Ctr1Id INT = (SELECT CenterId FROM dbo.ExaminationCenters WHERE CenterCode = 'CTR-01');

    INSERT INTO dbo.Examinations (
        ExamCode, ExamName, AcademicYearId, SemesterId, ExamType,
        StartDate, EndDate, ApplicationDeadline, LateFeeDeadline, FeeAmount, LateFeeAmount, DefaultCenterId, Status
    )
    VALUES
    ('EXAM-SPRING-2026', 'Spring 2026 Semester End Examination', @AcadYearCurrentId, @Sem1Id, 'Regular',
     '2026-04-15', '2026-05-05', '2026-03-25', '2026-03-31', 1500.00, 500.00, @Ctr1Id, 'Ongoing'),
    ('EXAM-FALL-2025', 'Fall 2025 Semester End Examination', @AcadYearCurrentId, @Sem2Id, 'Regular',
     '2025-10-10', '2025-11-02', '2025-09-20', '2025-09-28', 1500.00, 500.00, @Ctr1Id, 'ResultPublished');
END
GO

-- Seed System Users
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = 'admin@puexam.edu.np')
BEGIN
    DECLARE @AdminRoleId INT = (SELECT RoleId FROM dbo.Roles WHERE RoleName = 'Admin');
    DECLARE @SuperAdminRoleId INT = (SELECT RoleId FROM dbo.Roles WHERE RoleName = 'Super Admin');
    DECLARE @ControllerRoleId INT = (SELECT RoleId FROM dbo.Roles WHERE RoleName = 'Examination Controller');
    DECLARE @CollegeRoleId INT = (SELECT RoleId FROM dbo.Roles WHERE RoleName = 'College Admin');
    DECLARE @ExaminerRoleId INT = (SELECT RoleId FROM dbo.Roles WHERE RoleName = 'Examiner');
    DECLARE @StudentRoleId INT = (SELECT RoleId FROM dbo.Roles WHERE RoleName = 'Student');
    DECLARE @ColPusetId INT = (SELECT CollegeId FROM dbo.Colleges WHERE CollegeCode = 'PUSET');

    -- Default BCrypt hash (verified & auto-synced by DbInitializer)
    DECLARE @DefaultHash NVARCHAR(255) = '$2a$11$mI6GzF68x.871WpB0U1i9O3L6/N9.zS7J/pG8c1bE1a4Q2w5e6r7t';

    -- 1. Admin User
    INSERT INTO dbo.Users (Username, Email, PasswordHash, FullName, PhoneNumber, UserType, ReferenceId, IsActive)
    VALUES ('admin', 'admin@puexam.edu.np', @DefaultHash, 'System Administrator (PU OCoE)', '+977-21-465240', 'SuperAdmin', NULL, 1);
    DECLARE @UidAdmin INT = SCOPE_IDENTITY();
    INSERT INTO dbo.UserRoles (UserId, RoleId) VALUES (@UidAdmin, @SuperAdminRoleId);
    INSERT INTO dbo.UserRoles (UserId, RoleId) VALUES (@UidAdmin, @AdminRoleId);

    -- 2. Examination Controller User
    INSERT INTO dbo.Users (Username, Email, PasswordHash, FullName, PhoneNumber, UserType, ReferenceId, IsActive)
    VALUES ('controller', 'controller@puexam.edu.np', @DefaultHash, 'Prof. Dr. Suryanath Adhikari', '+977-21-465240', 'Controller', NULL, 1);
    DECLARE @UidCtrl INT = SCOPE_IDENTITY();
    INSERT INTO dbo.UserRoles (UserId, RoleId) VALUES (@UidCtrl, @ControllerRoleId);

    -- 3. College Admin User
    INSERT INTO dbo.Users (Username, Email, PasswordHash, FullName, PhoneNumber, UserType, ReferenceId, IsActive)
    VALUES ('college_puset', 'college@puexam.edu.np', @DefaultHash, 'Prof. Er. Pradeep Shrestha (PUSET Admin)', '+977-21-464964', 'CollegeAdmin', @ColPusetId, 1);
    DECLARE @UidCol INT = SCOPE_IDENTITY();
    INSERT INTO dbo.UserRoles (UserId, RoleId) VALUES (@UidCol, @CollegeRoleId);

    -- 4. Examiner User
    INSERT INTO dbo.Users (Username, Email, PasswordHash, FullName, PhoneNumber, UserType, ReferenceId, IsActive)
    VALUES ('examiner1', 'examiner@puexam.edu.np', @DefaultHash, 'Dr. Suresh Acharya', '+977-9841001122', 'Examiner', NULL, 1);
    DECLARE @UidEx INT = SCOPE_IDENTITY();
    INSERT INTO dbo.UserRoles (UserId, RoleId) VALUES (@UidEx, @ExaminerRoleId);
END
GO

-- Seed Sample Students
IF NOT EXISTS (SELECT 1 FROM dbo.Students WHERE StudentUniqueId = 'PU2024-00101')
BEGIN
    DECLARE @FengId INT = (SELECT FacultyId FROM dbo.Faculties WHERE FacultyName = 'ENGINEERING');
    DECLARE @FmsId INT = (SELECT FacultyId FROM dbo.Faculties WHERE FacultyName = 'MANAGEMENT');
    DECLARE @FstId INT = (SELECT FacultyId FROM dbo.Faculties WHERE FacultyName = 'SCIENCE AND TECHNOLOGY');
    DECLARE @ProgCompId INT = (SELECT ProgramId FROM dbo.Programs WHERE ProgramCode = 'BE-CMP');
    DECLARE @ProgBbaId INT = (SELECT ProgramId FROM dbo.Programs WHERE ProgramCode = 'BBA');
    DECLARE @ProgBcaId INT = (SELECT ProgramId FROM dbo.Programs WHERE ProgramCode = 'BCA');
    DECLARE @ColPusetId INT = (SELECT CollegeId FROM dbo.Colleges WHERE CollegeCode = 'PUSET');
    DECLARE @ColPusomId INT = (SELECT CollegeId FROM dbo.Colleges WHERE CollegeCode = 'PUSOM');
    DECLARE @AcadYearCurrentId INT = (SELECT AcademicYearId FROM dbo.AcademicYears WHERE YearName = '2025-2026');
    DECLARE @Sem1Id INT = (SELECT SemesterId FROM dbo.Semesters WHERE SemesterNumber = 1);
    DECLARE @StudentRoleId INT = (SELECT RoleId FROM dbo.Roles WHERE RoleName = 'Student');
    DECLARE @DefaultHashStud NVARCHAR(255) = '$2a$11$mI6GzF68x.871WpB0U1i9O3L6/N9.zS7J/pG8c1bE1a4Q2w5e6r7t';

    -- Student 1: Approved Student (Aarav Sharma)
    INSERT INTO dbo.Users (Username, Email, PasswordHash, FullName, PhoneNumber, UserType, IsActive)
    VALUES ('PU2024-00101', 'student@puexam.edu.np', @DefaultHashStud, 'Aarav Sharma', '+977-9846123456', 'Student', 1);
    DECLARE @UStud1 INT = SCOPE_IDENTITY();
    INSERT INTO dbo.UserRoles (UserId, RoleId) VALUES (@UStud1, @StudentRoleId);

    INSERT INTO dbo.Students (
        StudentUniqueId, RegistrationNumber, RollNumber, UserId, FullName, DateOfBirth, Gender,
        Email, MobileNumber, Address, FacultyId, ProgramId, CollegeId, AcademicYearId, CurrentSemesterId,
        PreviousQualification, PhotoUrl, RegistrationStatus, VerifiedDate, IsActive
    )
    VALUES (
        'PU2024-00101', '024-1-01-00001-2024', '2410101', @UStud1, 'Aarav Sharma', '2004-05-14', 'Male',
        'student@puexam.edu.np', '+977-9846123456', 'Biratnagar-5, Morang, Nepal', @FengId, @ProgCompId,
        @ColPusetId, @AcadYearCurrentId, @Sem1Id, '+2 Science (82.5%)', '/images/college-logo.png',
        'Approved', '2024-09-01', 1
    );
    DECLARE @Stud1 INT = SCOPE_IDENTITY();
    UPDATE dbo.Users SET ReferenceId = @Stud1 WHERE UserId = @UStud1;

    INSERT INTO dbo.StudentRegistrations (StudentId, ApplicationNumber, Status, ProcessedDate, Remarks)
    VALUES (@Stud1, 'REG2024000101', 'Approved', '2024-09-01', 'Verified with original transcripts.');

    -- Student 2: Approved Student (Pooja Gurung)
    INSERT INTO dbo.Users (Username, Email, PasswordHash, FullName, PhoneNumber, UserType, IsActive)
    VALUES ('PU2024-00102', 'pooja.gurung@puexam.edu.np', @DefaultHashStud, 'Pooja Gurung', '+977-9806123456', 'Student', 1);
    DECLARE @UStud2 INT = SCOPE_IDENTITY();
    INSERT INTO dbo.UserRoles (UserId, RoleId) VALUES (@UStud2, @StudentRoleId);

    INSERT INTO dbo.Students (
        StudentUniqueId, RegistrationNumber, RollNumber, UserId, FullName, DateOfBirth, Gender,
        Email, MobileNumber, Address, FacultyId, ProgramId, CollegeId, AcademicYearId, CurrentSemesterId,
        PreviousQualification, PhotoUrl, RegistrationStatus, VerifiedDate, IsActive
    )
    VALUES (
        'PU2024-00102', '024-1-01-00002-2024', '2410102', @UStud2, 'Pooja Gurung', '2004-09-22', 'Female',
        'pooja.gurung@puexam.edu.np', '+977-9806123456', 'Itahari-4, Sunsari, Nepal', @FengId, @ProgCompId,
        @ColPusetId, @AcadYearCurrentId, @Sem1Id, '+2 Science (88.0%)', '/images/college-logo.png',
        'Approved', '2024-09-01', 1
    );
    DECLARE @Stud2 INT = SCOPE_IDENTITY();
    UPDATE dbo.Users SET ReferenceId = @Stud2 WHERE UserId = @UStud2;

    INSERT INTO dbo.StudentRegistrations (StudentId, ApplicationNumber, Status, ProcessedDate, Remarks)
    VALUES (@Stud2, 'REG2024000102', 'Approved', '2024-09-01', 'Verified and confirmed.');

    -- Student 3: Pending Registration (Rohan Shrestha)
    INSERT INTO dbo.Students (
        StudentUniqueId, RegistrationNumber, RollNumber, UserId, FullName, DateOfBirth, Gender,
        Email, MobileNumber, Address, FacultyId, ProgramId, CollegeId, AcademicYearId, CurrentSemesterId,
        PreviousQualification, PhotoUrl, RegistrationStatus, IsActive
    )
    VALUES (
        'PU2026-00103', NULL, NULL, NULL, 'Rohan Shrestha', '2005-01-18', 'Male',
        'rohan.shrestha@puexam.edu.np', '+977-9815123456', 'Dharan-10, Sunsari', @FmsId, @ProgBbaId,
        @ColPusomId, @AcadYearCurrentId, @Sem1Id, '+2 Management (76.2%)', NULL,
        'Pending', 1
    );
    DECLARE @Stud3 INT = SCOPE_IDENTITY();
    INSERT INTO dbo.StudentRegistrations (StudentId, ApplicationNumber, Status, Remarks)
    VALUES (@Stud3, 'REG2026000103', 'Pending', 'Submitted online. Pending identity document verification.');

    -- Seed Examination Application for Student 1 in Spring 2026
    DECLARE @ExamSpringId INT = (SELECT ExaminationId FROM dbo.Examinations WHERE ExamCode = 'EXAM-SPRING-2026');
    DECLARE @Ctr1Id INT = (SELECT CenterId FROM dbo.ExaminationCenters WHERE CenterCode = 'CTR-01');

    INSERT INTO dbo.ExaminationApplications (
        ApplicationNumber, ExaminationId, StudentId, CollegeId, ProgramId, SemesterId,
        ExamType, AssignedRollNumber, ExaminationCenterId, FeePaid, PaymentReference, Status,
        SubmissionDate, ReviewedDate, CreatedDate
    )
    VALUES (
        'EX2026000001', @ExamSpringId, @Stud1, @ColPusetId, @ProgCompId, @Sem1Id,
        'Regular', '2410101', @Ctr1Id, 1500.00, 'TXN-98461-EBANK', 'Approved',
        '2026-03-10', '2026-03-15', GETDATE()
    );
    DECLARE @App1 INT = SCOPE_IDENTITY();

    -- Add Application Subjects
    INSERT INTO dbo.ExaminationApplicationSubjects (ApplicationId, SubjectId, SubjectType, ExamDate, ExamTime)
    SELECT @App1, SubjectId, 'Regular', '2026-04-15', '07:00 AM - 10:00 AM'
    FROM dbo.Subjects WHERE ProgramId = @ProgCompId AND SemesterId = @Sem1Id;

    -- Generate Admit Card for Student 1
    INSERT INTO dbo.AdmitCards (
        AdmitCardNumber, ApplicationId, StudentId, ExaminationId, RollNumber,
        RegistrationNumber, ExaminationCenterId, QrCodeHash, GeneratedDate, IsActive
    )
    VALUES (
        'AC2026000001', @App1, @Stud1, @ExamSpringId, '2410101',
        '024-1-01-00001-2024', @Ctr1Id, 'VERIFY-AC:AC2026000001:2410101:024-1-01-00001-2024', GETDATE(), 1
    );

    -- Seed Fall 2025 Marks and Published Result for Student 1
    DECLARE @ExamFallId INT = (SELECT ExaminationId FROM dbo.Examinations WHERE ExamCode = 'EXAM-FALL-2025');

    -- Insert Marks for Student 1
    INSERT INTO dbo.Marks (ExaminationId, StudentId, SubjectId, InternalMarks, ExternalMarks, PracticalMarks, TotalMarks, Grade, GradePoint, Status, IsAbsent)
    SELECT @ExamFallId, @Stud1, SubjectId, 
           CASE WHEN SubjectCode = 'MTH111' THEN 36.00
                WHEN SubjectCode = 'PHY112' THEN 26.00
                WHEN SubjectCode = 'CMP113' THEN 28.00
                WHEN SubjectCode = 'ELE114' THEN 35.00
                ELSE 18.00 END AS InternalMarks,
           CASE WHEN SubjectCode = 'MTH111' THEN 52.00
                WHEN SubjectCode = 'PHY112' THEN 44.00
                WHEN SubjectCode = 'CMP113' THEN 46.00
                WHEN SubjectCode = 'ELE114' THEN 50.00
                ELSE 26.00 END AS ExternalMarks,
           CASE WHEN HasPractical = 1 THEN 18.00 ELSE 0.00 END AS PracticalMarks,
           CASE WHEN SubjectCode = 'MTH111' THEN 88.00
                WHEN SubjectCode = 'PHY112' THEN 88.00
                WHEN SubjectCode = 'CMP113' THEN 92.00
                WHEN SubjectCode = 'ELE114' THEN 85.00
                ELSE 44.00 END AS TotalMarks,
           CASE WHEN SubjectCode = 'MTH111' THEN 'A-'
                WHEN SubjectCode = 'PHY112' THEN 'A-'
                WHEN SubjectCode = 'CMP113' THEN 'A'
                WHEN SubjectCode = 'ELE114' THEN 'A-'
                ELSE 'A-' END AS Grade,
           CASE WHEN SubjectCode = 'CMP113' THEN 4.00 ELSE 3.70 END AS GradePoint,
           'Locked', 0
    FROM dbo.Subjects WHERE ProgramId = @ProgCompId AND SemesterId = @Sem1Id;

    -- Process Results for Fall 2025
    EXEC dbo.sp_ProcessExaminationResults @ExamFallId;
    EXEC dbo.sp_PublishExaminationResults @ExamFallId, 1, 1;
END
GO

-- Seed Notices (Matching Live puexam.edu.np Notices)
IF NOT EXISTS (SELECT 1 FROM dbo.Notices WHERE Title LIKE '%EMBA - I, II, III, IV Sem%')
BEGIN
    DECLARE @CatAll INT = (SELECT CategoryId FROM dbo.NoticeCategories WHERE CategoryName = 'ALL');
    DECLARE @CatRoutine INT = (SELECT CategoryId FROM dbo.NoticeCategories WHERE CategoryName = 'EXAM ROUTINE');
    DECLARE @CatResult INT = (SELECT CategoryId FROM dbo.NoticeCategories WHERE CategoryName = 'RESULT');
    DECLARE @CatExamForm INT = (SELECT CategoryId FROM dbo.NoticeCategories WHERE CategoryName = 'EXAM FORM');
    DECLARE @CatReg INT = (SELECT CategoryId FROM dbo.NoticeCategories WHERE CategoryName = 'REGISTRATION');
    DECLARE @CatCenter INT = (SELECT CategoryId FROM dbo.NoticeCategories WHERE CategoryName = 'EXAM CENTER');
    DECLARE @CatInternal INT = (SELECT CategoryId FROM dbo.NoticeCategories WHERE CategoryName = 'INTERNAL');
    DECLARE @CatRetotal INT = (SELECT CategoryId FROM dbo.NoticeCategories WHERE CategoryName = 'RETOTALING');

    INSERT INTO dbo.Notices (CategoryId, Title, Slug, Description, PublishDate, ExpiryDate, AttachmentUrl, AttachmentFileName, IsPinned, IsActive, ViewCount) VALUES
    (@CatRoutine, '(2083.08.07): EMBA - I, II, III, IV Sem (Spring / Fall) Examination Routine', 
     'emba-i-ii-iii-iv-sem-spring-fall-routine', 
     'Examination schedule and routine for Executive Master of Business Administration (EMBA) First, Second, Third and Fourth Semester (Spring & Fall Sessions). All affiliated colleges and examinees are notified to adhere to the timetable.',
     '2026-11-20', '2026-12-31', '/files/notices/EMBA_Routine_2083.pdf', 'EMBA_Routine_2083.pdf', 1, 1, 2850),

    (@CatRoutine, '(2083.08.03): LLB - I/ II/ III & BALLB - IV/ V Year Examination Schedule',
     'llb-i-ii-iii-ballb-iv-v-year-schedule',
     'Official routine for Bachelor of Laws (LL.B.) 1st, 2nd, and 3rd Year and B.A. LL.B. 4th and 5th Year Regular and Back Examination.',
     '2026-11-18', '2026-12-31', '/files/notices/LLB_BALLB_Routine.pdf', 'LLB_BALLB_Routine.pdf', 1, 1, 1940),

    (@CatResult, '(2083.07.20): BPH - VIII Sem Result Published',
     'bph-viii-sem-result-published',
     'The Office of the Examination Management, Purbanchal University hereby announces the results of the Bachelor of Public Health (BPH) Eighth Semester Examinations.',
     '2026-11-05', '2027-01-31', '/files/notices/BPH_VIII_Result.pdf', 'BPH_VIII_Result.pdf', 1, 1, 4210),

    (@CatResult, '(2083.07.19): BBA - I Sem Result Published',
     'bba-i-sem-result-published',
     'Results of Bachelor of Business Administration (BBA) First Semester examinations held in 2026 have been officially published. View results on the Find Result portal.',
     '2026-11-04', '2027-01-31', '/files/notices/BBA_I_Result.pdf', 'BBA_I_Result.pdf', 1, 1, 5620),

    (@CatResult, '(2083.07.19): B.E.(CIVIL / COMPUTER) - V SEM Result Published',
     'be-civil-computer-v-sem-result',
     'Bachelor in Civil Engineering and Bachelor in Computer Engineering Fifth Semester examination results published with grade sheets available.',
     '2026-11-04', '2027-01-31', '/files/notices/BE_V_Sem_Result.pdf', 'BE_V_Sem_Result.pdf', 1, 1, 6800),

    (@CatExamForm, 'Notice Regarding Online Examination Form Fill-up for Bachelor Level (BE / BBA / BCA)',
     'notice-online-exam-form-fill-up-bachelor-level',
     'Examinees of BE, BBA, and BCA are notified to submit online examination forms through the Student Portal or respective college login within the deadline.',
     '2026-10-15', '2026-12-15', '/files/notices/Exam_Form_Fillup_Notice.pdf', 'Exam_Form_Fillup_Notice.pdf', 1, 1, 3890),

    (@CatCenter, 'Examination Centers list for Bachelor Level 2026 Examinations',
     'examination-centers-list-bachelor-level-2026',
     'Designated examination centers and seating allocations for constituent and affiliated colleges under Purbanchal University.',
     '2026-10-20', '2026-12-30', '/files/notices/Exam_Centers_2026.pdf', 'Exam_Centers_2026.pdf', 0, 1, 2100),

    (@CatInternal, 'Urgent Notice: Internal & Practical Marks Entry for Colleges (Spring 2026)',
     'urgent-notice-internal-practical-marks-entry',
     'All colleges affiliated with Purbanchal University must submit internal and practical assessment marks via the College Login portal on or before the closing date.',
     '2026-10-10', '2026-11-30', '/files/notices/Internal_Marks_Notice.pdf', 'Internal_Marks_Notice.pdf', 0, 1, 1720),

    (@CatRetotal, 'Notice Regarding Retotaling and Rechecking of Answer Books - 2026',
     'notice-retotaling-rechecking-2026',
     'Interested students may apply for retotaling of answer sheets within 15 days from the date of result publication with prescribed fee of Rs. 500 per paper.',
     '2026-10-25', '2026-11-25', '/files/notices/Retotaling_Notice_2026.pdf', 'Retotaling_Notice_2026.pdf', 0, 1, 1430);
END
GO
