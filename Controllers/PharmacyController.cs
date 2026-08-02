using DeliveryAdmin.Models;
using DeliveryAdmin.Resources;
using DeliveryAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DeliveryAdmin.Controllers
{
    // ─────────────────────────────────────────────────────────────────────────
    // بورتال مبسط لصاحب الصيدلية (Role = "Restaurant"): يشوف طلبات الروشتة
    // اللي جاله، يشات مع العميل، ويحدد تمن الفاتورة. ما بيشوفش أي حاجة تانية
    // من الأدمن العام (طلبات كل المنصة، مستخدمين، إلخ).
    // ─────────────────────────────────────────────────────────────────────────
    [Authorize(Roles = "Restaurant")]
    public class PharmacyController : LocalizedController
    {
        private readonly ApiService _api;
        public PharmacyController(ApiService api, IStringLocalizer<SharedResource> localizer) : base(localizer) => _api = api;

        // قائمة طلبات الروشتة بتاعة الصيدلية
        public async Task<IActionResult> Index()
        {
            SetTitle("Pharmacy_Title");
            var list = await _api.GetMyPrescriptionRequests() ?? new();
            return View(list.OrderByDescending(r => r.CreatedAt).ToList());
        }

        // شاشة الشات + تحديد السعر لطلب معين
        public async Task<IActionResult> Chat(int id)
        {
            SetTitle("Pharmacy_Chat_Title");
            var requests = await _api.GetMyPrescriptionRequests() ?? new();
            var req = requests.FirstOrDefault(r => r.Id == id);
            if (req == null) return NotFound();

            ViewBag.Messages = await _api.GetPrescriptionMessages(id) ?? new();
            return View(req);
        }

        // ── AJAX: بولينج للرسائل الجديدة (المكتب مفيهوش SignalR client) ──────
        [HttpGet]
        public async Task<IActionResult> Messages(int id)
        {
            var messages = await _api.GetPrescriptionMessages(id) ?? new();
            return Json(messages);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(int id, string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return BadRequest(new { message = "اكتب رسالة" });
            var (ok, error) = await _api.SendPrescriptionMessage(id, message.Trim());
            if (!ok) return BadRequest(new { message = error ?? "فشل الإرسال" });
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SetPrice(int id, decimal price)
        {
            if (price <= 0) return BadRequest(new { message = "السعر لازم يكون أكبر من صفر" });
            var (ok, error) = await _api.SetPrescriptionPrice(id, price);
            if (!ok) return BadRequest(new { message = error ?? "فشل تحديد السعر" });
            return Ok();
        }
    }
}
