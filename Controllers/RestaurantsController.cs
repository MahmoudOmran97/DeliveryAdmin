using DeliveryAdmin.Models;
using DeliveryAdmin.Resources;
using DeliveryAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DeliveryAdmin.Controllers
{
    [Authorize]
    public class RestaurantsController : LocalizedController
    {
        private readonly ApiService _api;
        public RestaurantsController(ApiService api, IStringLocalizer<SharedResource> localizer) : base(localizer) => _api = api;

        private bool IsSuperAdmin => User.Claims.Any(c => c.Type == "IsSuperAdmin" && c.Value == "true");

        public async Task<IActionResult> Index(string? search, bool? isOpen, int page = 1)
        {
            SetTitle("Restaurants_Title");
            var result = await _api.GetRestaurants(page, 12, search, isOpen);
            ViewBag.Search = search; ViewBag.IsOpen = isOpen;
            ViewBag.Page = page; ViewBag.TotalPages = (int)Math.Ceiling((result?.Total ?? 0) / 12.0);
            ViewBag.Total = result?.Total ?? 0;
            ViewBag.IsSuperAdmin = IsSuperAdmin;
            return View(result?.Data ?? new());
        }

        public async Task<IActionResult> Details(int id)
        {
            var rest = await _api.GetRestaurant(id);
            if (rest == null) return NotFound();
            ViewBag.Categories = await _api.GetCategories(id) ?? new();
            ViewBag.IsSuperAdmin = IsSuperAdmin;
            return View(rest);
        }

        public async Task<IActionResult> Create()
        {
            await LoadOwnerOptionsAsync();
            return View(new CreateRestaurantDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateRestaurantDto dto)
        {
            var (ok, error) = await _api.CreateRestaurant(dto);
            if (!ok) { TempData["Error"] = error; await LoadOwnerOptionsAsync(); return View(dto); }
            TempData["Success"] = "Restaurant created successfully!";
            return RedirectToAction("Index");
        }

        private async Task LoadOwnerOptionsAsync() =>
            ViewBag.Owners = (await _api.GetUsers(1, 200))?.Data?.Where(u => u.IsActive).ToList() ?? new();

        public async Task<IActionResult> Edit(int id)
        {
            var r = await _api.GetRestaurant(id);
            if (r == null) return NotFound();
            ViewBag.Owners = (await _api.GetUsers(1, 200))?.Data?.Where(u => u.IsActive).ToList() ?? new();
            var dto = new UpdateRestaurantDto
            {
                Name = r.Name,
                Description = r.Description,
                Address = r.Address,
                Phone = r.Phone,
                Latitude = r.Latitude,
                Longitude = r.Longitude,
                DeliveryFee = r.DeliveryFee,
                MinOrderAmount = r.MinOrderAmount,
                EstimatedTime = r.EstimatedTime,
                ImageUrl = r.ImageUrl,
                CoverImageUrl = r.CoverImageUrl,
                IsOpen = r.IsOpen,
                OwnerUserId = r.OwnerUserId,
                StoreType = r.StoreType
            };
            ViewBag.RestaurantId = id; ViewBag.RestaurantName = r.Name;
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, UpdateRestaurantDto dto)
        {
            var (ok, error) = await _api.UpdateRestaurant(id, dto);
            if (!ok)
            {
                TempData["Error"] = error;
                ViewBag.RestaurantId = id;
                ViewBag.RestaurantName = dto.Name;
                // ✅ Fix: يجب تحميل Owners عشان الـ dropdown في الـ View مش يطلع NullReferenceException
                ViewBag.Owners = (await _api.GetUsers(1, 200))?.Data?.Where(u => u.IsActive).ToList() ?? new();
                return View(dto);
            }
            TempData["Success"] = "Restaurant updated!";
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int id)
        {
            await _api.ToggleRestaurant(id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsSuperAdmin) return Forbid();

            var (ok, error) = await _api.DeleteRestaurant(id);
            if (!ok)
            {
                TempData["Error"] = error ?? "Failed to delete store";
                return RedirectToAction("Details", new { id });
            }
            TempData["Success"] = "تم حذف المحل وكل أقسامه ومنتجاته بنجاح";
            return RedirectToAction("Index");
        }

        // نسخ المنتجات من محل لمحل — سوبر أدمن فقط
        public async Task<IActionResult> CopyMenu(int? sourceId)
        {
            if (!IsSuperAdmin) return Forbid();

            var all = await _api.GetRestaurants(1, 500);
            var stores = all?.Data ?? new();
            ViewBag.Stores = stores;
            ViewBag.SourceId = sourceId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CopyMenu(int sourceId, int targetId)
        {
            if (!IsSuperAdmin) return Forbid();

            if (sourceId == targetId)
            {
                TempData["Error"] = "لازم تختار محل مصدر ومحل هدف مختلفين";
                return RedirectToAction(nameof(CopyMenu), new { sourceId });
            }

            var (ok, error, data) = await _api.CopyRestaurantMenu(sourceId, targetId);
            if (!ok)
            {
                TempData["Error"] = error ?? "فشل نسخ المنتجات";
                return RedirectToAction(nameof(CopyMenu), new { sourceId });
            }

            var copied = (data?.CategoriesCopied ?? 0) + (data?.ProductsCopied ?? 0);
            TempData["Success"] = copied == 0
                ? "مفيش حاجة جديدة للنسخ — كل الأقسام والمنتجات موجودة بالفعل"
                : $"تم نسخ الجديد فقط: {data?.CategoriesCopied ?? 0} قسم، {data?.ProductsCopied ?? 0} منتج، {data?.VariantsCopied ?? 0} خيار";
            return RedirectToAction("Details", new { id = targetId });
        }
    }
}