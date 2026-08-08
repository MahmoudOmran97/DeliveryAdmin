using DeliveryAdmin.Models;
using DeliveryAdmin.Resources;
using DeliveryAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DeliveryAdmin.Controllers
{
    // شاشة الأدمن الوحيدة اللي بتتحكم في مفتاح/موديل/برومبت الـ AI بتاع شات الدعم
    [Authorize(Roles = "Admin")]
    public class AiSettingsController : LocalizedController
    {
        private readonly ApiService _api;
        public AiSettingsController(ApiService api, IStringLocalizer<SharedResource> localizer) : base(localizer) => _api = api;

        public async Task<IActionResult> Index()
        {
            SetTitle("AiSettings_Title");
            var settings = await _api.GetAiSettings();
            return View(settings ?? new AiSettingsDto());
        }

        [HttpPost]
        public async Task<IActionResult> Save(bool isEnabled, string? apiKey, string model, string? systemPrompt, int maxTokens)
        {
            var (ok, error) = await _api.UpdateAiSettings(new UpdateAiSettingsRequest
            {
                IsEnabled = isEnabled,
                ApiKey = apiKey,
                Model = model,
                SystemPrompt = systemPrompt,
                MaxTokens = maxTokens
            });

            TempData[ok ? "Success" : "Error"] = ok ? "تم حفظ إعدادات الـ AI بنجاح" : (error ?? "حصل خطأ أثناء الحفظ");
            return RedirectToAction(nameof(Index));
        }
    }
}
