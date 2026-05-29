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

    public async Task<IActionResult> Send()
    {
        SetTitle("Notif_Title");
        ViewBag.Users = (await _api.GetUsers(1, 200))?.Data ?? new();
        return View(new SendNotificationDto());
    }

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
