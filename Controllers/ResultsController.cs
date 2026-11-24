using Microsoft.AspNetCore.Mvc;
using UniversityExamSystem.Models;
using UniversityExamSystem.Repositories;
using UniversityExamSystem.Services;

namespace UniversityExamSystem.Controllers
{
    public class ResultsController : Controller
    {
        private readonly IResultRepository _resultRepo;
        private readonly IExaminationRepository _examRepo;
        private readonly IAcademicRepository _academicRepo;
        private readonly IQrCodeService _qrCodeService;
        private readonly ISystemSettingsRepository _settingsRepo;

        public ResultsController(
            IResultRepository resultRepo,
            IExaminationRepository examRepo,
            IAcademicRepository academicRepo,
            IQrCodeService qrCodeService,
            ISystemSettingsRepository settingsRepo)
        {
            _resultRepo = resultRepo;
            _examRepo = examRepo;
            _academicRepo = academicRepo;
            _qrCodeService = qrCodeService;
            _settingsRepo = settingsRepo;
        }

        [HttpGet]
        [Route("Results")]
        [Route("Results/Index")]
        [Route("find-results_new")]
        public async Task<IActionResult> Index()
        {
            await LoadDropDownsAsync();
            return View(new ResultSearchViewModel());
        }

        [AcceptVerbs("GET", "POST")]
        [Route("Results/Search")]
        [Route("results")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Search(ResultSearchViewModel model)
        {
            await LoadDropDownsAsync();
            model.GradeScales = await _academicRepo.GetGradeScalesAsync();

            // Construct search term from puexam segmented inputs if provided
            var searchTerm = model.SearchTerm?.Trim() ?? string.Empty;

            if (!string.IsNullOrEmpty(model.SymbolNumber))
            {
                searchTerm = model.SymbolNumber.Trim();
            }
            else if (!string.IsNullOrEmpty(model.RegistrationNumber))
            {
                searchTerm = model.RegistrationNumber.Trim();
            }
            else if (!string.IsNullOrEmpty(model.Rn1) && !string.IsNullOrEmpty(model.Rn4))
            {
                searchTerm = $"{model.Rn1}-{model.Rn2}-{model.Rn3}-{model.Rn4}-{model.Rn5}".Trim();
            }

            model.SearchTerm = searchTerm;

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                ModelState.AddModelError("SearchTerm", "Please enter a valid Symbol Number or Registration Number.");
                model.Message = "Please enter your Symbol Number or Registration Number to check results.";
                return View("Index", model);
            }

            var result = await _resultRepo.SearchResultAsync(searchTerm, model.ExaminationId);
            model.Searched = true;

            if (result != null)
            {
                if (!string.IsNullOrEmpty(result.GradeSheetQrCode))
                {
                    result.GradeSheetQrCodeBase64 = _qrCodeService.GenerateQrCodeBase64(result.GradeSheetQrCode);
                }
                model.Result = result;
            }
            else
            {
                model.Message = $"No published result found matching credentials '{searchTerm}'. Please verify your details or contact the Examination Office.";
            }

            return View("Index", model);
        }
        [HttpGet]
        public async Task<IActionResult> GradeSheet(int id)
        {
            var result = await _resultRepo.GetResultByIdAsync(id);
            if (result == null || !result.IsPublished)
            {
                return NotFound("Published grade sheet not found.");
            }

            if (!string.IsNullOrEmpty(result.GradeSheetQrCode))
            {
                result.GradeSheetQrCodeBase64 = _qrCodeService.GenerateQrCodeBase64(result.GradeSheetQrCode);
            }

            ViewBag.GradeScales = await _academicRepo.GetGradeScalesAsync();
            ViewBag.Settings = await _settingsRepo.GetAllSettingsAsync();

            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetProgramsByFaculty(int facultyId)
        {
            var programs = await _academicRepo.GetProgramsByFacultyAsync(facultyId);
            return Json(programs.Select(p => new { programId = p.ProgramId, programName = p.ProgramName, programCode = p.ProgramCode }));
        }

        private async Task LoadDropDownsAsync()
        {
            ViewBag.Examinations = await _examRepo.GetExaminationsAsync();
            ViewBag.Faculties = await _academicRepo.GetFacultiesAsync();
            ViewBag.Programs = await _academicRepo.GetProgramsAsync();
            ViewBag.Semesters = await _academicRepo.GetSemestersAsync();
        }
    }
}
