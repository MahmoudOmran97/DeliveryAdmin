using DeliveryAdmin.Models;
using DeliveryAdmin.Resources;
using DeliveryAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DeliveryAdmin.Controllers;

// أرباحنا: اشتراكات المحلات وعمولة السواقين اللي بتيجي للمنصة
// (عكس Settlements اللي بيحسب المستحق للمحل/السواق منّنا)
[Authorize]
public class RevenueController : LocalizedController
{
    private readonly ApiService _api;

    public RevenueController(ApiService api, IStringLocalizer<SharedResource> localizer) : base(localizer)
        => _api = api;

    // ── الاستحقاقات والتحصيل ─────────────────────────────
    public async Task<IActionResult> Index(string tab = "store", string? status = null, DateTime? from = null, DateTime? to = null)
    {
        SetTitle("Revenue_Title");

        var entityType = tab == "driver" ? RevenueEntityType.Driver : RevenueEntityType.Store;
        SettlementStatus? statusFilter = Enum.TryParse<SettlementStatus>(status, out var s) ? s : null;

        var settlements = await _api.GetRevenueSettlements(entityType, statusFilter, from, to);
        var summary = await _api.GetRevenueSummary() ?? new RevenueSummaryDto();

        ViewBag.Tab = tab;
        ViewBag.Status = status;
        ViewBag.From = from?.ToString("yyyy-MM-dd");
        ViewBag.To = to?.ToString("yyyy-MM-dd");
        ViewBag.Summary = summary;

        return View(settlements);
    }

    [HttpPost]
    public async Task<IActionResult> Generate(DateTime periodStart, DateTime periodEnd, string tab = "store")
    {
        var (ok, error) = await _api.GenerateRevenueSettlements(periodStart, periodEnd);
        if (!ok) TempData["Error"] = error;
        return RedirectToAction("Index", new { tab });
    }

    [HttpPost]
    public async Task<IActionResult> MarkPaid(int id, decimal? amountPaid, string? notes, string tab = "store")
    {
        var (ok, error) = await _api.MarkSettlementPaid(id, amountPaid, notes);
        if (!ok) TempData["Error"] = error;
        return RedirectToAction("Index", new { tab });
    }

    // ── إدارة خطط الاشتراك (نسبة / مبلغ ثابت) لكل محل أو سواق ──
    public async Task<IActionResult> Plans(string tab = "store")
    {
        SetTitle("Revenue_PlansTitle");

        var entityType = tab == "driver" ? RevenueEntityType.Driver : RevenueEntityType.Store;
        var plans = await _api.GetSubscriptionPlans(entityType);

        ViewBag.Tab = tab;

        if (entityType == RevenueEntityType.Store)
            ViewBag.Restaurants = (await _api.GetRestaurants(1, 500))?.Data ?? new();
        else
            ViewBag.Drivers = (await _api.GetDrivers(1, 500))?.Data ?? new();

        return View(plans);
    }

    [HttpPost]
    public async Task<IActionResult> SavePlan(string tab, int entityId, SubscriptionType type, decimal value, bool isActive = true)
    {
        var dto = new UpsertSubscriptionPlanDto
        {
            EntityType = tab == "driver" ? RevenueEntityType.Driver : RevenueEntityType.Store,
            RestaurantId = tab == "driver" ? null : entityId,
            DriverId = tab == "driver" ? entityId : null,
            Type = type,
            Value = value,
            IsActive = isActive
        };

        var (ok, error) = await _api.SaveSubscriptionPlan(dto);
        if (!ok) TempData["Error"] = error;
        return RedirectToAction("Plans", new { tab });
    }
}
