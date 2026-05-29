using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryAdmin.Controllers;

[Authorize]
public class SettingsController : Controller
{
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
}