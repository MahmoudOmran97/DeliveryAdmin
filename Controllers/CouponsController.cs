using DeliveryAdmin.Models;
using DeliveryAdmin.Resources;
using DeliveryAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DeliveryAdmin.Controllers
{
    [Authorize]
    public class CouponsController : LocalizedController
    {
        private readonly ApiService _api;
        public CouponsController(ApiService api, IStringLocalizer<SharedResource> localizer) : base(localizer) => _api = api;

        public async Task<IActionResult> Index()
        {
            SetTitle("Coupons_Title");
            var coupons = await _api.GetCoupons() ?? new();
            return View(coupons.OrderByDescending(c => c.CreatedAt).ToList());
        }

        public async Task<IActionResult> Create()
        {
            var rests = await _api.GetRestaurants(1, 100);
            ViewBag.Restaurants = rests?.Data ?? new();
            return View(new CreateCouponDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCouponDto dto)
        {
            var (ok, error) = await _api.CreateCoupon(dto);
            if (!ok) { TempData["Error"] = error; return RedirectToAction("Create"); }
            TempData["Success"] = "Coupon created!";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var coupons = await _api.GetCoupons() ?? new();
            var c = coupons.FirstOrDefault(x => x.Id == id);
            if (c == null) return NotFound();

            var rests = await _api.GetRestaurants(1, 100);
            ViewBag.Restaurants = rests?.Data ?? new();
            ViewBag.CouponId = id;

            var dto = new CreateCouponDto
            {
                Code = c.Code,
                Title = c.Title,
                Description = c.Description,
                DiscountType = c.DiscountType,
                DiscountValue = c.DiscountValue,
                MinOrderAmount = c.MinOrderAmount,
                MaxDiscount = c.MaxDiscount,
                RestaurantId = c.RestaurantId,
                UsageLimit = c.UsageLimit,
                IsActive = c.IsActive,
                ExpiresAt = c.ExpiresAt
            };
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, CreateCouponDto dto)
        {
            var (ok, error) = await _api.UpdateCoupon(id, dto);
            if (!ok) { TempData["Error"] = error; return RedirectToAction("Edit", new { id }); }
            TempData["Success"] = "Coupon updated!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _api.DeleteCoupon(id);
            TempData["Success"] = "Coupon deleted!";
            return RedirectToAction("Index");
        }
    }
}
