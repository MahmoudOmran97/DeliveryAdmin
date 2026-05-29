using DeliveryAdmin.Models;
using DeliveryAdmin.Resources;
using DeliveryAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DeliveryAdmin.Controllers;

[Authorize]
public class SettlementsController : LocalizedController
{
    private readonly ApiService _api;

    public SettlementsController(ApiService api, IStringLocalizer<SharedResource> localizer) : base(localizer)
        => _api = api;

    public async Task<IActionResult> Index(DateTime? from, DateTime? to, int? driverId, int? restaurantId)
    {
        SetTitle("Settlements_Title");

        from ??= new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        to ??= DateTime.UtcNow.Date;

        var report = await _api.GetSettlements(from, to, driverId, restaurantId)
                     ?? new SettlementReportDto();

        ViewBag.From = from.Value.ToString("yyyy-MM-dd");
        ViewBag.To = to.Value.ToString("yyyy-MM-dd");
        ViewBag.DriverId = driverId;
        ViewBag.RestaurantId = restaurantId;
        ViewBag.Drivers = (await _api.GetDrivers(1, 200))?.Data ?? new();
        ViewBag.Restaurants = (await _api.GetRestaurants(1, 200))?.Data ?? new();

        return View(report);
    }
}
