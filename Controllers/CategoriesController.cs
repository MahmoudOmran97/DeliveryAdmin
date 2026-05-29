using DeliveryAdmin.Models;
using DeliveryAdmin.Resources;
using DeliveryAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DeliveryAdmin.Controllers
{
    [Authorize]
    public class CategoriesController : LocalizedController
    {
        private readonly ApiService _api;
        public CategoriesController(ApiService api, IStringLocalizer<SharedResource> localizer) : base(localizer) => _api = api;

        public async Task<IActionResult> Index(int restaurantId, string? search)
        {
            if (restaurantId == 0)
                return RedirectToAction("Index", "Restaurants");

            var allCats = await _api.GetCategories(restaurantId) ?? new();

            // Apply search filter client-side (categories are small lists)
            if (!string.IsNullOrWhiteSpace(search))
                allCats = allCats.Where(c => c.Name.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            var rest = await _api.GetRestaurant(restaurantId);
            if (rest == null) return NotFound();

            SetTitle("Nav_Categories");
            ViewBag.Restaurant = rest;
            ViewBag.RestaurantId = restaurantId;
            ViewBag.Search = search;
            ViewBag.Total = allCats.Count;
            return View(allCats);
        }

        public async Task<IActionResult> Create(int restaurantId)
        {
            var rest = await _api.GetRestaurant(restaurantId);
            SetTitle("Cat_Add");
            ViewBag.Restaurant = rest;
            ViewBag.RestaurantId = restaurantId;
            return View(new CreateCategoryDto { RestaurantId = restaurantId });
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryDto dto)
        {
            var (ok, error) = await _api.CreateCategory(dto);
            if (!ok) { TempData["Error"] = error; return RedirectToAction("Create", new { restaurantId = dto.RestaurantId }); }
            TempData["Success"] = L["Cat_Created"].Value;
            return RedirectToAction("Index", new { restaurantId = dto.RestaurantId });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var c = await _api.GetCategory(id);
            if (c == null) return NotFound();
            SetTitle("Cat_Edit");
            ViewBag.CategoryId = id;
            ViewBag.RestaurantId = c.RestaurantId;
            ViewBag.CategoryName = c.Name;
            return View(new UpdateCategoryDto { Name = c.Name, ImageUrl = c.ImageUrl, SortOrder = c.SortOrder });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, UpdateCategoryDto dto, int restaurantId)
        {
            var (ok, error) = await _api.UpdateCategory(id, dto);
            if (!ok) { TempData["Error"] = error; return RedirectToAction("Edit", new { id }); }
            TempData["Success"] = L["Cat_Updated"].Value;
            return RedirectToAction("Index", new { restaurantId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, int restaurantId)
        {
            await _api.DeleteCategory(id);
            TempData["Success"] = L["Cat_Deleted"].Value;
            return RedirectToAction("Index", new { restaurantId });
        }
    }
}
