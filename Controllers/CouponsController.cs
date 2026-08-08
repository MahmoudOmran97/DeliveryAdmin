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

        // GET /Coupons/SearchCustomers?q=... — بحث حي بالاسم/الإيميل/رقم التليفون.
        // لو q فاضي، بيرجّع أحدث 20 عميل (أحدث المسجلين الأول) عشان تقدر تشوف
        // العملاء الجداد بسهولة من غير ما تعرف اسمهم بالظبط.
        [HttpGet]
        public async Task<IActionResult> SearchCustomers(string? q)
        {
            var search = string.IsNullOrWhiteSpace(q) ? null : q.Trim();
            if (search != null && search.Length < 2)
                return Json(new List<object>());

            var result = await _api.GetUsers(1, 20, role: "Customer", search: search);
            var list = (result?.Data ?? new()).Select(u => new
            {
                id = u.Id,
                fullName = u.FullName,
                phone = u.Phone,
                email = u.Email
            });
            return Json(list);
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

            // لو الكوبون خاص بعميل معين، هنجيب بياناته عشان نعرضها جاهزة في حقل البحث
            if (c.OwnerUserId.HasValue)
            {
                var owner = await _api.GetUser(c.OwnerUserId.Value);
                ViewBag.OwnerCustomer = owner;
            }

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
                OwnerUserId = c.OwnerUserId,
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
