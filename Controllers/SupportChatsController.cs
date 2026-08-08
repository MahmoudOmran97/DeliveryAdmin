using DeliveryAdmin.Resources;
using DeliveryAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DeliveryAdmin.Controllers
{
    // ─────────────────────────────────────────────────────────────────────────
    // شاشة الأدمن لشاتات الدعم اللي المساعد الذكي حولها لتدخل بشري (Escalated).
    // بتفتح مباشرة من إشعار "شات دعم محتاج تدخل أدمن" (ActionUrl = supportchat/{id}).
    // ─────────────────────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin")]
    public class SupportChatsController : LocalizedController
    {
        private readonly ApiService _api;
        public SupportChatsController(ApiService api, IStringLocalizer<SharedResource> localizer) : base(localizer) => _api = api;

        public async Task<IActionResult> Index(string? status = "Escalated", int page = 1)
        {
            SetTitle("SupportChats_Title");
            var result = await _api.GetSupportSessionsAdmin(status, page, 20) ?? new();
            ViewBag.CurrentStatus = status;
            return View(result);
        }

        public async Task<IActionResult> Chat(int id)
        {
            SetTitle("SupportChats_Title");
            var session = await _api.GetSupportSessionById(id);
            if (session == null) return NotFound();
            return View(session);
        }

        // ── AJAX: بولينج للرسائل الجديدة (زي شات الروشتة بالظبط) ──────────
        [HttpGet]
        public async Task<IActionResult> Messages(int id)
        {
            var session = await _api.GetSupportSessionById(id);
            if (session == null) return NotFound();
            return Json(session.Messages);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(int id, string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return BadRequest(new { message = "اكتب رسالة" });
            var (ok, error) = await _api.SendSupportAdminReply(id, message.Trim());
            if (!ok) return BadRequest(new { message = error ?? "فشل الإرسال" });
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Close(int id)
        {
            var (ok, error) = await _api.CloseSupportSession(id);
            if (!ok) return BadRequest(new { message = error ?? "فشل إغلاق الشات" });
            return Ok();
        }
    }
}
