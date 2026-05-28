using DeliveryAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryAdmin.Controllers
{
    [Authorize]
    public class RatingsController : Controller
    {
        private readonly ApiService _api;
        public RatingsController(ApiService api) => _api = api;

        public async Task<IActionResult> Index(int page = 1)
        {
            var result = await _api.GetRatings(page, 20);
            ViewBag.Page = page; ViewBag.TotalPages = (int)Math.Ceiling((result?.Total ?? 0) / 20.0);
            return View(result?.Data ?? new());
        }
    }
}
