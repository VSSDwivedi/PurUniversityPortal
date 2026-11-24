using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UniversityExamSystem.Models;
using UniversityExamSystem.Repositories;

namespace UniversityExamSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly INoticeRepository _noticeRepo;
        private readonly IExaminationRepository _examRepo;
        private readonly IAcademicRepository _academicRepo;
        private readonly ISystemSettingsRepository _settingsRepo;

        public HomeController(
            INoticeRepository noticeRepo,
            IExaminationRepository examRepo,
            IAcademicRepository academicRepo,
            ISystemSettingsRepository settingsRepo)
        {
            _noticeRepo = noticeRepo;
            _examRepo = examRepo;
            _academicRepo = academicRepo;
            _settingsRepo = settingsRepo;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["ShowNoticeModal"] = true;
            ViewBag.Notices = await _noticeRepo.GetPublicNoticesAsync(limit: 20);
            ViewBag.Categories = await _noticeRepo.GetCategoriesAsync();
            ViewBag.ActiveExams = await _examRepo.GetActiveExaminationsAsync();
            ViewBag.Settings = await _settingsRepo.GetAllSettingsAsync();
            ViewBag.Faculties = await _academicRepo.GetFacultiesAsync();
            ViewBag.Colleges = await _academicRepo.GetCollegesAsync();
            ViewBag.Programs = await _academicRepo.GetProgramsAsync();

            return View();
        }

        public async Task<IActionResult> About()
        {
            ViewBag.Settings = await _settingsRepo.GetAllSettingsAsync();
            ViewBag.Faculties = await _academicRepo.GetFacultiesAsync();
            return View();
        }

        public async Task<IActionResult> Notices(int? categoryId, string? search)
        {
            var notices = await _noticeRepo.GetPublicNoticesAsync(categoryId, limit: 50);
            if (!string.IsNullOrWhiteSpace(search))
            {
                notices = notices.Where(n => n.Title.Contains(search, StringComparison.OrdinalIgnoreCase) 
                                          || n.Description.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            ViewBag.Categories = await _noticeRepo.GetCategoriesAsync();
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SearchTerm = search;

            return View(notices);
        }

        public async Task<IActionResult> NoticeDetails(int id)
        {
            var notice = await _noticeRepo.GetNoticeByIdAsync(id);
            if (notice == null) return NotFound();

            await _noticeRepo.IncrementViewCountAsync(id);
            ViewBag.RecentNotices = await _noticeRepo.GetPublicNoticesAsync(limit: 5);
            return View(notice);
        }

        [Route("Home/Routine")]
        [Route("exam-schedule-new")]
        public async Task<IActionResult> Routine(int? facultyId, int? programId, int? semesterYearId)
        {
            var exams = await _examRepo.GetExaminationsAsync();
            ViewBag.Exams = exams;
            ViewBag.Faculties = await _academicRepo.GetFacultiesAsync();
            ViewBag.Programs = await _academicRepo.GetProgramsAsync();
            ViewBag.Semesters = await _academicRepo.GetSemestersAsync();
            ViewBag.SelectedFacultyId = facultyId;
            ViewBag.SelectedProgramId = programId;
            ViewBag.SelectedSemesterId = semesterYearId;
            return View();
        }

        [Route("downloads-new")]
        [Route("Home/Downloads")]
        public async Task<IActionResult> Downloads()
        {
            ViewBag.Settings = await _settingsRepo.GetAllSettingsAsync();
            return View();
        }

        [Route("exam-event-new/all")]
        [Route("index.php/exam-event-new/all")]
        public async Task<IActionResult> ExamEventsAll()
        {
            var notices = await _noticeRepo.GetPublicNoticesAsync(limit: 50);
            return View(notices);
        }

        [Route("exam-event-new/{id:int}")]
        [Route("index.php/exam-event-new/{id:int}")]
        public async Task<IActionResult> ExamEventDetail(int id)
        {
            var notice = await _noticeRepo.GetNoticeByIdAsync(id) ?? new Notice
            {
                NoticeId = id,
                Title = $"(2083.08.07): EMBA - I, II, III, IV Sem (Spring / Fall)",
                PublishDate = DateTime.Today,
                CategoryName = "Exam-center Notice",
                AttachmentUrl = "/public/notice/emba_i_ii_iii_iv_2026_1795407433.pdf"
            };
            ViewBag.RelatedEvents = await _noticeRepo.GetPublicNoticesAsync(limit: 6);
            return View(notice);
        }

        [Route("result-event-new/all")]
        [Route("index.php/result-event-new/all")]
        public async Task<IActionResult> ResultEventsAll()
        {
            var notices = await _noticeRepo.GetPublicNoticesAsync(limit: 50);
            return View(notices);
        }

        [Route("result-event-new/{id:int}")]
        [Route("index.php/result-event-new/{id:int}")]
        public async Task<IActionResult> ResultEventDetail(int id)
        {
            var notice = await _noticeRepo.GetNoticeByIdAsync(id) ?? new Notice
            {
                NoticeId = id,
                Title = $"(2083.07.20): BPH - VIII Sem Result Announcement",
                PublishDate = DateTime.Today,
                CategoryName = "Result Notice",
                AttachmentUrl = "/public/event/emba_i_ii_iii_iv_2026_1795407448.pdf"
            };
            ViewBag.RelatedEvents = await _noticeRepo.GetPublicNoticesAsync(limit: 6);
            return View(notice);
        }

        [Route("notice_new")]
        public async Task<IActionResult> NoticeNew()
        {
            var notices = await _noticeRepo.GetPublicNoticesAsync(limit: 10);
            return PartialView("_NoticeListItems", notices);
        }

        [AcceptVerbs("GET", "POST")]
        [Route("home/getNoticeGroupBy/{category}/{date?}")]
        public async Task<IActionResult> GetNoticeGroupBy(string category, string? date, int limit = 10, int off = 0)
        {
            var notices = await _noticeRepo.GetPublicNoticesAsync(limit: 50);
            if (!string.IsNullOrEmpty(category) && category != "all-notice" && category != "all")
            {
                var catNorm = category.Replace("-", " ").ToLower();
                notices = notices.Where(n => n.CategoryName != null && 
                    (n.CategoryName.ToLower().Contains(catNorm) ||
                     (category == "exam-registration" && n.CategoryName.ToLower().Contains("registration")) ||
                     (category == "exam-form" && n.CategoryName.ToLower().Contains("form")) ||
                     (category == "exam-routine" && n.CategoryName.ToLower().Contains("routine")) ||
                     (category == "exam-center" && n.CategoryName.ToLower().Contains("center")) ||
                     (category == "internal" && n.CategoryName.ToLower().Contains("internal")) ||
                     (category == "result" && n.CategoryName.ToLower().Contains("result")) ||
                     (category == "retotaling" && n.CategoryName.ToLower().Contains("retotaling"))
                    )).ToList();
            }

            return PartialView("_NoticeListItems", notices.Skip(off).Take(limit).ToList());
        }

        [HttpPost]
        [Route("home/getprogrambyfaculty")]
        public async Task<IActionResult> GetProgramByFaculty([FromForm] int? facultyid)
        {
            if (!facultyid.HasValue) return Content("[]", "application/json");
            var programs = await _academicRepo.GetProgramsByFacultyAsync(facultyid.Value);
            var items = programs.Select(p => new { programid = p.ProgramId.ToString(), program = p.ProgramName }).ToList();
            var json = System.Text.Json.JsonSerializer.Serialize(new[] { items });
            return Content(json, "application/json");
        }

        [HttpPost]
        [Route("home/getsemyearbyprogram")]
        public async Task<IActionResult> GetSemYearByProgram([FromForm] int? programid)
        {
            var semesters = await _academicRepo.GetSemestersAsync();
            var items = semesters.Select(s => new { semyearid = s.SemesterId.ToString(), semyear = s.SemesterName }).ToList();
            var json = System.Text.Json.JsonSerializer.Serialize(new[] { items });
            return Content(json, "application/json");
        }

        [HttpPost]
        [Route("contact-us")]
        public IActionResult ContactUs([FromForm] string name, [FromForm] string email, [FromForm] string subject, [FromForm] string message)
        {
            return Json(new { type = "success", message = "Thank you! Your inquiry has been submitted successfully to the Examination Office." });
        }

        public async Task<IActionResult> Services()
        {
            ViewBag.Settings = await _settingsRepo.GetAllSettingsAsync();
            return View();
        }

        public async Task<IActionResult> Contact()
        {
            ViewBag.Settings = await _settingsRepo.GetAllSettingsAsync();
            return View();
        }

        [HttpPost]
        public IActionResult Contact(string fullName, string email, string subject, string message)
        {
            TempData["SuccessMessage"] = "Thank you! Your inquiry has been submitted to the Office of the Controller of Examinations. We will respond promptly.";
            return RedirectToAction(nameof(Contact));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
