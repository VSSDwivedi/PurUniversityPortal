using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityExamSystem.Models;
using UniversityExamSystem.Repositories;
using UniversityExamSystem.Services;

namespace UniversityExamSystem.Controllers
{
    [Authorize(Policy = "RequireCollegeRole")]
    public class CollegeController : Controller
    {
        private readonly IStudentRepository _studentRepo;
        private readonly IExaminationRepository _examRepo;
        private readonly IAcademicRepository _academicRepo;
        private readonly IResultRepository _resultRepo;
        private readonly IAuthService _authService;
        private readonly IFileService _fileService;
        private readonly IQrCodeService _qrCodeService;

        public CollegeController(
            IStudentRepository studentRepo,
            IExaminationRepository examRepo,
            IAcademicRepository academicRepo,
            IResultRepository resultRepo,
            IAuthService authService,
            IFileService fileService,
            IQrCodeService qrCodeService)
        {
            _studentRepo = studentRepo;
            _examRepo = examRepo;
            _academicRepo = academicRepo;
            _resultRepo = resultRepo;
            _authService = authService;
            _fileService = fileService;
            _qrCodeService = qrCodeService;
        }

        private int GetCurrentCollegeId()
        {
            var refClaim = User.FindFirstValue("ReferenceId");
            if (int.TryParse(refClaim, out int cid)) return cid;
            return 0;
        }

        public async Task<IActionResult> Index()
        {
            var collegeId = GetCurrentCollegeId();
            if (collegeId == 0) return Forbid();
            var college = await _academicRepo.GetCollegeByIdAsync(collegeId);
            ViewBag.College = college;

            var students = await _studentRepo.GetStudentsAsync(collegeId: collegeId);
            ViewBag.Students = students;

            var exams = await _examRepo.GetActiveExaminationsAsync();
            ViewBag.ActiveExams = exams;

            var applications = await _examRepo.GetApplicationsAsync(collegeId: collegeId);
            ViewBag.Applications = applications;

            return View();
        }

