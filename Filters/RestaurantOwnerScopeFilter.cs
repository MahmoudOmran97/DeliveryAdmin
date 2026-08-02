using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DeliveryAdmin.Filters;

// ─────────────────────────────────────────────────────────────────────────────
// أي حساب بدوره "Restaurant" (صاحب صيدلية/مطعم) مسموحله بس بـ Pharmacy وAuth.
// أي محاولة يوصل لأي كنترولر تاني (Dashboard، Orders، Users...) بترجّعه على
// بورتاله هو، عشان مايشوفش بيانات المنصة كلها.
// ─────────────────────────────────────────────────────────────────────────────
public class RestaurantOwnerScopeFilter : IAsyncAuthorizationFilter
{
    private static readonly string[] AllowedControllers = { "Pharmacy", "Auth" };

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated == true && user.IsInRole("Restaurant"))
        {
            var controller = context.RouteData.Values["controller"]?.ToString() ?? "";
            if (!AllowedControllers.Contains(controller, StringComparer.OrdinalIgnoreCase))
            {
                context.Result = new RedirectToActionResult("Index", "Pharmacy", null);
            }
        }

        return Task.CompletedTask;
    }
}
