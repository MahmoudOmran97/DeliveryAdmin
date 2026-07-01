using DeliveryAdmin.Models;
using DeliveryAdmin.Resources;
using DeliveryAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DeliveryAdmin.Controllers
{
    [Authorize]
    public class DealsController : LocalizedController
    {
        private readonly ApiService _api;
        public DealsController(ApiService api, IStringLocalizer<SharedResource> localizer) : base(localizer) => _api = api;

        public async Task<IActionResult> Index()
        {
            SetTitle("Deals_Title");
            var deals = await _api.GetDeals() ?? new();
            return View(deals.OrderByDescending(d => d.CreatedAt).ToList());
        }

        public async Task<IActionResult> Create(int? restaurantId)
        {
            var rests = await _api.GetRestaurants(1, 100);
            ViewBag.Restaurants = rests?.Data ?? new();
            ViewBag.RestaurantId = restaurantId;
            if (restaurantId.HasValue)
            {
                var products = await _api.SearchProducts("", restaurantId.Value, 1, 200);
                ViewBag.Products = products?.Data ?? new();
            }
            else ViewBag.Products = new List<ProductDto>();
            return View(new CreateDealDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateDealDto dto)
        {
            var (ok, error) = await _api.CreateDeal(dto);
            if (!ok) { TempData["Error"] = error; return RedirectToAction("Create"); }
            TempData["Success"] = "Deal created!";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var deals = await _api.GetDeals() ?? new();
            var d = deals.FirstOrDefault(x => x.Id == id);
            if (d == null) return NotFound();

            var rests = await _api.GetRestaurants(1, 100);
            ViewBag.Restaurants = rests?.Data ?? new();
            ViewBag.DealId = id;
            ViewBag.RestaurantId = d.RestaurantId;
            if (d.RestaurantId.HasValue)
            {
                var products = await _api.SearchProducts("", d.RestaurantId.Value, 1, 200);
                ViewBag.Products = products?.Data ?? new();
            }
            else ViewBag.Products = new List<ProductDto>();

            var dto = new CreateDealDto
            {
                Title = d.Title,
                Description = d.Description,
                ImageUrl = d.ImageUrl,
                RestaurantId = d.RestaurantId,
                ProductId = d.ProductId,
                OriginalPrice = d.OriginalPrice,
                DiscountedPrice = d.DiscountedPrice,
                DiscountPercent = d.DiscountPercent,
                BadgeText = d.BadgeText,
                BadgeColor = d.BadgeColor,
                IsActive = d.IsActive,
                SortOrder = d.SortOrder,
                ExpiresAt = d.ExpiresAt
            };
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, CreateDealDto dto)
        {
            var (ok, error) = await _api.UpdateDeal(id, dto);
            if (!ok) { TempData["Error"] = error; return RedirectToAction("Edit", new { id }); }
            TempData["Success"] = "Deal updated!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _api.DeleteDeal(id);
            TempData["Success"] = "Deal deleted!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> GetProductsJson(int restaurantId)
        {
            var products = await _api.SearchProducts("", restaurantId, 1, 200);
            return Json(products?.Data ?? new());
        }
    }
}
