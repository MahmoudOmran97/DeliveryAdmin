using DeliveryAdmin.Models;
using DeliveryAdmin.Resources;
using DeliveryAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DeliveryAdmin.Controllers
{
    // ─────────────────────────────────────────────────────────────────────────
    // بورتال صاحب المحل (Role = "Restaurant") — بيشتغل لأي نوع محل: صيدلية،
    // مطعم، أو أي StoreType تاني. كل Action بيجيب المحل بتاع صاحب الحساب
    // الحالي من التوكين نفسه (GET /api/restaurants/me) — مش بياخد Id من الـ
    // query ولا من اليوزر، فمينفعش صاحب محل يشوف/يعدل بيانات محل غيره.
    // ─────────────────────────────────────────────────────────────────────────
    [Authorize(Roles = "Restaurant")]
    public class MyStoreController : LocalizedController
    {
        private readonly ApiService _api;
        public MyStoreController(ApiService api, IStringLocalizer<SharedResource> localizer) : base(localizer) => _api = api;

        // بيانات المحل + تعديلها
        public async Task<IActionResult> Index()
        {
            var store = await _api.GetMyRestaurant();
            if (store == null) return RedirectToAction("Login", "Auth");

            ViewData["Title"] = "محلي";
            ViewBag.StoreType = store.StoreType;
            return View(store);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateInfo(UpdateRestaurantDto dto)
        {
            var store = await _api.GetMyRestaurant();
            if (store == null) return Forbid();

            var (ok, error) = await _api.UpdateRestaurant(store.Id, dto);
            TempData[ok ? "Success" : "Error"] = ok ? "تم حفظ بيانات المحل" : (error ?? "فشل الحفظ");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleOpen()
        {
            var store = await _api.GetMyRestaurant();
            if (store == null) return Forbid();

            await _api.ToggleRestaurant(store.Id);
            return RedirectToAction(nameof(Index));
        }

        // المنيو (أقسام + منتجات) — لصاحب الصيدلية ممكن تختفي التبويب ده من الـ View لو مش محتاجه
        public async Task<IActionResult> Menu()
        {
            var store = await _api.GetMyRestaurant();
            if (store == null) return RedirectToAction("Login", "Auth");

            ViewData["Title"] = "المنيو";
            ViewBag.RestaurantId = store.Id;
            ViewBag.StoreType = store.StoreType;

            var categories = await _api.GetCategories(store.Id) ?? new();
            return View(categories);
        }

        // منتجات قسم معين (بيتأكد إن القسم ده تابع لمحل صاحب الحساب فعلاً)
        public async Task<IActionResult> MenuProducts(int categoryId)
        {
            var store = await _api.GetMyRestaurant();
            if (store == null) return RedirectToAction("Login", "Auth");

            var category = await _api.GetCategory(categoryId);
            if (category == null || category.RestaurantId != store.Id) return Forbid();

            ViewData["Title"] = category.Name;
            ViewBag.RestaurantId = store.Id;
            ViewBag.CategoryId = categoryId;

            var products = await _api.SearchProducts(restaurantId: store.Id, categoryId: categoryId, size: 200);
            return View(products?.Data ?? new());
        }

        // الطلبات الجاية للمحل بتاعه
        public async Task<IActionResult> Orders(string? status)
        {
            var store = await _api.GetMyRestaurant();
            if (store == null) return RedirectToAction("Login", "Auth");

            ViewData["Title"] = "طلباتي";
            ViewBag.StoreType = store.StoreType;
            ViewBag.Status = status;

            var orders = await _api.GetOrdersByRestaurant(store.Id, status);
            return View(orders?.Data ?? new());
        }

        // السواقين اللي شغالين/اشتغلوا على طلبات المحل بتاعه
        public async Task<IActionResult> Drivers()
        {
            var store = await _api.GetMyRestaurant();
            if (store == null) return RedirectToAction("Login", "Auth");

            ViewData["Title"] = "سائقين الدليفري";
            var result = await _api.GetStoreDrivers(store.Id);
            return View(result?.Data ?? new());
        }

        // تقييمات المحل بتاعه (من العملاء)
        public async Task<IActionResult> Ratings(int page = 1)
        {
            var store = await _api.GetMyRestaurant();
            if (store == null) return RedirectToAction("Login", "Auth");

            ViewData["Title"] = "تقييمات محلي";
            var result = await _api.GetStoreRatings(store.Id, page, 20);
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling((result?.Total ?? 0) / 20.0);
            ViewBag.AvgRestaurant = result?.AvgRestaurant ?? 0;
            ViewBag.AvgFood = result?.AvgFood ?? 0;
            ViewBag.Total = result?.Total ?? 0;
            return View(result?.Data ?? new());
        }
    }
}
