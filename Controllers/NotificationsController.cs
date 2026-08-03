using DeliveryAdmin.Models;
using DeliveryAdmin.Resources;
using DeliveryAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DeliveryAdmin.Controllers;

[Authorize]
public class NotificationsController : LocalizedController
{
    private readonly ApiService _api;
    public NotificationsController(ApiService api, IStringLocalizer<SharedResource> localizer) : base(localizer) => _api = api;

    // ─────────────────────────────────────────────────────────────
    // زر الجرس 🔔 — عداد + قائمة، متاحة لأي مستخدم مسجل دخول (أدمن
    // أو صاحب محل)، كل واحد بياخد تنبيهاته هو بس (الـ API بيفلتر
    // على الـ userId من التوكن).
    // ─────────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Feed(int page = 1)
    {
        var result = await _api.GetNotifications(page, 15);
        return Json(result ?? new PagedResult<NotificationDto>());
    }

    [HttpGet]
    public async Task<IActionResult> UnreadCount()
    {
        var count = await _api.GetUnreadNotificationCount();
        return Json(new { count });
    }

    [HttpPost]
    public async Task<IActionResult> MarkRead(int id)
    {
        var ok = await _api.MarkNotificationRead(id);
        return Json(new { ok });
    }

    [HttpPost]
    public async Task<IActionResult> MarkAllRead()
    {
        var ok = await _api.MarkAllNotificationsRead();
        return Json(new { ok });
    }

    // ─────────────────────────────────────────────────────────────
    // إرسال تنبيه يدوي (بث) — أدمن بس، صاحب المحل ملوش دعوة بيها
    // ─────────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Send()
    {
        SetTitle("Notif_Title");
        ViewBag.Users = (await _api.GetUsers(1, 200))?.Data ?? new();
        return View(new SendNotificationDto());
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Send(SendNotificationDto dto, string sendMode)
    {
        if (sendMode == "role") dto.UserId = null;
        else dto.Role = null;

        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Body))
        {
            TempData["Error"] = "Title and message are required";
            ViewBag.Users = (await _api.GetUsers(1, 200))?.Data ?? new();
            return View(dto);
        }

        var (ok, error, count) = await _api.SendNotification(dto);
        if (!ok)
        {
            TempData["Error"] = error;
            ViewBag.Users = (await _api.GetUsers(1, 200))?.Data ?? new();
            return View(dto);
        }

        TempData["Success"] = $"Notification sent to {count} user(s)";
        return RedirectToAction(nameof(Send));
    }
}
