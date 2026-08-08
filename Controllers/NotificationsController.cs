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
        ViewBag.Restaurants = (await _api.GetRestaurants(1, 500))?.Data ?? new();
        return View(new SendNotificationDto());
    }

    // GET /Notifications/SearchUsers?q=... — بحث حي بكل المستخدمين (أي دور)
    // بالاسم/الإيميل/رقم التليفون. لو q فاضي، بيرجّع أحدث 20 مستخدم مسجل
    // عشان تلاقي المستخدمين الجداد بسهولة من غير ما تعرف اسمهم بالظبط.
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> SearchUsers(string? q)
    {
        var search = string.IsNullOrWhiteSpace(q) ? null : q.Trim();
        if (search != null && search.Length < 2)
            return Json(new List<object>());

        var result = await _api.GetUsers(1, 20, role: null, search: search);
        var list = (result?.Data ?? new()).Select(u => new
        {
            id = u.Id,
            fullName = u.FullName,
            phone = u.Phone,
            email = u.Email,
            role = u.Role
        });
        return Json(list);
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
            ViewBag.Restaurants = (await _api.GetRestaurants(1, 500))?.Data ?? new();
            if (dto.UserId.HasValue) ViewBag.SelectedUser = await _api.GetUser(dto.UserId.Value);
            return View(dto);
        }

        var (ok, error, count) = await _api.SendNotification(dto);
        if (!ok)
        {
            TempData["Error"] = error;
            ViewBag.Restaurants = (await _api.GetRestaurants(1, 500))?.Data ?? new();
            if (dto.UserId.HasValue) ViewBag.SelectedUser = await _api.GetUser(dto.UserId.Value);
            return View(dto);
        }

        TempData["Success"] = $"Notification sent to {count} user(s)";
        return RedirectToAction(nameof(Send));
    }
}
