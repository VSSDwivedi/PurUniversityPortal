using System.Security.Claims;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityExamSystem.Models;
using UniversityExamSystem.Repositories;

namespace UniversityExamSystem.Controllers
{
    //[Authorize(Policy = "RequireExaminerRole")]
    public class AdminMarksController : Controller
    {
        private readonly IMarksRepository _marksRepo;
        private readonly IExaminationRepository _examRepo;
        private readonly IAcademicRepository _academicRepo;

        public AdminMarksController(
            IMarksRepository marksRepo,
            IExaminationRepository examRepo,
            IAcademicRepository academicRepo)
        {
            _marksRepo = marksRepo;
            _examRepo = examRepo;
            _academicRepo = academicRepo;
        }

        public async Task<IActionResult> Index(int? examId, int? programId, int? semesterId, int? subjectId, int? collegeId)
        {
            ViewBag.Examinations = await _examRepo.GetExaminationsAsync();
            ViewBag.Programs = await _academicRepo.GetProgramsAsync();
            ViewBag.Semesters = await _academicRepo.GetSemestersAsync();
            ViewBag.Colleges = await _academicRepo.GetCollegesAsync();

            var subjects = await _academicRepo.GetSubjectsAsync(programId, semesterId);
            ViewBag.Subjects = subjects;

            ViewBag.SelectedExam = examId;
            ViewBag.SelectedProgram = programId;
            ViewBag.SelectedSemester = semesterId;
            ViewBag.SelectedSubject = subjectId;
            ViewBag.SelectedCollege = collegeId;

            if (examId.HasValue && subjectId.HasValue)
            {
                var marks = await _marksRepo.GetMarksAsync(examId.Value, subjectId.Value, collegeId);
                var subject = await _academicRepo.GetSubjectByIdAsync(subjectId.Value);
                var exam = await _examRepo.GetExaminationByIdAsync(examId.Value);

                var vm = new MarksEntryViewModel
                {
                    ExaminationId = examId.Value,
                    ProgramId = programId ?? 0,
                    SemesterId = semesterId ?? 0,
                    SubjectId = subjectId.Value,
                    CollegeId = collegeId,
                    Subject = subject,
                    Examination = exam,
                    Rows = marks.Select(m => new MarkEntryRow
                    {
                        StudentId = m.StudentId,
                        StudentName = m.StudentName ?? "",
                        RollNumber = m.RollNumber ?? "",
                        RegistrationNumber = m.RegistrationNumber ?? "",
                        InternalMarks = m.InternalMarks,
                        ExternalMarks = m.ExternalMarks,
                        PracticalMarks = m.PracticalMarks,
                        TotalMarks = m.TotalMarks,
                        Grade = m.Grade ?? "F",
                        GradePoint = m.GradePoint ?? 0,
                        IsAbsent = m.IsAbsent,
                        Status = m.Status
                    }).ToList()
                };

                return View(vm);
            }

            return View(new MarksEntryViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(MarksEntryViewModel model, string actionType)
        {
            if (model.Rows == null || !model.Rows.Any())
            {
                TempData["ErrorMessage"] = "No students or marks found to save.";
                return RedirectToAction(nameof(Index), new { examId = model.ExaminationId, subjectId = model.SubjectId, collegeId = model.CollegeId });
            }

            var status = actionType == "Submit" ? "Submitted" : "Draft";
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int savedBy);
            await _marksRepo.BatchSaveMarksAsync(model.Rows, model.ExaminationId, model.SubjectId, savedBy, status);

            TempData["SuccessMessage"] = actionType == "Submit" 
                ? "Marks submitted for controller verification successfully!" 
                : "Marks draft saved successfully.";

            return RedirectToAction(nameof(Index), new { examId = model.ExaminationId, programId = model.ProgramId, semesterId = model.SemesterId, subjectId = model.SubjectId, collegeId = model.CollegeId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveAndLock(int examId, int subjectId)
        {
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int approvedBy);
            await _marksRepo.ApproveAndLockMarksAsync(examId, subjectId, approvedBy);
            TempData["SuccessMessage"] = "Marks approved and locked successfully! Results can now be processed.";
            return RedirectToAction(nameof(Index), new { examId, subjectId });
        }

        public async Task<IActionResult> ExportTemplate(int examId, int subjectId, int? collegeId)
        {
            var dt = await _marksRepo.ExportMarksTemplateAsync(examId, subjectId, collegeId);
            var subject = await _academicRepo.GetSubjectByIdAsync(subjectId);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add(dt, "Marks");
            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            var subCode = subject?.SubjectCode ?? "SUB";
            return File(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Marks_Template_{subCode}_Exam{examId}.xlsx"
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(int examId, int subjectId, int? collegeId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select an Excel file (.xlsx) to import.";
                return RedirectToAction(nameof(Index), new { examId, subjectId, collegeId });
            }

            try
            {
                using var stream = file.OpenReadStream();
                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1);
                var range = worksheet.RangeUsed();
                if (range == null)
                {
                    TempData["ErrorMessage"] = "The uploaded spreadsheet is empty.";
                    return RedirectToAction(nameof(Index), new { examId, subjectId, collegeId });
                }

                var dt = range.AsTable().AsNativeDataTable();
                int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int importedBy);
                var importedCount = await _marksRepo.ImportMarksFromDataTableAsync(dt, examId, subjectId, importedBy);

                TempData["SuccessMessage"] = $"Successfully imported marks for {importedCount} students!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Failed to import Excel file: {ex.Message}";
            }

            return RedirectToAction(nameof(Index), new { examId, subjectId, collegeId });
        }
    }
}
