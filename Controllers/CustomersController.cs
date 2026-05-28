using DeliveryAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryAdmin.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly ApiService _api;
        public CustomersController(ApiService api) => _api = api;

        public async Task<IActionResult> Index(string? role, int page = 1)
        {
            var result = await _api.GetUsers(page, 20, role);
            ViewBag.Role = role; ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling((result?.Total ?? 0) / 20.0);
            ViewBag.Total = result?.Total ?? 0;
            return View(result?.Data ?? new());
        }
    }
}
