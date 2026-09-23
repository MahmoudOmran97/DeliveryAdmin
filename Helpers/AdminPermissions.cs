namespace DeliveryAdmin.Helpers;

// ─────────────────────────────────────────────────────────────────────────────
// تعريف مركزي لكل "قسم" الأدمن المحدود ممكن ياخد صلاحية عليه. الـ Key هنا هو
// نفسه اسم الـ Controller بالظبط عشان AdminPermissionFilter يقدر يقارن بيه مباشرة.
// لو ضفت Controller جديد للوحة وعايز يتحكم فيه بالصلاحيات، ضيفه هنا وفي الـ Views
// اللي بتعرض الـ checkboxes (Create/Edit للـ Users).
// ─────────────────────────────────────────────────────────────────────────────
public static class AdminPermissions
{
    public record Module(string Key, string LabelResourceKey, string Icon);

    public static readonly Module[] All = new[]
    {
        new Module("Dashboard", "Nav_Dashboard", "📊"),
        new Module("Orders", "Nav_Orders", "📦"),
        new Module("Drivers", "Nav_Drivers", "🛵"),
        new Module("Restaurants", "Nav_Restaurants", "🍽️"),
        new Module("Products", "Nav_Products", "🍕"),
        new Module("Categories", "Nav_Categories", "📂"),
        new Module("Customers", "Users_Customers", "🧑‍🤝‍🧑"),
        new Module("Users", "Nav_Users", "👥"),
        new Module("PharmacyChats", "Nav_PharmacyChats", "💊"),
        new Module("Revenue", "Nav_Revenue", "💰"),
        new Module("Settlements", "Nav_Settlements", "📋"),
        new Module("DeliverySettings", "Nav_DeliveryFee", "🚚"),
        new Module("Payments", "Nav_Payments", "💳"),
        new Module("Ratings", "Nav_Ratings", "⭐"),
        new Module("Coupons", "Nav_Coupons", "🎟️"),
        new Module("Deals", "Nav_Deals", "🔥"),
        new Module("Banners", "Nav_Banners", "🖼️"),
        new Module("SupportChats", "Sidebar_SupportChats", "💬"),
        new Module("Complaints", "Sidebar_Complaints", "📝"),
        new Module("AiSettings", "Sidebar_AiSettings", "🤖"),
        new Module("SiteLinks", "Sidebar_SiteLinks", "🔗"),
        new Module("Notifications", "Notif_Title", "🔔"),
    };

    public static string[] AllKeys => All.Select(m => m.Key).ToArray();

    // مجموعات جاهزة (Presets) بتتحط للأدمن الجديد بضغطة واحدة، وبرضو ممكن يتعدل
    // بعدها يدوي بالـ checkboxes. دي مجرد اختصار في الواجهة، مش نوع تاني من الأدوار.
    public static Dictionary<string, string[]> Presets => new()
    {
        ["Support"] = new[] { "Dashboard", "SupportChats", "Complaints", "PharmacyChats", "Customers", "Notifications", "Ratings" },
        ["Finance"] = new[] { "Dashboard", "Revenue", "Settlements", "Payments", "DeliverySettings" },
        ["Operations"] = new[] { "Dashboard", "Orders", "Drivers", "Restaurants", "Products", "Categories", "Users" },
        ["All"] = AllKeys,
    };

    public static string[] Parse(string? permissions) =>
        string.IsNullOrWhiteSpace(permissions)
            ? Array.Empty<string>()
            : permissions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    public static string Serialize(IEnumerable<string> keys) =>
        string.Join(',', keys.Where(k => AllKeys.Contains(k)).Distinct());
}
