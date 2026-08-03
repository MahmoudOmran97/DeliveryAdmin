using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryAdmin.Controllers;

[Authorize]
public class SettingsController : Controller
{
    private readonly IConfiguration _config;
    public SettingsController(IConfiguration config) => _config = config;

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public IActionResult SetLanguage(string culture, string returnUrl = "/")
    {
        // ✅ التحقق من صحة الثقافة المطلوبة
        if (culture is not ("en" or "ar")) culture = "en";

        // ✅ حفظ الـ cookie بشكل صحيح
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                SameSite = SameSiteMode.Lax  // ✅ مهم لضمان إرسال الـ cookie مع الـ redirect
            });

        // ✅ التأكد إن الـ returnUrl آمن ومش فارغ
        if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
            returnUrl = "/";

        return LocalRedirect(returnUrl);
    }

    // GET /firebase-config.js — بيتقرا من غير أوثنتيكيشن لإن السيرفس ووركر
    // (firebase-messaging-sw.js) بيعمله import مباشر وممكن يتنفذ من غير
    // كوكي سيشن. القيم دي عامة/عمومية بطبيعتها في فايربيز، مفيش فيها سر.
    [AllowAnonymous]
    [HttpGet("/firebase-config.js")]
    public IActionResult FirebaseConfig()
    {
        var apiKey = _config["FirebaseWeb:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return Content("self.firebaseConfig = null;", "text/javascript");

        var json = System.Text.Json.JsonSerializer.Serialize(new
        {
            apiKey,
            authDomain = _config["FirebaseWeb:AuthDomain"],
            projectId = _config["FirebaseWeb:ProjectId"],
            storageBucket = _config["FirebaseWeb:StorageBucket"],
            messagingSenderId = _config["FirebaseWeb:MessagingSenderId"],
            appId = _config["FirebaseWeb:AppId"]
        });

        return Content($"self.firebaseConfig = {json};", "text/javascript");
    }
}