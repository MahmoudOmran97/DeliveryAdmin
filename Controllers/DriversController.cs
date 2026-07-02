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

        public async Task<IActionResult> Index(string? filter, int page = 1)
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
            ViewBag.Filter = filter;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling((result?.Total ?? 0) / 100.0);
            ViewBag.Total = result?.Total ?? 0;
            ViewBag.TotalAll = all.Count;
            ViewBag.TotalOnline = all.Count(d => d.IsOnline);
            ViewBag.TotalVerified = all.Count(d => d.IsVerified);
            ViewBag.TotalPending = all.Count(d => !d.IsVerified);
            return View(filtered);
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