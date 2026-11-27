using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityExamSystem.Models;
using UniversityExamSystem.Repositories;
using UniversityExamSystem.Services;

namespace UniversityExamSystem.Controllers
{
    //[Authorize(Policy = "RequireAdminRole")]
    public class AdminNoticeController : Controller
    {
        private readonly INoticeRepository _noticeRepo;
        private readonly IFileService _fileService;

        public AdminNoticeController(INoticeRepository noticeRepo, IFileService fileService)
        {
            _noticeRepo = noticeRepo;
            _fileService = fileService;
        }

        public async Task<IActionResult> Index()
        {
            var notices = await _noticeRepo.GetAllNoticesAsync();
            return View(notices);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _noticeRepo.GetCategoriesAsync();
            return View(new Notice
            {
                PublishDate = DateTime.Today,
                IsActive = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Notice notice, IFormFile? attachmentFile)
        {
            if (string.IsNullOrWhiteSpace(notice.Title) || string.IsNullOrWhiteSpace(notice.Description))
            {
                ModelState.AddModelError(string.Empty, "Notice Title and Description are required.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _noticeRepo.GetCategoriesAsync();
                return View(notice);
            }

            if (attachmentFile != null)
            {
                notice.AttachmentUrl = await _fileService.SaveFileAsync(attachmentFile, "notices");
                notice.AttachmentFileName = attachmentFile.FileName;
            }

            await _noticeRepo.CreateNoticeAsync(notice);
            TempData["SuccessMessage"] = $"Notice '{notice.Title}' published successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var notice = await _noticeRepo.GetNoticeByIdAsync(id);
            if (notice == null) return NotFound();

            ViewBag.Categories = await _noticeRepo.GetCategoriesAsync();
            return View(notice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Notice notice, IFormFile? attachmentFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _noticeRepo.GetCategoriesAsync();
                return View(notice);
            }

            if (attachmentFile != null)
            {
                notice.AttachmentUrl = await _fileService.SaveFileAsync(attachmentFile, "notices");
                notice.AttachmentFileName = attachmentFile.FileName;
            }

            await _noticeRepo.UpdateNoticeAsync(notice);
            TempData["SuccessMessage"] = "Notice updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _noticeRepo.DeleteNoticeAsync(id);
            TempData["SuccessMessage"] = "Notice deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
