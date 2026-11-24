using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityExamSystem.Models;
using UniversityExamSystem.Repositories;

namespace UniversityExamSystem.Controllers
{
    [Authorize(Policy = "RequireAdminRole")]
    public class AdminController : Controller
    {
        private readonly ISystemSettingsRepository _settingsRepo;
        private readonly IAcademicRepository _academicRepo;
        private readonly IStudentRepository _studentRepo;
        private readonly IExaminationRepository _examRepo;
        private readonly INoticeRepository _noticeRepo;

        public AdminController(
            ISystemSettingsRepository settingsRepo,
            IAcademicRepository academicRepo,
            IStudentRepository studentRepo,
            IExaminationRepository examRepo,
            INoticeRepository noticeRepo)
        {
            _settingsRepo = settingsRepo;
            _academicRepo = academicRepo;
            _studentRepo = studentRepo;
            _examRepo = examRepo;
            _noticeRepo = noticeRepo;
        }

        public async Task<IActionResult> Index()
        {
            var stats = await _settingsRepo.GetDashboardStatsAsync();
            stats.RecentApplications = (await _examRepo.GetApplicationsAsync()).Take(5).ToList();
            stats.RecentStudents = (await _studentRepo.GetStudentsAsync()).Take(5).ToList();
            stats.RecentNotices = (await _noticeRepo.GetAllNoticesAsync()).Take(5).ToList();

            return View(stats);
        }

