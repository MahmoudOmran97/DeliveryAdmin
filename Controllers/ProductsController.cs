using DeliveryAdmin.Models;
using DeliveryAdmin.Resources;
using DeliveryAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DeliveryAdmin.Controllers
{
    [Authorize]
    public class ProductsController : LocalizedController
    {
        private readonly ApiService _api;
        public ProductsController(ApiService api, IStringLocalizer<SharedResource> localizer) : base(localizer) => _api = api;

        public async Task<IActionResult> Index(string? q, int? restaurantId, int? categoryId, bool? isAvailable, int page = 1)
        {
            SetTitle("Products_Title");
            var result = await _api.SearchProducts(q ?? "", restaurantId, page, 20, categoryId, isAvailable);
            var rests = await _api.GetRestaurants(1, 100);
            ViewBag.Restaurants = rests?.Data ?? new();
            ViewBag.Q = q;
            ViewBag.RestaurantId = restaurantId;
            ViewBag.CategoryId = categoryId;
            ViewBag.IsAvailable = isAvailable;

            // Load categories for selected restaurant
            if (restaurantId.HasValue)
                ViewBag.Categories = await _api.GetCategories(restaurantId.Value) ?? new();
            else
                ViewBag.Categories = new List<DeliveryAdmin.Models.CategoryDto>();

            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling((result?.Total ?? 0) / 20.0);
            ViewBag.Total = result?.Total ?? 0;
            return View(result?.Data ?? new());
        }

        public async Task<IActionResult> Create(int? restaurantId)
        {
            var rests = await _api.GetRestaurants(1, 100);
            ViewBag.Restaurants = rests?.Data ?? new();
            ViewBag.RestaurantId = restaurantId;
            if (restaurantId.HasValue) ViewBag.Categories = await _api.GetCategories(restaurantId.Value) ?? new();
            return View(new CreateProductDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductDto dto, string? variantsJson)
        {
            var (ok, error, newId) = await _api.CreateProductWithId(dto);
            if (!ok || newId == null) { TempData["Error"] = error; return RedirectToAction("Create"); }

            var variants = ParseVariants(variantsJson);
            if (variants.Any())
            {
                var (vOk, vErr) = await _api.SetProductVariants(newId.Value, variants);
                if (!vOk) TempData["Error"] = "Product saved, but variants failed: " + vErr;
            }

            TempData["Success"] = "Product created!";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var p = await _api.GetProduct(id);
            if (p == null) return NotFound();
            var rests = await _api.GetRestaurants(1, 100);
            ViewBag.Restaurants = rests?.Data ?? new();
            ViewBag.ProductId = id; ViewBag.ProductName = p.Name;
            ViewBag.Variants = p.Variants ?? new List<ProductVariantDto>();

            // Fix: load categories for the product's restaurant so the dropdown is populated
            if (p.RestaurantId.HasValue)
            {
                ViewBag.Categories = await _api.GetCategories(p.RestaurantId.Value) ?? new();
                ViewBag.RestaurantId = p.RestaurantId.Value;
            }

            var dto = new CreateProductDto { CategoryId = p.Category is System.Text.Json.JsonElement cat && cat.TryGetProperty("id", out var cid) ? cid.GetInt32() : 0, Name = p.Name, Description = p.Description, Price = p.Price, DiscountedPrice = p.DiscountedPrice, ImageUrl = p.ImageUrl, PreparationTime = p.PreparationTime, Calories = p.Calories };
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, CreateProductDto dto, string? variantsJson)
        {
            var (ok, error) = await _api.UpdateProduct(id, dto);
            if (!ok) { TempData["Error"] = error; return RedirectToAction("Edit", new { id }); }

            var variants = ParseVariants(variantsJson);
            var (vOk, vErr) = await _api.SetProductVariants(id, variants);
            if (!vOk) TempData["Error"] = "Product saved, but variants failed: " + vErr;
            else TempData["Success"] = "Product updated!";

            return RedirectToAction("Index");
        }

        private static List<ProductVariantDto> ParseVariants(string? variantsJson)
        {
            if (string.IsNullOrWhiteSpace(variantsJson)) return new();
            try
            {
                var list = System.Text.Json.JsonSerializer.Deserialize<List<ProductVariantDto>>(
                    variantsJson,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return (list ?? new())
                    .Where(v => !string.IsNullOrWhiteSpace(v.Name))
                    .ToList();
            }
            catch { return new(); }
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int id)
        {
            await _api.ToggleProduct(id);
            TempData["Success"] = "Product availability toggled!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _api.DeleteProduct(id);
            TempData["Success"] = "Product deleted!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoriesJson(int restaurantId)
        {
            var cats = await _api.GetCategories(restaurantId);
            return Json(cats ?? new());
        }
    }
}