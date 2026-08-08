using DeliveryAdmin.Models;
using DeliveryAdmin.Resources;
using DeliveryAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DeliveryAdmin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ComplaintsController : LocalizedController
    {
        private readonly ApiService _api;
        public ComplaintsController(ApiService api, IStringLocalizer<SharedResource> localizer) : base(localizer) => _api = api;

        // قائمة كل الشكاوى — يدوية من العميل أو تلقائية من المساعد الذكي
        public async Task<IActionResult> Index(string? status, int page = 1)
        {
            SetTitle("Complaints_Title");
            var result = await _api.GetComplaintsAdmin(status, page, 20) ?? new ComplaintsAdminResult();
            ViewBag.CurrentStatus = status;
            return View(result);
        }

        public async Task<IActionResult> Details(int id)
        {
            SetTitle("Complaints_Description");
            var complaint = await _api.GetComplaintById(id);
            if (complaint == null) return NotFound();
            return View(complaint);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status, string? adminNote)
        {
            var (ok, error) = await _api.UpdateComplaintStatus(id, new UpdateComplaintStatusRequest
            {
                Status = status,
                AdminNote = adminNote
            });

            TempData[ok ? "Success" : "Error"] = ok ? "تم تحديث حالة الشكوى" : (error ?? "حصل خطأ أثناء التحديث");
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
