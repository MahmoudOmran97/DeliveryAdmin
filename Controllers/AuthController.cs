using System.Security.Claims;
using DeliveryAdmin.Models;
using DeliveryAdmin.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
// AuthenticationProperties موجودة في Microsoft.AspNetCore.Authentication (مضافة فوق)

namespace DeliveryAdmin.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApiService _api;
        public AuthController(ApiService api) => _api = api;

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Restaurant")) return RedirectToAction("Index", "MyStore");
                return RedirectToAction("Index", "Dashboard");
            }
            ViewData["Title"] = "Login";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            var (ok, error, data) = await _api.Login(dto);
            if (!ok || data?.Token == null) { ViewBag.Error = error ?? "Login failed"; return View(dto); }

            // ✅ لوحة التحكم دي مخصصة للأدمن وصاحب المحل (Restaurant) بس.
            // الدرايفر والكاستمر بتوعهم الموبايل أبس، مش مفروض يدخلوا هنا خالص.
            if (!string.Equals(data.Role, "Admin", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(data.Role, "Restaurant", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Error = "غير مسموح لك بالدخول إلى لوحة التحكم من هنا.";
                return View(dto);
            }

            // ✅ FIX: التوكن كان بيتخزن في Session (in-memory) بس.
            // على أي hosting شير بيحصل فيه App Pool recycle (بيحصل عادي جدًا كل شوية،
            // خصوصًا مع idle timeout)، الـ Session بتتمسح فورًا حتى لو الـ auth cookie
            // نفسها لسه صالحة 7 أيام → أول طلب API بيتبعت من غير توكن → 401 →
            // تسجيل خروج فوري تلقائي. الحل: نخزّن التوكن كـ claim جوا الـ cookie نفسها
            // (مشفرة بالـ Data Protection) عشان تفضل موجودة لحد ما الكوكي تنتهي فعليًا.
            HttpContext.Session.SetString("UserName", data.FullName ?? "Admin");
            HttpContext.Session.SetString("UserRole", data.Role ?? "Admin");

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, data.FullName ?? ""),
                new(ClaimTypes.Email, data.Email ?? ""),
                new(ClaimTypes.Role, data.Role ?? ""),
                new("UserId", data.Id.ToString()),
                new("JWT", data.Token)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // ✅ IsPersistent = true عشان الكوكي تتخزن فعليًا على الجهاز (زي "تذكرني")
            // وتفضل شغالة حتى لو المستخدم قفل المتصفح، لحد ما الـ ExpireTimeSpan (7 أيام) تخلص.
            var authProps = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), authProps);

            // ✅ صاحب المحل (صيدلية/مطعم/محل — Role=Restaurant) بيروح على بورتاله
            // الخاص بيه بس (MyStore)، مش الداشبورد العام اللي فيه بيانات كل المنصة.
            // وجوه MyStore، لو محله صيدلية بيتوجه لتبويب شات الروشتة (Pharmacy).
            if (string.Equals(data.Role, "Restaurant", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Index", "MyStore");

            return RedirectToAction("Index", "Dashboard");
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
