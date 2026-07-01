using DeliveryAdmin.Models;
using DeliveryAdmin.Resources;
using DeliveryAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DeliveryAdmin.Controllers
{
    [Authorize]
    public class BannersController : LocalizedController
    {
        private readonly ApiService _api;
        public BannersController(ApiService api, IStringLocalizer<SharedResource> localizer) : base(localizer) => _api = api;

        public async Task<IActionResult> Index()
        {
            SetTitle("Banners_Title");
            var banners = await _api.GetBanners() ?? new();
            return View(banners.OrderBy(b => b.SortOrder).ToList());
        }

        public IActionResult Create() => View(new CreateBannerDto());

        [HttpPost]
        public async Task<IActionResult> Create(CreateBannerDto dto)
        {
            var (ok, error) = await _api.CreateBanner(dto);
            if (!ok) { TempData["Error"] = error; return RedirectToAction("Create"); }
            TempData["Success"] = "Banner created!";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var banners = await _api.GetBanners() ?? new();
            var b = banners.FirstOrDefault(x => x.Id == id);
            if (b == null) return NotFound();

            ViewBag.BannerId = id;
            var dto = new CreateBannerDto
            {
                Title = b.Title,
                SubTitle = b.SubTitle,
                ImageUrl = b.ImageUrl,
                ActionUrl = b.ActionUrl,
                BackgroundColor = b.BackgroundColor,
                SortOrder = b.SortOrder,
                IsActive = b.IsActive,
                StartsAt = b.StartsAt,
                EndsAt = b.EndsAt
            };
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, CreateBannerDto dto)
        {
            var (ok, error) = await _api.UpdateBanner(id, dto);
            if (!ok) { TempData["Error"] = error; return RedirectToAction("Edit", new { id }); }
            TempData["Success"] = "Banner updated!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _api.DeleteBanner(id);
            TempData["Success"] = "Banner deleted!";
            return RedirectToAction("Index");
        }
    }
}
