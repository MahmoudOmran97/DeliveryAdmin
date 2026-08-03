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
        private readonly IConfiguration _config;
        public MyStoreController(ApiService api, IStringLocalizer<SharedResource> localizer, IConfiguration config) : base(localizer)
        {
            _api = api;
            _config = config;
        }

        // بيانات المحل + تعديلها
        public async Task<IActionResult> Index()
        {
            var store = await _api.GetMyRestaurant();
            if (store == null) return RedirectToAction("Login", "Auth");

            ViewData["Title"] = L["Nav_MyStore"].Value;
            ViewBag.StoreType = store.StoreType;
            return View(store);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateInfo(UpdateRestaurantDto dto)
        {
            var store = await _api.GetMyRestaurant();
            if (store == null) return Forbid();

            var (ok, error) = await _api.UpdateRestaurant(store.Id, dto);
            TempData[ok ? "Success" : "Error"] = ok ? L["Msg_StoreInfoSaved"].Value : (error ?? L["Msg_SaveFailed"].Value);
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

            ViewData["Title"] = L["Nav_MyMenu"].Value;
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
            ViewBag.CategoryName = category.Name;

            var products = await _api.GetMyStoreProducts(categoryId);
            return View(products ?? new());
        }

        // ── إضافة/تعديل/حذف قسم ────────────────────────────────────────
        public async Task<IActionResult> CreateCategory()
        {
            var store = await _api.GetMyRestaurant();
            if (store == null) return RedirectToAction("Login", "Auth");
            ViewData["Title"] = L["Cat_Add"].Value;
            return View(new CreateCategoryDto { RestaurantId = store.Id });
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(CreateCategoryDto dto)
        {
            var store = await _api.GetMyRestaurant();
            if (store == null) return Forbid();
            dto.RestaurantId = store.Id; // تجاهل أي قيمة متبعتة من الفورم عشان محدش يضيف قسم لمحل غيره

            var (ok, error) = await _api.CreateCategory(dto);
            TempData[ok ? "Success" : "Error"] = ok ? L["Msg_CategoryAdded"].Value : (error ?? L["Msg_CategoryAddFailed"].Value);
            return ok ? RedirectToAction(nameof(Menu)) : RedirectToAction(nameof(CreateCategory));
        }

        public async Task<IActionResult> EditCategory(int id)
        {
            var store = await _api.GetMyRestaurant();
            if (store == null) return RedirectToAction("Login", "Auth");

            var c = await _api.GetCategory(id);
            if (c == null || c.RestaurantId != store.Id) return Forbid();

            ViewData["Title"] = L["Cat_Edit"].Value;
            ViewBag.CategoryId = id;
            return View(new UpdateCategoryDto { Name = c.Name, ImageUrl = c.ImageUrl, SortOrder = c.SortOrder });
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory(int id, UpdateCategoryDto dto)
        {
            var store = await _api.GetMyRestaurant();
            if (store == null) return Forbid();

            var c = await _api.GetCategory(id);
            if (c == null || c.RestaurantId != store.Id) return Forbid();

            var (ok, error) = await _api.UpdateCategory(id, dto);
            TempData[ok ? "Success" : "Error"] = ok ? L["Msg_EditSaved"].Value : (error ?? L["Msg_EditSaveFailed"].Value);
            return ok ? RedirectToAction(nameof(Menu)) : RedirectToAction(nameof(EditCategory), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var store = await _api.GetMyRestaurant();
            if (store == null) return Forbid();

            var c = await _api.GetCategory(id);
            if (c == null || c.RestaurantId != store.Id) return Forbid();

            var (ok, error) = await _api.DeleteCategory(id);
            TempData[ok ? "Success" : "Error"] = ok ? L["Msg_CategoryDeleted"].Value : (error ?? L["Msg_DeleteFailed"].Value);
            return RedirectToAction(nameof(Menu));
        }

        // ── إضافة/تعديل/حذف منتج ───────────────────────────────────────
        public async Task<IActionResult> CreateProduct(int categoryId)
        {
            var store = await _api.GetMyRestaurant();
            if (store == null) return RedirectToAction("Login", "Auth");

            var category = await _api.GetCategory(categoryId);
            if (category == null || category.RestaurantId != store.Id) return Forbid();

            ViewData["Title"] = L["Products_Add"].Value;
            ViewBag.CategoryId = categoryId;
            ViewBag.CategoryName = category.Name;
            return View(new CreateProductDto { CategoryId = categoryId });
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(CreateProductDto dto, string? variantsJson)
        {
            var store = await _api.GetMyRestaurant();
            if (store == null) return Forbid();

            var category = await _api.GetCategory(dto.CategoryId);
            if (category == null || category.RestaurantId != store.Id) return Forbid();

            var (ok, error, newId) = await _api.CreateProductWithId(dto);
            if (!ok || newId == null)
            {
                TempData["Error"] = error ?? L["Msg_ProductAddFailed"].Value;
                return RedirectToAction(nameof(CreateProduct), new { categoryId = dto.CategoryId });
            }

            var variants = ParseVariants(variantsJson);
            if (variants.Any())
            {
                var (vOk, vErr) = await _api.SetProductVariants(newId.Value, variants);
                if (!vOk) TempData["Error"] = $"{L["Msg_ProductAdded"].Value}, {vErr}";
            }

            TempData["Success"] = L["Msg_ProductAdded"].Value;
            return RedirectToAction(nameof(MenuProducts), new { categoryId = dto.CategoryId });
        }

        public async Task<IActionResult> EditProduct(int id)
        {
            var store = await _api.GetMyRestaurant();
            if (store == null) return RedirectToAction("Login", "Auth");

            var p = await _api.GetProduct(id);
            if (p == null || p.RestaurantId != store.Id) return Forbid();

            ViewData["Title"] = L["Products_Edit"].Value;
            ViewBag.ProductId = id;
            var categoryId = p.Category is System.Text.Json.JsonElement cat && cat.TryGetProperty("id", out var cid) ? cid.GetInt32() : 0;
            ViewBag.CategoryId = categoryId;

            var dto = new CreateProductDto
            {
                CategoryId = categoryId,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                DiscountedPrice = p.DiscountedPrice,
                ImageUrl = p.ImageUrl,
                PreparationTime = p.PreparationTime,
                Calories = p.Calories
            };
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(int id, CreateProductDto dto, string? variantsJson)
        {
            var store = await _api.GetMyRestaurant();
            if (store == null) return Forbid();

            var p = await _api.GetProduct(id);
            if (p == null || p.RestaurantId != store.Id) return Forbid();

            var (ok, error) = await _api.UpdateProduct(id, dto);
            if (!ok)
            {
                TempData["Error"] = error ?? L["Msg_EditSaveFailed"].Value;
                return RedirectToAction(nameof(EditProduct), new { id });
            }

            var variants = ParseVariants(variantsJson);
            var (vOk, vErr) = await _api.SetProductVariants(id, variants);
            TempData[vOk ? "Success" : "Error"] = vOk ? L["Msg_EditSaved"].Value : $"{L["Msg_EditSaved"].Value}, {vErr}";
            return RedirectToAction(nameof(MenuProducts), new { categoryId = dto.CategoryId });
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
        public async Task<IActionResult> ToggleProduct(int id, int categoryId)
        {
            var store = await _api.GetMyRestaurant();
            if (store == null) return Forbid();

            var p = await _api.GetProduct(id);
            if (p == null || p.RestaurantId != store.Id) return Forbid();

            await _api.ToggleProduct(id);
            return RedirectToAction(nameof(MenuProducts), new { categoryId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id, int categoryId)
        {
            var store = await _api.GetMyRestaurant();
            if (store == null) return Forbid();

            var p = await _api.GetProduct(id);
            if (p == null || p.RestaurantId != store.Id) return Forbid();

            var (ok, error) = await _api.DeleteProduct(id);
            TempData[ok ? "Success" : "Error"] = ok ? L["Msg_ProductDeleted"].Value : (error ?? L["Msg_DeleteFailed"].Value);
            return RedirectToAction(nameof(MenuProducts), new { categoryId });
        }

        // تفاصيل طلب معين (بيتأكد إن الطلب ده تابع لمحل صاحب الحساب)
        public async Task<IActionResult> OrderDetails(int id)
        {
            var store = await _api.GetMyRestaurant();
            if (store == null) return RedirectToAction("Login", "Auth");

            var order = await _api.GetOrder(id);
            if (order == null || order.Restaurant?.Id != store.Id) return Forbid();

            return View(order);
        }

        // الطلبات الجاية للمحل بتاعه
        public async Task<IActionResult> Orders(string? status)
        {
            var store = await _api.GetMyRestaurant();
            if (store == null) return RedirectToAction("Login", "Auth");

            ViewData["Title"] = L["Nav_MyOrders"].Value;
            ViewBag.StoreType = store.StoreType;
            ViewBag.Status = status;
            ViewBag.StoreId = store.Id;
            // بيتبعتوا لـ JS عشان يوصل الـ SignalR Hub ويستقبل تنبيه لحظي لما يجي أوردر جديد
            ViewBag.ApiToken = _api.GetCurrentToken();
            var apiBase = _config["ApiSettings:BaseUrl"]?.TrimEnd('/') ?? "";
            ViewBag.HubBaseUrl = apiBase.EndsWith("/api") ? apiBase[..^4] : apiBase;

            var orders = await _api.GetOrdersByRestaurant(store.Id, status);
            return View(orders?.Data ?? new());
        }

        // قبول / رفض / تحضير / جاهز — بيتاخد من زرار في صفحة الطلبات
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {
            var store = await _api.GetMyRestaurant();
            if (store == null) return Forbid();

            var (ok, error) = await _api.UpdateMyOrderStatus(id, status);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return ok ? Ok() : BadRequest(new { message = error ?? L["Msg_OrderUpdateFailed"].Value });

            TempData[ok ? "Success" : "Error"] = ok ? $"{L["Msg_OrderUpdated"].Value} #{id}" : (error ?? L["Msg_OrderUpdateFailed"].Value);
            return RedirectToAction(nameof(Orders));
        }

        // السواقين اللي شغالين/اشتغلوا على طلبات المحل بتاعه
        public async Task<IActionResult> Drivers()
        {
            var store = await _api.GetMyRestaurant();
            if (store == null) return RedirectToAction("Login", "Auth");

            ViewData["Title"] = L["Nav_MyDrivers"].Value;
            var result = await _api.GetStoreDrivers(store.Id);
            return View(result?.Data ?? new());
        }

        // تقييمات المحل بتاعه (من العملاء)
        public async Task<IActionResult> Ratings(int page = 1)
        {
            var store = await _api.GetMyRestaurant();
            if (store == null) return RedirectToAction("Login", "Auth");

            ViewData["Title"] = L["Nav_MyRatings"].Value;
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
