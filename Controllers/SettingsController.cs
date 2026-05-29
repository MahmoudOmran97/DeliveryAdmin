using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryAdmin.Controllers;

public class SettingsController : Controller
{
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public IActionResult SetLanguage(string culture, string returnUrl = "/")
    {
        if (culture is not ("en" or "ar")) culture = "en";
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });
        return LocalRedirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
    }
}
