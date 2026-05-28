using DeliveryAdmin.Models;
using DeliveryAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryAdmin.Controllers
{
    [Authorize]
    public class RestaurantsController : Controller
    {
        private readonly ApiService _api;
        public RestaurantsController(ApiService api) => _api = api;

        public async Task<IActionResult> Index(string? search, bool? isOpen, int page = 1)
        {
            var result = await _api.GetRestaurants(page, 12, search, isOpen);
            ViewBag.Search = search; ViewBag.IsOpen = isOpen;
            ViewBag.Page = page; ViewBag.TotalPages = (int)Math.Ceiling((result?.Total ?? 0) / 12.0);
            ViewBag.Total = result?.Total ?? 0;
            return View(result?.Data ?? new());
        }

        public async Task<IActionResult> Details(int id)
        {
            var rest = await _api.GetRestaurant(id);
            if (rest == null) return NotFound();
            ViewBag.Categories = await _api.GetCategories(id) ?? new();
            return View(rest);
        }

        public IActionResult Create() => View(new CreateRestaurantDto());

        [HttpPost]
        public async Task<IActionResult> Create(CreateRestaurantDto dto)
        {
            var (ok, error) = await _api.CreateRestaurant(dto);
            if (!ok) { TempData["Error"] = error; return View(dto); }
            TempData["Success"] = "Restaurant created successfully!";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var r = await _api.GetRestaurant(id);
            if (r == null) return NotFound();
            var dto = new UpdateRestaurantDto
            {
                Name=r.Name, Description=r.Description, Address=r.Address, Phone=r.Phone,
                Latitude=r.Latitude, Longitude=r.Longitude, DeliveryFee=r.DeliveryFee,
                MinOrderAmount=r.MinOrderAmount, EstimatedTime=r.EstimatedTime,
                ImageUrl=r.ImageUrl, CoverImageUrl=r.CoverImageUrl, IsOpen=r.IsOpen
            };
            ViewBag.RestaurantId = id; ViewBag.RestaurantName = r.Name;
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, UpdateRestaurantDto dto)
        {
            var (ok, error) = await _api.UpdateRestaurant(id, dto);
            if (!ok) { TempData["Error"] = error; ViewBag.RestaurantId = id; return View(dto); }
            TempData["Success"] = "Restaurant updated!";
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int id)
        {
            await _api.ToggleRestaurant(id);
            return RedirectToAction("Index");
        }
    }
}
