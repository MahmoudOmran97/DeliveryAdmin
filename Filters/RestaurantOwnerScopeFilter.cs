using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DeliveryAdmin.Filters;

// ─────────────────────────────────────────────────────────────────────────────
// أي حساب بدوره "Restaurant" (صاحب صيدلية/مطعم/محل) مسموحله بس بـ MyStore وAuth.
// جوه MyStore نفسه كل حاجة متفلترة تلقائي على المحل بتاعه بس (مش ريدايركت
// على صفحة واحدة تابتة زي ما كان قبل كده).
// ─────────────────────────────────────────────────────────────────────────────
public class RestaurantOwnerScopeFilter : IAsyncAuthorizationFilter
{
    // Pharmacy لسه متسيبة عشان شات الروشتة الحالي يفضل شغال زي ما هو
    private static readonly string[] AllowedControllers = { "MyStore", "Pharmacy", "Auth" };

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated == true && user.IsInRole("Restaurant"))
        {
            var controller = context.RouteData.Values["controller"]?.ToString() ?? "";
            if (!AllowedControllers.Contains(controller, StringComparer.OrdinalIgnoreCase))
            {
                context.Result = new RedirectToActionResult("Index", "MyStore", null);
            }
        }

        return Task.CompletedTask;
    }
}
