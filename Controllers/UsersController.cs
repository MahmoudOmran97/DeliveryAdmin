using DeliveryAdmin.Helpers;
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

    // الوصول لقسم "Users" نفسه (عرض/إضافة عملاء وسائقين وأصحاب محلات) بيتحكم فيه
    // AdminPermissionFilter زي أي قسم تاني. لكن التعامل مع حسابات الأدمن نفسها
    // (عرضها، إنشاء حساب أدمن جديد، تعديل دور/صلاحيات حد لـ Admin) مقفول هنا
    // على السوبر أدمن بس، مهما كانت صلاحيات الأدمن المحدود.
    private bool IsSuperAdmin => User.Claims.Any(c => c.Type == "IsSuperAdmin" && c.Value == "true");

    public async Task<IActionResult> Index(string? role, string? search, int page = 1)
    {
        if (role == "Admin" && !IsSuperAdmin) return Forbid();

        SetTitle("Users_Title");
        var result = await _api.GetUsers(page, 20, role, search);
        var data = result?.Data ?? new();
        // لو الأدمن المحدود بيتصفح "كل المستخدمين" من غير فلتر دور، لازم نشيل
        // حسابات الأدمنز التانيين من القايمة عشان ميشوفهمش أو يلعب فيهم.
        if (!IsSuperAdmin) data = data.Where(u => u.Role != "Admin").ToList();

        ViewBag.Role = role;
        ViewBag.Search = search;
        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling((result?.Total ?? 0) / 20.0);
        ViewBag.Total = result?.Total ?? 0;
        ViewBag.CanManageAdmins = IsSuperAdmin;
        return View(data);
    }

    public async Task<IActionResult> Create()
    {
        SetTitle("Users_Add");
        ViewBag.Restaurants = (await _api.GetRestaurants(1, 200))?.Data ?? new();
        SetPermissionsViewBag();
        return View(new CreateUserDto { Role = "Restaurant" });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserDto dto)
    {
        // مينفعش أدمن محدود ينشئ حساب أدمن (كامل أو محدود) — السوبر أدمن بس.
        // الـ API بيرفض الطلب برضو، ده مجرد منع مبكر يوفر رحلة الطلب.
        if (dto.Role == "Admin" && !IsSuperAdmin)
        {
            TempData["Error"] = "السوبر أدمن بس هو المسموحله يضيف حسابات أدمن";
            return RedirectToAction(nameof(Index));
        }

        dto.Permissions = dto.Role == "Admin" ? AdminPermissions.Serialize(dto.PermissionKeys) : null;
        if (!ModelState.IsValid)
        {
            ViewBag.Restaurants = (await _api.GetRestaurants(1, 200))?.Data ?? new();
            SetPermissionsViewBag();
            return View(dto);
        }
        var (ok, error) = await _api.CreateUser(dto);
        if (!ok)
        {
            TempData["Error"] = error;
            ViewBag.Restaurants = (await _api.GetRestaurants(1, 200))?.Data ?? new();
            SetPermissionsViewBag();
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
        // أدمن محدود مينفعش يفتح صفحة تعديل حساب أدمن تاني خالص، حتى لو عارف الـ id.
        if (user.Role == "Admin" && !IsSuperAdmin) return Forbid();

        ViewBag.Restaurants = (await _api.GetRestaurants(1, 200))?.Data ?? new();
        ViewBag.UserId = id;
        SetPermissionsViewBag();
        return View(new UpdateUserDto
        {
            FullName = user.FullName,
            Phone = user.Phone,
            Address = user.Address,
            Role = user.Role,
            IsActive = user.IsActive,
            RestaurantId = user.RestaurantId,
            IsSuperAdmin = user.IsSuperAdmin,
            Permissions = user.Permissions,
            PermissionKeys = AdminPermissions.Parse(user.Permissions).ToList()
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, UpdateUserDto dto)
    {
        var existing = await _api.GetUser(id);
        if (existing == null) return NotFound();

        bool targetIsOrWillBeAdmin = existing.Role == "Admin" || dto.Role == "Admin";
        if (targetIsOrWillBeAdmin && !IsSuperAdmin)
        {
            TempData["Error"] = "السوبر أدمن بس هو المسموحله يعدّل حسابات/صلاحيات الأدمنز";
            return RedirectToAction(nameof(Index));
        }

        dto.Permissions = dto.Role == "Admin" ? AdminPermissions.Serialize(dto.PermissionKeys) : null;
        var (ok, error) = await _api.UpdateUser(id, dto);
        if (!ok)
        {
            TempData["Error"] = error;
            ViewBag.Restaurants = (await _api.GetRestaurants(1, 200))?.Data ?? new();
            ViewBag.UserId = id;
            SetPermissionsViewBag();
            return View(dto);
        }
        TempData["Success"] = "User updated";
        return RedirectToAction(nameof(Index), new { role = dto.Role });
    }

    [HttpPost]
    public async Task<IActionResult> ToggleActive(int id, string? role)
    {
        if (role == "Admin" && !IsSuperAdmin) return Forbid();

        var (ok, error) = await _api.ToggleUserActive(id);
        TempData[ok ? "Success" : "Error"] = ok ? "User status updated" : error;
        return RedirectToAction(nameof(Index), new { role });
    }

    private void SetPermissionsViewBag()
    {
        ViewBag.CanManageAdmins = IsSuperAdmin;
        ViewBag.PermissionModules = AdminPermissions.All;
        ViewBag.PermissionPresets = AdminPermissions.Presets;
    }
}
