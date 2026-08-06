using DeliveryAdmin.Resources;
using DeliveryAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DeliveryAdmin.Controllers
{
    // ─────────────────────────────────────────────────────────────────────────
    // مراجعة الأدمن العام لكل شاتات الروشتة على مستوى المنصة (كل الصيدليات)
    // للمتابعة/الإشراف فقط — مش بورتال صاحب الصيدلية (ده Pharmacy Controller
    // المخصص لـ Role=Restaurant).
    // ─────────────────────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin")]
    public class PharmacyChatsController : LocalizedController
    {
        private readonly ApiService _api;
        public PharmacyChatsController(ApiService api, IStringLocalizer<SharedResource> localizer) : base(localizer) => _api = api;

        public async Task<IActionResult> Index(string? status, int page = 1)
        {
            SetTitle("PharmacyChats_Title");
            var result = await _api.GetAllPrescriptionRequestsAdmin(status, page, 20);
            ViewBag.Status = status;
            ViewBag.Page = page;
            ViewBag.Total = result?.Total ?? 0;
            ViewBag.TotalPages = (int)Math.Ceiling((result?.Total ?? 0) / 20.0);
            return View(result?.Items ?? new());
        }

        // شاشة عرض الشات (Read-only للأدمن — بيراجع بس، مش طرف في المحادثة)
        public async Task<IActionResult> Chat(int id)
        {
            SetTitle("PharmacyChats_ChatTitle");
            var req = await _api.GetPrescriptionRequestById(id);
            if (req == null) return NotFound();

            ViewBag.Messages = await _api.GetPrescriptionMessages(id) ?? new();
            return View(req);
        }

        // ── AJAX: بولينج للرسائل الجديدة ──
        [HttpGet]
        public async Task<IActionResult> Messages(int id)
        {
            var messages = await _api.GetPrescriptionMessages(id) ?? new();
            return Json(messages);
        }
    }
}
