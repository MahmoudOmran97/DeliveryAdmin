using DeliveryAdmin.Models;
using DeliveryAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryAdmin.Controllers
{
    [Authorize]
    public class DeliverySettingsController : Controller
    {
        private readonly ApiService _api;
        public DeliverySettingsController(ApiService api) => _api = api;

        public async Task<IActionResult> Index()
        {
            var settings = await _api.GetDeliverySettings() ?? new DeliverySettingsDto { FreeRadiusKm = 3.0, ExtraFeePerKm = 10m };

            var dto = new UpdateDeliverySettingsDto
            {
                FreeRadiusKm = settings.FreeRadiusKm,
                ExtraFeePerKm = settings.ExtraFeePerKm
            };
            ViewBag.UpdatedAt = settings.UpdatedAt;
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Index(UpdateDeliverySettingsDto dto)
        {
            if (dto.FreeRadiusKm < 0 || dto.ExtraFeePerKm < 0)
            {
                TempData["Error"] = "القيم لازم تكون صفر أو أكبر";
                return RedirectToAction("Index");
            }

            var (ok, error) = await _api.UpdateDeliverySettings(dto);
            if (!ok) { TempData["Error"] = error; return RedirectToAction("Index"); }

            TempData["Success"] = "تم تحديث إعدادات سعر التوصيل بنجاح";
            return RedirectToAction("Index");
        }
    }
}
