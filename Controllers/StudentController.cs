using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityExamSystem.Models;
using UniversityExamSystem.Repositories;
using UniversityExamSystem.Services;

namespace UniversityExamSystem.Controllers
{
    //[Authorize(Policy = "RequireStudentRole")]
    public class StudentController : Controller
    {
        private readonly IStudentRepository _studentRepo;
        private readonly IExaminationRepository _examRepo;
        private readonly IResultRepository _resultRepo;
        private readonly IAcademicRepository _academicRepo;
        private readonly INoticeRepository _noticeRepo;
        private readonly IQrCodeService _qrCodeService;
        private readonly IFileService _fileService;
        private readonly ISystemSettingsRepository _settingsRepo;

        public StudentController(
            IStudentRepository studentRepo,
            IExaminationRepository examRepo,
            IResultRepository resultRepo,
            IAcademicRepository academicRepo,
            INoticeRepository noticeRepo,
            IQrCodeService qrCodeService,
            IFileService fileService,
            ISystemSettingsRepository settingsRepo)
        {
            _studentRepo = studentRepo;
            _examRepo = examRepo;
            _resultRepo = resultRepo;
            _academicRepo = academicRepo;
            _noticeRepo = noticeRepo;
            _qrCodeService = qrCodeService;
            _fileService = fileService;
            _settingsRepo = settingsRepo;
        }