        public async Task<IActionResult> Students(int? programId, int? semesterId, string? status, string? search)
        {
            var collegeId = GetCurrentCollegeId();
            if (collegeId == 0) return Forbid();
            ViewBag.College = await _academicRepo.GetCollegeByIdAsync(collegeId);
            ViewBag.Programs = await _academicRepo.GetProgramsAsync();
            ViewBag.Semesters = await _academicRepo.GetSemestersAsync();
            ViewBag.SelectedProgram = programId;
            ViewBag.SelectedSemester = semesterId;
            ViewBag.SelectedStatus = status;
            ViewBag.Search = search;

            var list = await _studentRepo.GetStudentsAsync(collegeId, programId, semesterId, status, search);
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> RegisterStudent()
        {
            var collegeId = GetCurrentCollegeId();
            if (collegeId == 0) return Forbid();
            ViewBag.College = await _academicRepo.GetCollegeByIdAsync(collegeId);
            ViewBag.Faculties = await _academicRepo.GetFacultiesAsync();
            ViewBag.Programs = await _academicRepo.GetProgramsAsync();
            ViewBag.AcademicYears = await _academicRepo.GetAcademicYearsAsync();
            ViewBag.Semesters = await _academicRepo.GetSemestersAsync();

            return View(new StudentRegistrationViewModel
            {
                CollegeId = collegeId,
                DateOfBirth = DateTime.Today.AddYears(-19)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterStudent(StudentRegistrationViewModel model)
        {
            var collegeId = GetCurrentCollegeId();
            if (collegeId == 0) return Forbid();
            model.CollegeId = collegeId;

            if (!ModelState.IsValid)
            {
                ViewBag.College = await _academicRepo.GetCollegeByIdAsync(collegeId);
                ViewBag.Faculties = await _academicRepo.GetFacultiesAsync();
                ViewBag.Programs = await _academicRepo.GetProgramsAsync();
                ViewBag.AcademicYears = await _academicRepo.GetAcademicYearsAsync();
                ViewBag.Semesters = await _academicRepo.GetSemestersAsync();
                return View(model);
            }

            try
            {
                string? photoUrl = null;
                string? docUrl = null;

                if (model.PhotoFile != null) photoUrl = await _fileService.SaveFileAsync(model.PhotoFile, "students");
                if (model.IdentityDocumentFile != null) docUrl = await _fileService.SaveFileAsync(model.IdentityDocumentFile, "documents");

                var student = new Student
                {
                    FullName = model.FullName.Trim(),
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender,
                    Email = model.Email.Trim().ToLowerInvariant(),
                    MobileNumber = model.MobileNumber.Trim(),
                    Address = model.Address.Trim(),
                    FacultyId = model.FacultyId,
                    ProgramId = model.ProgramId,
                    CollegeId = collegeId,
                    AcademicYearId = model.AcademicYearId,
                    CurrentSemesterId = model.CurrentSemesterId,
                    PreviousQualification = model.PreviousQualification,
                    PhotoUrl = photoUrl,
                    IdentityDocumentUrl = docUrl
                };

                var passwordHash = _authService.HashPassword(model.Password);
                var (studentId, uniqueId, appNo) = await _studentRepo.RegisterStudentAsync(student, passwordHash);

                TempData["SuccessMessage"] = $"Student '{student.FullName}' registered successfully with ID: {uniqueId} (Application #{appNo}).";
                return RedirectToAction(nameof(Students));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.College = await _academicRepo.GetCollegeByIdAsync(collegeId);
                ViewBag.Faculties = await _academicRepo.GetFacultiesAsync();
                ViewBag.Programs = await _academicRepo.GetProgramsAsync();
                ViewBag.AcademicYears = await _academicRepo.GetAcademicYearsAsync();
                ViewBag.Semesters = await _academicRepo.GetSemestersAsync();
                return View(model);
            }
        }

        public async Task<IActionResult> Applications(int? examId, string? status)
        {
            var collegeId = GetCurrentCollegeId();
            if (collegeId == 0) return Forbid();
            ViewBag.College = await _academicRepo.GetCollegeByIdAsync(collegeId);
            ViewBag.Examinations = await _examRepo.GetExaminationsAsync();
            ViewBag.SelectedExam = examId;
            ViewBag.SelectedStatus = status;

            var apps = await _examRepo.GetApplicationsAsync(examId, collegeId, null, status);
            return View(apps);
        }

        public async Task<IActionResult> AdmitCards(int? examId)
        {
            var collegeId = GetCurrentCollegeId();
            if (collegeId == 0) return Forbid();
            var exams = await _examRepo.GetExaminationsAsync();
            ViewBag.Examinations = exams;

            var selectedExamId = examId ?? exams.FirstOrDefault()?.ExaminationId ?? 0;
            ViewBag.SelectedExamId = selectedExamId;

            var cards = await _examRepo.GetAdmitCardsForExamAsync(selectedExamId, collegeId);
            foreach (var card in cards)
            {
                if (!string.IsNullOrEmpty(card.QrCodeHash))
                {
                    card.QrCodeBase64 = _qrCodeService.GenerateQrCodeBase64(card.QrCodeHash);
                }
            }

            return View(cards);
        }

        public async Task<IActionResult> Results(int? examId, int? programId)
        {
            var collegeId = GetCurrentCollegeId();
            if (collegeId == 0) return Forbid();
            var exams = await _examRepo.GetExaminationsAsync();
            ViewBag.Examinations = exams;
            ViewBag.Programs = await _academicRepo.GetProgramsAsync();

            var selectedExamId = examId ?? exams.FirstOrDefault(e => e.Status == "ResultPublished")?.ExaminationId ?? 0;
            ViewBag.SelectedExamId = selectedExamId;
            ViewBag.SelectedProgramId = programId;

            var results = await _resultRepo.GetResultsByExamAsync(selectedExamId, collegeId, programId);
            return View(results);
        }
    }
}
