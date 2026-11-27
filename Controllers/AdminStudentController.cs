using System.Security.Claims;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityExamSystem.Models;
using UniversityExamSystem.Repositories;

namespace UniversityExamSystem.Controllers
{
    //[Authorize(Policy = "RequireAdminRole")]
    public class AdminStudentController : Controller
    {
        private readonly IStudentRepository _studentRepo;
        private readonly IAcademicRepository _academicRepo;

        public AdminStudentController(IStudentRepository studentRepo, IAcademicRepository academicRepo)
        {
            _studentRepo = studentRepo;
            _academicRepo = academicRepo;
        }

        public async Task<IActionResult> Index(int? collegeId, int? programId, int? semesterId, string? status, string? search)
        {
            ViewBag.Colleges = await _academicRepo.GetCollegesAsync();
            ViewBag.Programs = await _academicRepo.GetProgramsAsync();
            ViewBag.Semesters = await _academicRepo.GetSemestersAsync();
            ViewBag.SelectedCollege = collegeId;
            ViewBag.SelectedProgram = programId;
            ViewBag.SelectedSemester = semesterId;
            ViewBag.SelectedStatus = status;
            ViewBag.Search = search;

            var students = await _studentRepo.GetStudentsAsync(collegeId, programId, semesterId, status, search);
            return View(students);
        }

        public async Task<IActionResult> Details(int id)
        {
            var student = await _studentRepo.GetProfileAsync(id);
            if (student == null) return NotFound();

            ViewBag.Documents = await _studentRepo.GetStudentDocumentsAsync(id);
            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(int studentId, string status, string remarks)
        {
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int verifiedBy);
            var (success, regNumber) = await _studentRepo.VerifyRegistrationAsync(studentId, status, remarks, verifiedBy);

            if (status == "Approved")
            {
                TempData["SuccessMessage"] = $"Student registration approved successfully! Generated Registration Number: {regNumber}";
            }
            else
            {
                TempData["InfoMessage"] = $"Student registration marked as '{status}'.";
            }

            return RedirectToAction(nameof(Details), new { id = studentId });
        }

        public async Task<IActionResult> Export(int? collegeId, int? programId)
        {
            var dt = await _studentRepo.ExportStudentsToDataTableAsync(collegeId, programId);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add(dt, "Students");
            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Students_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            );
        }
    }
}
