using DeliveryAdmin.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace DeliveryAdmin.Filters;

// ─────────────────────────────────────────────────────────────────────────────
// بيتحكم في وصول حساب "Admin محدود" (IsSuperAdmin = false) للأقسام المختلفة.
// - السوبر أدمن (IsSuperAdmin = true) شغال زي الأول بالظبط، من غير أي قيد.
// - الأدمن المحدود مسموحله بس بالأقسام الموجودة في claim الـ Permissions بتاعته
//   (اتخزنت وقت اللوجن في AuthController)، بالإضافة لصفحات أساسية لازم تفضل
//   متاحة دايمًا (تسجيل الدخول/الخروج، تغيير اللغة، جرس التنبيهات، الداشبورد
//   كصفحة هبوط أساسية بعد اللوجن).
// - قسم "Users" ممكن يتحط في صلاحيات أدمن محدود عشان يدير عملاء/سائقين/أصحاب
//   محلات، لكن التعامل مع حسابات الأدمن نفسها (دور Admin) مقفول جوه
//   UsersController نفسه على السوبر أدمن بس مهما كانت صلاحياته.
// ─────────────────────────────────────────────────────────────────────────────
public class AdminPermissionFilter : IAsyncAuthorizationFilter
{
    private static readonly string[] AlwaysAllowed = { "Auth", "Settings", "Notifications", "Dashboard" };

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true || !user.IsInRole("Admin"))
            return Task.CompletedTask;

        var isSuperAdmin = user.Claims.Any(c => c.Type == "IsSuperAdmin" && c.Value == "true");
        if (isSuperAdmin) return Task.CompletedTask;

        var controller = context.RouteData.Values["controller"]?.ToString() ?? "";

        if (AlwaysAllowed.Contains(controller, StringComparer.OrdinalIgnoreCase))
            return Task.CompletedTask;

        var permissions = AdminPermissions.Parse(user.Claims.FirstOrDefault(c => c.Type == "Permissions")?.Value);
        if (!permissions.Contains(controller, StringComparer.OrdinalIgnoreCase))
        {
            Deny(context);
        }

        return Task.CompletedTask;
    }

    private static void Deny(AuthorizationFilterContext context)
    {
        var tempDataFactory = context.HttpContext.RequestServices.GetService(typeof(ITempDataDictionaryFactory)) as ITempDataDictionaryFactory;
        var tempData = tempDataFactory?.GetTempData(context.HttpContext);
        if (tempData != null) tempData["Error"] = "مالكش صلاحية تدخل على القسم ده. تواصل مع السوبر أدمن.";
        context.Result = new RedirectToActionResult("Index", "Dashboard", null);
    }
}
