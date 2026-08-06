using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Globalization;
using DeliveryAdmin.Resources;

var builder = WebApplication.CreateBuilder(args);

// ✅ Fix: ResourcesPath يجب يكون "" مش "Resources"
// لأن الـ namespace هو DeliveryAdmin.Resources.SharedResource
// .NET بيشيل الـ RootNamespace (DeliveryAdmin) → يبقى "Resources.SharedResource"
// لو ResourcesPath = "Resources" → يبقى المسار "Resources/Resources/SharedResource" ❌ (مضاعف!)
// لو ResourcesPath = "" → يبقى المسار "Resources/SharedResource" ✅
builder.Services.AddLocalization(options => options.ResourcesPath = "");

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<DeliveryAdmin.Filters.RestaurantOwnerScopeFilter>();
})
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (_, factory) => factory.Create(typeof(SharedResource));
    });

// ✅ FIX: من غير الإعداد ده، مفاتيح تشفير الكوكي (Data Protection Keys) بتتخزن
// بشكل مؤقت افتراضيًا. على أي hosting شير زي runasp.net، أي App Pool recycle أو
// نشر جديد ممكن يغيّر المفاتيح دي، فكل الكوكيز (حتى لو لسه صالحة لـ 7 أيام)
// بتبقى غير قابلة لفك التشفير → المستخدم بيتعمله logout فوري من غير أي سبب ظاهر.
// الحل: نخزّن المفاتيح في مجلد ثابت جوا App_Data عشان تفضل موجودة بين كل الـ recycles.
var keysFolder = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "Keys");
Directory.CreateDirectory(keysFolder);
builder.Services.AddDataProtection()
    .SetApplicationName("TawseelaAdmin")
    .PersistKeysToFileSystem(new DirectoryInfo(keysFolder));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(7);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpClient("DeliveryAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]!.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<DeliveryAdmin.Services.ApiService>();

var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("ar") };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new CookieRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    };
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

// ✅ الترتيب الصحيح: Session → RequestLocalization → Routing → Auth
app.UseSession();

var locOptions = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(locOptions);

// ✅ FIX: لغة الواجهة (UICulture) لازم تفضل منفصلة عن نسق الأرقام (Culture).
// UseRequestLocalization بيحط الاتنين على نفس القيمة (مثلاً "ar" لو المستخدم مبدّل
// اللغة عربي). المشكلة إن ثقافة "ar" بتغيّر نسق تحويل الأرقام (الفاصلة العشرية/الأرقام
// الهندية)، فلما الفورم بيتبعت (POST) وفيه حقول زي DeliveryFee أو MinOrderAmount أو
// Latitude/Longitude، الـ Model Binder بيحاول يحوّل النص اللي جاي من المتصفح (اللي دايمًا
// بيبعته بصيغة إنجليزية زي "150.50") باستخدام ثقافة "ar" → التحويل بيفشل بصمت وبترجع
// القيمة صفر أو مش بتتحدث خالص. نفس المشكلة كانت بتظهر في عرض الأرقام في الفورم برضو.
// الحل: نسيب Culture (نسق الأرقام/التواريخ) على الإنجليزي دايمًا، ونسيب UICulture
// (النصوص المترجمة بس) على اختيار المستخدم.
app.Use(async (context, next) =>
{
    var uiCulture = CultureInfo.CurrentUICulture;
    CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    CultureInfo.CurrentUICulture = uiCulture;
    await next();
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();