using System.Security.Claims;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityExamSystem.Models;
using UniversityExamSystem.Repositories;
using UniversityExamSystem.Services;

namespace UniversityExamSystem.Controllers
{
    //[Authorize(Policy = "RequireControllerRole")]
    public class AdminResultController : Controller
    {
        private readonly IResultRepository _resultRepo;
        private readonly IExaminationRepository _examRepo;
        private readonly IAcademicRepository _academicRepo;
        private readonly IQrCodeService _qrCodeService;

        public AdminResultController(
            IResultRepository resultRepo,
            IExaminationRepository examRepo,
            IAcademicRepository academicRepo,
            IQrCodeService qrCodeService)
        {
            _resultRepo = resultRepo;
            _examRepo = examRepo;
            _academicRepo = academicRepo;
            _qrCodeService = qrCodeService;
        }

        public async Task<IActionResult> Index()
        {
            var exams = await _examRepo.GetExaminationsAsync();
            return View(exams);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Process(int examId)
        {
            try
            {
                int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int processedBy);
                await _resultRepo.ProcessResultsAsync(examId, processedBy);
                TempData["SuccessMessage"] = "Examination results calculated and processed successfully! You can now review the result ledger before publishing.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to process results: {ex.Message}";
            }

            return RedirectToAction(nameof(ViewLedger), new { examId });
        }

        public async Task<IActionResult> ViewLedger(int examId, int? collegeId, int? programId)
        {
            var exam = await _examRepo.GetExaminationByIdAsync(examId);
            if (exam == null) return NotFound();

            ViewBag.Examination = exam;
            ViewBag.Colleges = await _academicRepo.GetCollegesAsync();
            ViewBag.Programs = await _academicRepo.GetProgramsAsync();
            ViewBag.SelectedCollege = collegeId;
            ViewBag.SelectedProgram = programId;

            var results = await _resultRepo.GetResultsByExamAsync(examId, collegeId, programId);
            return View(results);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int examId, bool publish = true)
        {
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int publishedBy);
            await _resultRepo.PublishResultsAsync(examId, publishedBy, publish);
            TempData["SuccessMessage"] = publish 
                ? "Examination results PUBLISHED officially! Students can now check their results and download grade sheets online."
                : "Examination results UNPUBLISHED. Access revoked from public view.";

            return RedirectToAction(nameof(ViewLedger), new { examId });
        }

        public async Task<IActionResult> ExportExcel(int examId, int? collegeId, int? programId)
        {
            var dt = await _resultRepo.ExportResultsToDataTableAsync(examId, collegeId, programId);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add(dt, "Results");
            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Results_Exam{examId}_{DateTime.Now:yyyyMMdd}.xlsx"
            );
        }

        public async Task<IActionResult> BulkGradeSheets(int examId, int? collegeId)
        {
            var exam = await _examRepo.GetExaminationByIdAsync(examId);
            ViewBag.Examination = exam;
            ViewBag.GradeScales = await _academicRepo.GetGradeScalesAsync();

            var results = await _resultRepo.GetResultsByExamAsync(examId, collegeId);
            foreach (var r in results)
            {
                if (!string.IsNullOrEmpty(r.GradeSheetQrCode))
                {
                    r.GradeSheetQrCodeBase64 = _qrCodeService.GenerateQrCodeBase64(r.GradeSheetQrCode);
                }
            }

            return View(results);
        }
    }
}
