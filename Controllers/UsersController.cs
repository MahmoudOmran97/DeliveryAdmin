using DeliveryAdmin.Models;
using DeliveryAdmin.Resources;
using DeliveryAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DeliveryAdmin.Controllers;

[Authorize]
public class UsersController : LocalizedController
{
    private readonly ApiService _api;
    public UsersController(ApiService api, IStringLocalizer<SharedResource> localizer) : base(localizer) => _api = api;

    public async Task<IActionResult> Index(string? role, int page = 1)
    {
        SetTitle("Users_Title");
        var result = await _api.GetUsers(page, 20, role);
        ViewBag.Role = role;
        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling((result?.Total ?? 0) / 20.0);
        ViewBag.Total = result?.Total ?? 0;
        return View(result?.Data ?? new());
    }

    public async Task<IActionResult> Create()
    {
        SetTitle("Users_Add");
        ViewBag.Restaurants = (await _api.GetRestaurants(1, 200))?.Data ?? new();
        return View(new CreateUserDto { Role = "Restaurant" });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Restaurants = (await _api.GetRestaurants(1, 200))?.Data ?? new();
            return View(dto);
        }
        var (ok, error) = await _api.CreateUser(dto);
        if (!ok)
        {
            TempData["Error"] = error;
            ViewBag.Restaurants = (await _api.GetRestaurants(1, 200))?.Data ?? new();
            return View(dto);
        }
        TempData["Success"] = "User created successfully";
        return RedirectToAction(nameof(Index), new { role = dto.Role });
    }

    public async Task<IActionResult> Edit(int id)
    {
        SetTitle("Users_Edit");
        var user = await _api.GetUser(id);
        if (user == null) return NotFound();
        ViewBag.Restaurants = (await _api.GetRestaurants(1, 200))?.Data ?? new();
        ViewBag.UserId = id;
        return View(new UpdateUserDto
        {
            FullName = user.FullName,
            Phone = user.Phone,
            Address = user.Address,
            Role = user.Role,
            IsActive = user.IsActive,
            RestaurantId = user.RestaurantId
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, UpdateUserDto dto)
    {
        var (ok, error) = await _api.UpdateUser(id, dto);
        if (!ok)
        {
            TempData["Error"] = error;
            ViewBag.Restaurants = (await _api.GetRestaurants(1, 200))?.Data ?? new();
            ViewBag.UserId = id;
            return View(dto);
        }
        TempData["Success"] = "User updated";
        return RedirectToAction(nameof(Index), new { role = dto.Role });
    }

    [HttpPost]
    public async Task<IActionResult> ToggleActive(int id, string? role)
    {
        var (ok, error) = await _api.ToggleUserActive(id);
        TempData[ok ? "Success" : "Error"] = ok ? "User status updated" : error;
        return RedirectToAction(nameof(Index), new { role });
    }
}
