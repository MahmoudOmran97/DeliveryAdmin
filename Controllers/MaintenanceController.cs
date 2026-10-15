using DeliveryAdmin.Models;
using DeliveryAdmin.Resources;
using DeliveryAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DeliveryAdmin.Controllers;

[Authorize]
public class MaintenanceController : LocalizedController
{
    private readonly ApiService _api;

    public MaintenanceController(ApiService api, IStringLocalizer<SharedResource> localizer) : base(localizer)
        => _api = api;

    private bool IsSuperAdmin => User.Claims.Any(c => c.Type == "IsSuperAdmin" && c.Value == "true");

    public async Task<IActionResult> Index(int olderThanDays = 30)
    {
        if (!IsSuperAdmin) return Forbid();

        SetTitle("صيانة الداتابيز");
        if (olderThanDays < 1) olderThanDays = 30;

        ViewBag.OlderThanDays = olderThanDays;
        ViewBag.Summary = await _api.GetMaintenanceSummary(olderThanDays);
        ViewBag.Drivers = await _api.GetMaintenanceDrivers() ?? new();
        ViewBag.Admins = await _api.GetMaintenanceAdmins() ?? new();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> PurgeNotifications(int olderThanDays = 30)
    {
        if (!IsSuperAdmin) return Forbid();
        var (ok, error, data) = await _api.PurgeNotifications(olderThanDays);
        TempData[ok ? "Success" : "Error"] = ok
            ? $"تم حذف {data?.Deleted ?? 0} إشعار"
            : (error ?? "فشل الحذف");
        return RedirectToAction(nameof(Index), new { olderThanDays });
    }

    [HttpPost]
    public async Task<IActionResult> PurgeOrders(int olderThanDays = 30)
    {
        if (!IsSuperAdmin) return Forbid();
        var (ok, error, data) = await _api.PurgeOrders(olderThanDays);
        TempData[ok ? "Success" : "Error"] = ok
            ? $"تم حذف {data?.Deleted ?? 0} طلب منتهي (ومعه التوابع)"
            : (error ?? "فشل الحذف");
        return RedirectToAction(nameof(Index), new { olderThanDays });
    }

    [HttpPost]
    public async Task<IActionResult> PurgeOrderChats(int olderThanDays = 30)
    {
        if (!IsSuperAdmin) return Forbid();
        var (ok, error, data) = await _api.PurgeOrderChats(olderThanDays);
        TempData[ok ? "Success" : "Error"] = ok
            ? $"تم حذف {data?.Deleted ?? 0} رسالة شات طلبات"
            : (error ?? "فشل الحذف");
        return RedirectToAction(nameof(Index), new { olderThanDays });
    }

    [HttpPost]
    public async Task<IActionResult> PurgePharmacyChats(int olderThanDays = 30)
    {
        if (!IsSuperAdmin) return Forbid();
        var (ok, error, data) = await _api.PurgePharmacyChats(olderThanDays);
        TempData[ok ? "Success" : "Error"] = ok
            ? $"تم حذف {data?.MessagesDeleted ?? 0} رسالة صيدلية و {data?.RequestsDeleted ?? 0} طلب روشتة"
            : (error ?? "فشل الحذف");
        return RedirectToAction(nameof(Index), new { olderThanDays });
    }

    [HttpPost]
    public async Task<IActionResult> PurgeSupportChats(int olderThanDays = 30)
    {
        if (!IsSuperAdmin) return Forbid();
        var (ok, error, data) = await _api.PurgeSupportChats(olderThanDays);
        TempData[ok ? "Success" : "Error"] = ok
            ? $"تم حذف {data?.MessagesDeleted ?? 0} رسالة دعم و {data?.SessionsDeleted ?? 0} جلسة مقفولة"
            : (error ?? "فشل الحذف");
        return RedirectToAction(nameof(Index), new { olderThanDays });
    }

    [HttpPost]
    public async Task<IActionResult> PurgeOtps(int olderThanDays = 30)
    {
        if (!IsSuperAdmin) return Forbid();
        var (ok, error, data) = await _api.PurgeOtps();
        TempData[ok ? "Success" : "Error"] = ok
            ? $"تم حذف {data?.Deleted ?? 0} كود OTP"
            : (error ?? "فشل الحذف");
        return RedirectToAction(nameof(Index), new { olderThanDays });
    }

    [HttpPost]
    public async Task<IActionResult> PurgeDriverLocations(int olderThanDays = 14)
    {
        if (!IsSuperAdmin) return Forbid();
        var (ok, error, data) = await _api.PurgeDriverLocations(olderThanDays);
        TempData[ok ? "Success" : "Error"] = ok
            ? $"تم حذف {data?.Deleted ?? 0} موقع سائق قديم"
            : (error ?? "فشل الحذف");
        return RedirectToAction(nameof(Index), new { olderThanDays });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteDriver(int driverId, int olderThanDays = 30)
    {
        if (!IsSuperAdmin) return Forbid();
        var (ok, error) = await _api.DeleteMaintenanceDriver(driverId);
        TempData[ok ? "Success" : "Error"] = ok ? "تم حذف السائق" : (error ?? "فشل حذف السائق");
        return RedirectToAction(nameof(Index), new { olderThanDays });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteAdmin(int userId, int olderThanDays = 30)
    {
        if (!IsSuperAdmin) return Forbid();
        var (ok, error) = await _api.DeleteMaintenanceAdmin(userId);
        TempData[ok ? "Success" : "Error"] = ok ? "تم حذف الأدمن" : (error ?? "فشل حذف الأدمن");
        return RedirectToAction(nameof(Index), new { olderThanDays });
    }
}
