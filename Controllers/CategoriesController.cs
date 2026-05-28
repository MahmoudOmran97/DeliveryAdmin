using DeliveryAdmin.Models;
using DeliveryAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryAdmin.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly ApiService _api;
        public CategoriesController(ApiService api) => _api = api;

        public async Task<IActionResult> Index(int restaurantId)
        {
            var cats = await _api.GetCategories(restaurantId) ?? new();
            var rest = await _api.GetRestaurant(restaurantId);
            ViewBag.Restaurant = rest;
            ViewBag.RestaurantId = restaurantId;
            return View(cats);
        }

        public async Task<IActionResult> Create(int restaurantId)
        {
            var rest = await _api.GetRestaurant(restaurantId);
            ViewBag.Restaurant = rest; ViewBag.RestaurantId = restaurantId;
            return View(new CreateCategoryDto { RestaurantId = restaurantId });
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryDto dto)
        {
            var (ok, error) = await _api.CreateCategory(dto);
            if (!ok) { TempData["Error"] = error; return RedirectToAction("Create", new { restaurantId = dto.RestaurantId }); }
            TempData["Success"] = "Category created!";
            return RedirectToAction("Index", new { restaurantId = dto.RestaurantId });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var c = await _api.GetCategory(id);
            if (c == null) return NotFound();
            ViewBag.CategoryId = id; ViewBag.RestaurantId = c.RestaurantId;
            return View(new UpdateCategoryDto { Name=c.Name, ImageUrl=c.ImageUrl, SortOrder=c.SortOrder });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, UpdateCategoryDto dto, int restaurantId)
        {
            var (ok, error) = await _api.UpdateCategory(id, dto);
            if (!ok) { TempData["Error"] = error; return RedirectToAction("Edit", new { id }); }
            TempData["Success"] = "Category updated!";
            return RedirectToAction("Index", new { restaurantId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, int restaurantId)
        {
            await _api.DeleteCategory(id);
            TempData["Success"] = "Category deleted!";
            return RedirectToAction("Index", new { restaurantId });
        }
    }
}