        private async Task<Student?> GetCurrentStudentAsync()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int userId))
            {
                var student = await _studentRepo.GetByUserIdAsync(userId);
                if (student != null) return student;
            }

            // Fallback by username or email
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
            {
                var student = await _studentRepo.TrackRegistrationAsync(email);
                if (student != null) return student;
            }

            return null;
        }

        public async Task<IActionResult> Index()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Student = student;
            ViewBag.ActiveExams = await _examRepo.GetActiveExaminationsAsync();
            ViewBag.RecentNotices = await _noticeRepo.GetPublicNoticesAsync(limit: 5);
            ViewBag.Applications = await _examRepo.GetApplicationsAsync(studentId: student.StudentId);
            ViewBag.Results = await _resultRepo.GetStudentResultsHistoryAsync(student.StudentId);

            return View(student);
        }

        public async Task<IActionResult> Profile()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return RedirectToAction("Login", "Account");

            ViewBag.Documents = await _studentRepo.GetStudentDocumentsAsync(student.StudentId);
            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string mobileNumber, string address)
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return RedirectToAction("Login", "Account");

            student.MobileNumber = mobileNumber;
            student.Address = address;

            await _studentRepo.UpdateProfileAsync(student);
            TempData["SuccessMessage"] = "Profile details updated successfully.";

            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        public async Task<IActionResult> ApplyExam(int? examId)
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return RedirectToAction("Login", "Account");

            if (student.RegistrationStatus != "Approved")
            {
                TempData["ErrorMessage"] = "Your university registration is currently pending verification. You can apply for examinations only after your registration is approved.";
                return RedirectToAction(nameof(Index));
            }

            var activeExams = await _examRepo.GetActiveExaminationsAsync();
            ViewBag.ActiveExams = activeExams;

            var selectedExamId = examId ?? activeExams.FirstOrDefault()?.ExaminationId ?? 0;
            if (selectedExamId == 0)
            {
                TempData["InfoMessage"] = "There are no open examinations available for application at this moment.";
                return RedirectToAction(nameof(Index));
            }

            // Check if already applied
            var existingApp = await _examRepo.GetApplicationByStudentAndExamAsync(student.StudentId, selectedExamId);
            if (existingApp != null)
            {
                TempData["InfoMessage"] = $"You have already applied for this examination (Application #{existingApp.ApplicationNumber}). Current status: {existingApp.Status}.";
                return RedirectToAction(nameof(MyApplications));
            }

            var exam = await _examRepo.GetExaminationByIdAsync(selectedExamId);
            var subjects = await _academicRepo.GetSubjectsAsync(student.ProgramId, student.CurrentSemesterId);

            var model = new ExamApplicationViewModel
            {
                ExaminationId = selectedExamId,
                StudentId = student.StudentId,
                FeeAmount = exam?.FeeAmount ?? 1500m,
                Examination = exam,
                Student = student,
                AvailableSubjects = subjects,
                SelectedSubjectIds = subjects.Select(s => s.SubjectId).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyExam(ExamApplicationViewModel model)
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return RedirectToAction("Login", "Account");

            if (model.SelectedSubjectIds == null || !model.SelectedSubjectIds.Any())
            {
                ModelState.AddModelError("SelectedSubjectIds", "You must select at least one subject to sit for examination.");
            }

            if (!ModelState.IsValid)
            {
                var exam = await _examRepo.GetExaminationByIdAsync(model.ExaminationId);
                var subjects = await _academicRepo.GetSubjectsAsync(student.ProgramId, student.CurrentSemesterId);
                model.Examination = exam;
                model.Student = student;
                model.AvailableSubjects = subjects;
                ViewBag.ActiveExams = await _examRepo.GetActiveExaminationsAsync();
                return View(model);
            }

            try
            {
                string? voucherUrl = null;
                if (model.VoucherFile != null)
                {
                    voucherUrl = await _fileService.SaveFileAsync(model.VoucherFile, "vouchers");
                }

                var (appId, appNumber) = await _examRepo.SubmitApplicationAsync(
                    model.ExaminationId,
                    student.StudentId,
                    model.ExamType,
                    model.FeeAmount,
                    model.PaymentReference,
                    voucherUrl,
                    model.SelectedSubjectIds ?? new List<int>()
                );

                TempData["SuccessMessage"] = $"Examination Application #{appNumber} submitted successfully! Your application has been sent for college and university verification.";
                return RedirectToAction(nameof(MyApplications));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var exam = await _examRepo.GetExaminationByIdAsync(model.ExaminationId);
                var subjects = await _academicRepo.GetSubjectsAsync(student.ProgramId, student.CurrentSemesterId);
                model.Examination = exam;
                model.Student = student;
                model.AvailableSubjects = subjects;
                ViewBag.ActiveExams = await _examRepo.GetActiveExaminationsAsync();
                return View(model);
            }
        }

        public async Task<IActionResult> MyApplications()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return RedirectToAction("Login", "Account");

            var applications = await _examRepo.GetApplicationsAsync(studentId: student.StudentId);
            return View(applications);
        }

        public async Task<IActionResult> AdmitCard(int? examId)
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return RedirectToAction("Login", "Account");

            AdmitCard? card = null;

            if (examId.HasValue)
            {
                card = await _examRepo.GetAdmitCardByStudentAndExamAsync(student.StudentId, examId.Value);
            }
            else
            {
                // Find most recent active exam admit card
                var exams = await _examRepo.GetExaminationsAsync();
                foreach (var exam in exams)
                {
                    card = await _examRepo.GetAdmitCardByStudentAndExamAsync(student.StudentId, exam.ExaminationId);
                    if (card != null) break;
                }
            }

            if (card == null)
            {
                TempData["InfoMessage"] = "No approved admit card is available for download at this time. Admit cards become available once your examination application is approved.";
                return RedirectToAction(nameof(MyApplications));
            }

            if (!string.IsNullOrEmpty(card.QrCodeHash))
            {
                card.QrCodeBase64 = _qrCodeService.GenerateQrCodeBase64(card.QrCodeHash);
            }

            await _examRepo.RecordAdmitCardDownloadAsync(card.AdmitCardId);
            ViewBag.Settings = await _settingsRepo.GetAllSettingsAsync();

            return View(card);
        }

        public async Task<IActionResult> Results()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return RedirectToAction("Login", "Account");

            var results = await _resultRepo.GetStudentResultsHistoryAsync(student.StudentId);
            ViewBag.GradeScales = await _academicRepo.GetGradeScalesAsync();

            return View(results);
        }
    }
}
