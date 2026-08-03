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
    // Settings و Notifications لازم يبقوا متاحين لصاحب المحل برضو (تغيير اللغة + جرس التنبيهات)،
    // وإلا الفلتر بيقطع الطلب قبل ما الأكشن يتنفذ ويرجّعه MyStore تلقائي (كان ده سبب إن تغيير
    // اللغة لصاحب المحل مكنش بيشتغل، كان بس بيرجعه لصفحة المحل من غير ما يغير الكوكي)
    private static readonly string[] AllowedControllers = { "MyStore", "Pharmacy", "Auth", "Settings", "Notifications" };

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