        // ======================== FACULTIES ========================
        public async Task<IActionResult> Faculties()
        {
            var faculties = await _academicRepo.GetFacultiesAsync();
            return View(faculties);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFaculty(Faculty faculty)
        {
            if (string.IsNullOrWhiteSpace(faculty.FacultyName) || string.IsNullOrWhiteSpace(faculty.FacultyCode))
            {
                TempData["ErrorMessage"] = "Faculty Code and Name are required.";
                return RedirectToAction(nameof(Faculties));
            }

            await _academicRepo.AddFacultyAsync(faculty);
            TempData["SuccessMessage"] = $"Faculty '{faculty.FacultyName}' created successfully.";
            return RedirectToAction(nameof(Faculties));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFaculty(Faculty faculty)
        {
            await _academicRepo.UpdateFacultyAsync(faculty);
            TempData["SuccessMessage"] = "Faculty updated successfully.";
            return RedirectToAction(nameof(Faculties));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFaculty(int facultyId)
        {
            await _academicRepo.DeleteFacultyAsync(facultyId);
            TempData["SuccessMessage"] = "Faculty deactivated successfully.";
            return RedirectToAction(nameof(Faculties));
        }

        // ======================== PROGRAMS ========================
        public async Task<IActionResult> Programs()
        {
            ViewBag.Faculties = await _academicRepo.GetFacultiesAsync();
            var programs = await _academicRepo.GetProgramsAsync();
            return View(programs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProgram(ProgramModel program)
        {
            if (string.IsNullOrWhiteSpace(program.ProgramName) || string.IsNullOrWhiteSpace(program.ProgramCode))
            {
                TempData["ErrorMessage"] = "Program Code and Name are required.";
                return RedirectToAction(nameof(Programs));
            }

            await _academicRepo.AddProgramAsync(program);
            TempData["SuccessMessage"] = $"Program '{program.ProgramName}' created successfully.";
            return RedirectToAction(nameof(Programs));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProgram(ProgramModel program)
        {
            await _academicRepo.UpdateProgramAsync(program);
            TempData["SuccessMessage"] = "Program updated successfully.";
            return RedirectToAction(nameof(Programs));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProgram(int programId)
        {
            await _academicRepo.DeleteProgramAsync(programId);
            TempData["SuccessMessage"] = "Program deactivated successfully.";
            return RedirectToAction(nameof(Programs));
        }

        // ======================== COLLEGES ========================
        public async Task<IActionResult> Colleges()
        {
            var colleges = await _academicRepo.GetCollegesAsync();
            return View(colleges);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCollege(College college)
        {
            if (string.IsNullOrWhiteSpace(college.CollegeName) || string.IsNullOrWhiteSpace(college.CollegeCode))
            {
                TempData["ErrorMessage"] = "College Code and Name are required.";
                return RedirectToAction(nameof(Colleges));
            }

            await _academicRepo.AddCollegeAsync(college);
            TempData["SuccessMessage"] = $"College '{college.CollegeName}' added successfully.";
            return RedirectToAction(nameof(Colleges));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCollege(College college)
        {
            await _academicRepo.UpdateCollegeAsync(college);
            TempData["SuccessMessage"] = "College updated successfully.";
            return RedirectToAction(nameof(Colleges));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCollege(int collegeId)
        {
            await _academicRepo.DeleteCollegeAsync(collegeId);
            TempData["SuccessMessage"] = "College deactivated successfully.";
            return RedirectToAction(nameof(Colleges));
        }

        // ======================== SUBJECTS ========================
        public async Task<IActionResult> Subjects(int? programId, int? semesterId)
        {
            ViewBag.Programs = await _academicRepo.GetProgramsAsync();
            ViewBag.Semesters = await _academicRepo.GetSemestersAsync();
            ViewBag.SelectedProgramId = programId;
            ViewBag.SelectedSemesterId = semesterId;

            var subjects = await _academicRepo.GetSubjectsAsync(programId, semesterId);
            return View(subjects);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSubject(Subject subject)
        {
            if (string.IsNullOrWhiteSpace(subject.SubjectName) || string.IsNullOrWhiteSpace(subject.SubjectCode))
            {
                TempData["ErrorMessage"] = "Subject Code and Name are required.";
                return RedirectToAction(nameof(Subjects));
            }

            await _academicRepo.AddSubjectAsync(subject);
            TempData["SuccessMessage"] = $"Subject '{subject.SubjectName}' ({subject.SubjectCode}) added successfully.";
            return RedirectToAction(nameof(Subjects), new { programId = subject.ProgramId, semesterId = subject.SemesterId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSubject(Subject subject)
        {
            await _academicRepo.UpdateSubjectAsync(subject);
            TempData["SuccessMessage"] = "Subject updated successfully.";
            return RedirectToAction(nameof(Subjects), new { programId = subject.ProgramId, semesterId = subject.SemesterId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSubject(int subjectId)
        {
            await _academicRepo.DeleteSubjectAsync(subjectId);
            TempData["SuccessMessage"] = "Subject deactivated successfully.";
            return RedirectToAction(nameof(Subjects));
        }

        // ======================== GRADE SCALES ========================
        public async Task<IActionResult> GradeScales()
        {
            var scales = await _academicRepo.GetGradeScalesAsync();
            return View(scales);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGradeScale(GradeScale scale)
        {
            await _academicRepo.UpdateGradeScaleAsync(scale);
            TempData["SuccessMessage"] = $"Grade Scale boundary '{scale.Grade}' updated successfully.";
            return RedirectToAction(nameof(GradeScales));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGradeScale(GradeScale scale)
        {
            await _academicRepo.AddGradeScaleAsync(scale);
            TempData["SuccessMessage"] = $"New grade boundary '{scale.Grade}' created successfully.";
            return RedirectToAction(nameof(GradeScales));
        }

        // ======================== SYSTEM SETTINGS ========================
        public async Task<IActionResult> Settings()
        {
            var settings = await _settingsRepo.GetAllSettingsAsync();
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSettings(IFormCollection form)
        {
            foreach (var key in form.Keys)
            {
                if (key.StartsWith("__")) continue; // Ignore anti-forgery token etc.
                var value = form[key].ToString();
                await _settingsRepo.UpdateSettingAsync(key, value);
            }

            TempData["SuccessMessage"] = "System settings updated successfully.";
            return RedirectToAction(nameof(Settings));
        }
    }
}
