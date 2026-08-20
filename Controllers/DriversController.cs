using DeliveryAdmin.Resources;
using DeliveryAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DeliveryAdmin.Controllers
{
    [Authorize]
    public class DriversController : LocalizedController
    {
        private readonly ApiService _api;
        public DriversController(ApiService api, IStringLocalizer<SharedResource> localizer) : base(localizer) => _api = api;

        public async Task<IActionResult> Index(string? filter, string? search, int page = 1)
        {
            SetTitle("Drivers_Title");
            // Fix: fetch a larger page to get accurate counts, not just first 20
            var result = await _api.GetDrivers(page, 100);
            var all = result?.Data ?? new();
            var filtered = filter switch
            {
                "online" => all.Where(d => d.IsOnline).ToList(),
                "offline" => all.Where(d => !d.IsOnline).ToList(),
                "verified" => all.Where(d => d.IsVerified).ToList(),
                "pending" => all.Where(d => !d.IsVerified).ToList(),
                _ => all
            };
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                filtered = filtered.Where(d =>
                    (d.FullName?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.UserName?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.Phone?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.LicensePlate?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();
            }
            ViewBag.Filter = filter;
            ViewBag.Search = search;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling((result?.Total ?? 0) / 100.0);
            ViewBag.Total = result?.Total ?? 0;
            ViewBag.TotalAll = all.Count;
            ViewBag.TotalOnline = all.Count(d => d.IsOnline);
            ViewBag.TotalVerified = all.Count(d => d.IsVerified);
            ViewBag.TotalPending = all.Count(d => !d.IsVerified);
            return View(filtered);
        }

        // ─────────────────────────────────────────────
        // GET /Drivers/LiveMapData  — بيانات الخريطة الحية (JSON)
        // بيرجع كل السواقين اللي عندهم موقع + كل المحلات مع عدد الطلبات المعلقة
        // بيتنادى بالـ AJAX من مودال الخريطة كل شوية عشان التحديث اللحظي
        // ─────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> LiveMapData()
        {
            var driversTask = _api.GetDrivers(1, 1000);
            var restaurantsTask = _api.GetRestaurantsMap();
            await Task.WhenAll(driversTask, restaurantsTask);

            var drivers = (driversTask.Result?.Data ?? new())
                .Where(d => d.CurrentLatitude.HasValue && d.CurrentLongitude.HasValue)
                .Select(d => new
                {
                    id = d.Id,
                    name = d.UserName ?? d.FullName ?? ("#" + d.Id),
                    phone = d.Phone,
                    vehicleType = d.VehicleType,
                    rating = d.Rating,
                    isOnline = d.IsOnline,
                    isAvailable = d.IsAvailable,
                    lat = d.CurrentLatitude,
                    lng = d.CurrentLongitude
                });

            var restaurants = (restaurantsTask.Result ?? new())
                .Select(r => new
                {
                    id = r.Id,
                    name = r.Name,
                    storeType = r.StoreType,
                    address = r.Address,
                    imageUrl = r.ImageUrl,
                    lat = r.Latitude,
                    lng = r.Longitude,
                    isOpen = r.IsOpen,
                    pendingOrders = r.PendingOrders
                });

            return Json(new { drivers, restaurants, serverTime = DateTime.UtcNow });
        }

        [HttpPost]
        public async Task<IActionResult> Verify(int id)
        {
            var (ok, error) = await _api.VerifyDriver(id);
            TempData[ok ? "Success" : "Error"] = ok ? "Driver verified!" : error;
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var d = await _api.GetDriver(id);
            if (d == null) return NotFound();
            ViewBag.DriverId = id;
            ViewBag.DriverName = d.FullName ?? d.UserName;
            return View(new DeliveryAdmin.Models.AdminUpdateDriverDto
            {
                VehicleType = d.VehicleType,
                LicensePlate = d.LicensePlate,
                NationalId = d.NationalId,
                IsVerified = d.IsVerified,
                IsAvailable = d.IsAvailable
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, DeliveryAdmin.Models.AdminUpdateDriverDto dto)
        {
            var (ok, error) = await _api.UpdateDriver(id, dto);
            if (!ok)
            {
                TempData["Error"] = error;
                ViewBag.DriverId = id;
                return View(dto);
            }
            TempData["Success"] = "Driver updated successfully";
            return RedirectToAction("Index");
        }
    }
}