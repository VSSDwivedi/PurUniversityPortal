using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityExamSystem.Models;
using UniversityExamSystem.Repositories;
using UniversityExamSystem.Services;

namespace UniversityExamSystem.Controllers
{
    //[Authorize(Policy = "RequireControllerRole")]
    public class AdminExamController : Controller
    {
        private readonly IExaminationRepository _examRepo;
        private readonly IAcademicRepository _academicRepo;
        private readonly IQrCodeService _qrCodeService;

        public AdminExamController(
            IExaminationRepository examRepo,
            IAcademicRepository academicRepo,
            IQrCodeService qrCodeService)
        {
            _examRepo = examRepo;
            _academicRepo = academicRepo;
            _qrCodeService = qrCodeService;
        }

        public async Task<IActionResult> Index()
        {
            int? collegeId = null;
            var userType = User.FindFirst("UserType")?.Value;
            bool isCollegeAdmin = string.Equals(userType, "CollegeAdmin", StringComparison.OrdinalIgnoreCase);

            if (isCollegeAdmin)
            {
                if (!int.TryParse(User.FindFirst("CollegeId")?.Value, out var myCollegeId))
                    return Forbid();

                collegeId = myCollegeId;
            }

            ViewBag.IsCollegeAdmin = isCollegeAdmin;

            var exams = await _examRepo.GetExaminationsAsync(collegeId);
            return View(exams);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.AcademicYears = await _academicRepo.GetAcademicYearsAsync();
            ViewBag.Semesters = await _academicRepo.GetSemestersAsync();
            ViewBag.Faculties = await _academicRepo.GetFacultiesAsync();
            ViewBag.Programs = await _academicRepo.GetProgramsAsync();
            ViewBag.Centers = await _academicRepo.GetCentersAsync();

            return View(new Examination
            {
                StartDate = DateTime.Today.AddDays(30),
                EndDate = DateTime.Today.AddDays(50),
                ApplicationDeadline = DateTime.Today.AddDays(15),
                LateFeeDeadline = DateTime.Today.AddDays(20),
                FeeAmount = 1500m,
                LateFeeAmount = 500m
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Examination exam)
        {
            if (string.IsNullOrWhiteSpace(exam.ExamName) || string.IsNullOrWhiteSpace(exam.ExamCode))
            {
                ModelState.AddModelError(string.Empty, "Exam Code and Name are required.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.AcademicYears = await _academicRepo.GetAcademicYearsAsync();
                ViewBag.Semesters = await _academicRepo.GetSemestersAsync();
                ViewBag.Faculties = await _academicRepo.GetFacultiesAsync();
                ViewBag.Programs = await _academicRepo.GetProgramsAsync();
                ViewBag.Centers = await _academicRepo.GetCentersAsync();
                return View(exam);
            }

            var id = await _examRepo.AddExaminationAsync(exam);
            TempData["SuccessMessage"] = $"Examination '{exam.ExamName}' created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Applications(int? examId, int? collegeId, string? status)
        {
            var userType = User.FindFirst("UserType")?.Value;
            bool isCollegeAdmin = string.Equals(userType, "CollegeAdmin", StringComparison.OrdinalIgnoreCase);

            if (isCollegeAdmin)
            {
                if (!int.TryParse(User.FindFirst("CollegeId")?.Value, out var myCollegeId))
                    return Forbid();

                collegeId = myCollegeId;
            }

            ViewBag.IsCollegeAdmin = isCollegeAdmin;

            ViewBag.Examinations = await _examRepo.GetExaminationsAsync();
            ViewBag.Colleges = await _academicRepo.GetCollegesAsync();
            ViewBag.Centers = await _academicRepo.GetCentersAsync();
            ViewBag.SelectedExam = examId;
            ViewBag.SelectedCollege = collegeId;
            ViewBag.SelectedStatus = status;

            var apps = await _examRepo.GetApplicationsAsync(examId, collegeId, null, status);
            return View(apps);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReviewApplication(int applicationId, string status, string remarks)
        {
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int reviewedBy);
            await _examRepo.UpdateApplicationStatusAsync(applicationId, status, remarks, reviewedBy);
            TempData["SuccessMessage"] = $"Application status updated to '{status}'.";
            return RedirectToAction(nameof(Applications));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateAdmitCard(int applicationId, string rollNumber, int centerId)
        {
            if (string.IsNullOrWhiteSpace(rollNumber))
            {
                TempData["ErrorMessage"] = "Roll Number is required to generate Admit Card.";
                return RedirectToAction(nameof(Applications));
            }

            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int generatedBy);
            var (cardId, cardNumber) = await _examRepo.GenerateAdmitCardAsync(applicationId, rollNumber.Trim(), centerId, generatedBy);
            TempData["SuccessMessage"] = $"Admit Card #{cardNumber} generated successfully for Roll Number: {rollNumber}!";
            return RedirectToAction(nameof(Applications));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkGenerateAdmitCards(int examId, int? collegeId, int? centerId)
        {
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int generatedBy);
            var count = await _examRepo.BulkGenerateAdmitCardsAsync(examId, collegeId, centerId, generatedBy);
            TempData["SuccessMessage"] = $"Successfully bulk-generated {count} admit cards!";
            return RedirectToAction(nameof(Applications), new { examId, collegeId });
        }

        // ======================== CENTERS ========================
        public async Task<IActionResult> Centers()
        {
            var centers = await _academicRepo.GetCentersAsync();
            return View(centers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCenter(ExaminationCenter center)
        {
            if (string.IsNullOrWhiteSpace(center.CenterName) || string.IsNullOrWhiteSpace(center.CenterCode))
            {
                TempData["ErrorMessage"] = "Center Code and Name are required.";
                return RedirectToAction(nameof(Centers));
            }

            await _academicRepo.AddCenterAsync(center);
            TempData["SuccessMessage"] = $"Examination Center '{center.CenterName}' added successfully.";
            return RedirectToAction(nameof(Centers));
        }
    }
}
