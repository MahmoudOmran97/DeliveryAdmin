using DeliveryAdmin.Resources;
using DeliveryAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DeliveryAdmin.Controllers
{
    [Authorize]
    public class RatingsController : LocalizedController
    {
        private readonly ApiService _api;
        public RatingsController(ApiService api, IStringLocalizer<SharedResource> localizer) : base(localizer) => _api = api;

        public async Task<IActionResult> Index(int page = 1)
        {
            SetTitle("Ratings_Title");
            var result = await _api.GetRatings(page, 20);
            ViewBag.Page = page; ViewBag.TotalPages = (int)Math.Ceiling((result?.Total ?? 0) / 20.0);
            return View(result?.Data ?? new());
        }
    }
}
