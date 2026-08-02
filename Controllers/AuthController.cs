using System.Security.Claims;
using DeliveryAdmin.Models;
using DeliveryAdmin.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

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
                if (User.IsInRole("Restaurant")) return RedirectToAction("Index", "Pharmacy");
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
            
            HttpContext.Session.SetString("JWT", data.Token);
            HttpContext.Session.SetString("UserName", data.FullName ?? "Admin");
            HttpContext.Session.SetString("UserRole", data.Role ?? "Admin");

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, data.FullName ?? ""),
                new(ClaimTypes.Email, data.Email ?? ""),
                new(ClaimTypes.Role, data.Role ?? ""),
                new("UserId", data.Id.ToString())
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            // ✅ صاحب الصيدلية/المطعم (Role=Restaurant) بيروح على بورتاله المبسط بس،
            // مش الداشبورد العام اللي فيه بيانات كل المنصة.
            if (string.Equals(data.Role, "Restaurant", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Index", "Pharmacy");

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
