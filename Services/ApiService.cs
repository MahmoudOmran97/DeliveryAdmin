using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DeliveryAdmin.Models;

namespace DeliveryAdmin.Services
{
    public class ApiService
    {
        private readonly IHttpClientFactory _factory;
        private readonly IHttpContextAccessor _ctx;
        private static readonly JsonSerializerOptions _opts = new() { PropertyNameCaseInsensitive = true };

        public ApiService(IHttpClientFactory factory, IHttpContextAccessor ctx)
        {
            _factory = factory;
            _ctx = ctx;
        }

        private HttpClient Client()
        {
            var client = _factory.CreateClient("DeliveryAPI");
            var token = _ctx.HttpContext?.Session.GetString("JWT");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        private async Task<T?> Get<T>(string path)
        {
            try
            {
                var res = await Client().GetAsync(path);
                if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _ctx.HttpContext?.Session.Clear();
                    return default;
                }
                if (!res.IsSuccessStatusCode) return default;
                var json = await res.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json, _opts);
            }
            catch (Exception)
            {
                return default;
            }
        }

        private async Task<(bool ok, string? error, T? data)> Post<T>(string path, object body)
        {
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                var res = await Client().PostAsync(path, content);
                var json = await res.Content.ReadAsStringAsync();
                if (!res.IsSuccessStatusCode)
                {
                    try
                    {
                        var err = JsonSerializer.Deserialize<JsonElement>(json, _opts);
                        // 1) Custom API error: { "message": "..." }
                        if (err.TryGetProperty("message", out var msg) && msg.GetString() is string s && !string.IsNullOrEmpty(s))
                            return (false, s, default);
                        // 2) ASP.NET [ApiController] validation error: { "errors": { "Field": ["msg"] }, "title": "..." }
                        if (err.TryGetProperty("errors", out var errs))
                        {
                            var msgs = errs.EnumerateObject()
                                .SelectMany(p => p.Value.EnumerateArray().Select(v => v.GetString()))
                                .Where(v => v != null)
                                .ToList();
                            if (msgs.Any()) return (false, string.Join(" | ", msgs!), default);
                        }
                        if (err.TryGetProperty("title", out var title)) return (false, title.GetString(), default);
                        return (false, res.ReasonPhrase, default);
                    }
                    catch { return (false, res.ReasonPhrase, default); }
                }
                return (true, null, JsonSerializer.Deserialize<T>(json, _opts));
            }
            catch (Exception ex)
            {
                return (false, ex.Message, default);
            }
        }

        private async Task<(bool ok, string? error)> Put(string path, object? body = null)
        {
            HttpContent? content = body != null ? new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json") : null;
            var res = await Client().PutAsync(path, content);
            var json = await res.Content.ReadAsStringAsync();
            if (!res.IsSuccessStatusCode)
            {
                try
                {
                    var err = JsonSerializer.Deserialize<JsonElement>(json, _opts);
                    if (err.TryGetProperty("message", out var msg) && msg.GetString() is string s && !string.IsNullOrEmpty(s))
                        return (false, s);
                    if (err.TryGetProperty("errors", out var errs))
                    {
                        var msgs = errs.EnumerateObject()
                            .SelectMany(p => p.Value.EnumerateArray().Select(v => v.GetString()))
                            .Where(v => v != null).ToList();
                        if (msgs.Any()) return (false, string.Join(" | ", msgs!));
                    }
                    if (err.TryGetProperty("title", out var title)) return (false, title.GetString());
                    return (false, res.ReasonPhrase);
                }
                catch { return (false, res.ReasonPhrase); }
            }
            return (true, null);
        }

        private async Task<(bool ok, string? error)> Delete(string path)
        {
            var res = await Client().DeleteAsync(path);
            return res.IsSuccessStatusCode ? (true, null) : (false, res.ReasonPhrase);
        }

        // ── Auth ──────────────────────────────────────────────────────────
        public async Task<(bool ok, string? error, AuthResponse? data)> Login(LoginDto dto) => await Post<AuthResponse>("auth/login", dto);
        public async Task<(bool ok, string? error, AuthResponse? data)> Register(RegisterDto dto) => await Post<AuthResponse>("auth/register", dto);

        // ── Restaurants ───────────────────────────────────────────────────
        public async Task<PagedResult<RestaurantDto>?> GetRestaurants(int page = 1, int size = 20, string? search = null, bool? isOpen = null)
        {
            var q = $"restaurants?page={page}&pageSize={size}";
            if (!string.IsNullOrEmpty(search)) q += $"&search={Uri.EscapeDataString(search)}";
            if (isOpen.HasValue) q += $"&isOpen={isOpen.Value.ToString().ToLower()}";
            return await Get<PagedResult<RestaurantDto>>(q);
        }
        public async Task<RestaurantDto?> GetRestaurant(int id) => await Get<RestaurantDto>($"restaurants/{id}");
        public async Task<(bool ok, string? error)> CreateRestaurant(CreateRestaurantDto dto) { var r = await Post<object>("restaurants", dto); return (r.ok, r.error); }
        public async Task<(bool ok, string? error)> UpdateRestaurant(int id, UpdateRestaurantDto dto) => await Put($"restaurants/{id}/desktop-update", dto);
        public async Task<(bool ok, string? error)> ToggleRestaurant(int id) => await Put($"restaurants/{id}/toggle-status");

        // ── Categories ────────────────────────────────────────────────────
        public async Task<List<CategoryDto>?> GetCategories(int restaurantId) => await Get<List<CategoryDto>>($"categories/restaurant/{restaurantId}");
        public async Task<CategoryDto?> GetCategory(int id) => await Get<CategoryDto>($"categories/{id}");
        public async Task<(bool ok, string? error)> CreateCategory(CreateCategoryDto dto) { var r = await Post<object>("categories", dto); return (r.ok, r.error); }
        public async Task<(bool ok, string? error)> UpdateCategory(int id, UpdateCategoryDto dto) => await Put($"categories/{id}", dto);
        public async Task<(bool ok, string? error)> DeleteCategory(int id) => await Delete($"categories/{id}");

        // ── Products ──────────────────────────────────────────────────────
        public async Task<PagedResult<ProductDto>?> SearchProducts(string q = "", int? restaurantId = null, int page = 1, int size = 50, int? categoryId = null, bool? isAvailable = null)
        {
            var url = $"products/admin?q={Uri.EscapeDataString(q ?? "")}&page={page}&pageSize={size}";
            if (restaurantId.HasValue) url += $"&restaurantId={restaurantId}";
            if (categoryId.HasValue)   url += $"&categoryId={categoryId}";
            if (isAvailable.HasValue)  url += $"&isAvailable={isAvailable.Value.ToString().ToLower()}";
            return await Get<PagedResult<ProductDto>>(url);
        }
        public async Task<ProductDto?> GetProduct(int id) => await Get<ProductDto>($"products/{id}");
        public async Task<(bool ok, string? error, int? id)> CreateProductWithId(CreateProductDto dto)
        {
            var r = await Post<CreatedIdResult>("products", dto);
            return (r.ok, r.error, r.data?.Id);
        }
        public async Task<(bool ok, string? error)> CreateProduct(CreateProductDto dto) { var r = await Post<object>("products", dto); return (r.ok, r.error); }
        public async Task<(bool ok, string? error)> UpdateProduct(int id, CreateProductDto dto) => await Put($"products/{id}", dto);
        public async Task<(bool ok, string? error)> ToggleProduct(int id) => await Put($"products/{id}/toggle-availability");
        public async Task<(bool ok, string? error)> DeleteProduct(int id) => await Delete($"products/{id}");
        public async Task<(bool ok, string? error)> SetProductVariants(int id, List<ProductVariantDto> variants) => await Put($"products/{id}/variants", variants);

        // ── Drivers ───────────────────────────────────────────────────────
        public async Task<PagedResult<DriverDto>?> GetDrivers(int page = 1, int size = 50) => await Get<PagedResult<DriverDto>>($"drivers/admin?page={page}&pageSize={size}");
        public async Task<DriverDto?> GetDriver(int id) => await Get<DriverDto>($"drivers/{id}/admin");
        public async Task<(bool ok, string? error)> VerifyDriver(int id) => await Put($"drivers/{id}/verify");
        public async Task<(bool ok, string? error)> UpdateDriver(int id, AdminUpdateDriverDto dto) => await Put($"drivers/{id}/admin-update", dto);

        // ── Users ─────────────────────────────────────────────────────────
        public async Task<PagedResult<UserDto>?> GetUsers(int page = 1, int size = 20, string? role = null)
        {
            var q = $"user/all?page={page}&pageSize={size}";
            if (!string.IsNullOrEmpty(role)) q += $"&role={role}";
            return await Get<PagedResult<UserDto>>(q);
        }
        public async Task<UserDto?> GetUser(int id) => await Get<UserDto>($"user/{id}");
        public async Task<(bool ok, string? error)> CreateUser(CreateUserDto dto) { var r = await Post<object>("user/admin", dto); return (r.ok, r.error); }
        public async Task<(bool ok, string? error)> UpdateUser(int id, UpdateUserDto dto) => await Put($"user/{id}", dto);
        public async Task<(bool ok, string? error)> ToggleUserActive(int id) => await Put($"user/{id}/toggle-active");
        public async Task<(bool ok, string? error)> AssignRestaurantOwner(int userId, int restaurantId) => await Put($"user/{userId}/assign-restaurant/{restaurantId}");

        // ── Orders ────────────────────────────────────────────────────────
        public async Task<PagedResult<OrderDto>?> GetOrders(int page = 1, int size = 20, string? status = null)
        {
            var q = $"orders/admin?page={page}&pageSize={size}";
            if (!string.IsNullOrEmpty(status)) q += $"&status={status}";
            return await Get<PagedResult<OrderDto>>(q);
        }
        public async Task<OrderDto?> GetOrder(int id) => await Get<OrderDto>($"orders/{id}");
        public async Task<(bool ok, string? error)> UpdateOrderStatus(int id, string status) => await Put($"orders/{id}/status", new { status });

        public async Task<SettlementReportDto?> GetSettlements(DateTime? from, DateTime? to, int? driverId = null, int? restaurantId = null)
        {
            var q = "orders/admin/settlements?";
            if (from.HasValue) q += $"from={from.Value:yyyy-MM-dd}&";
            if (to.HasValue) q += $"to={to.Value:yyyy-MM-dd}&";
            if (driverId.HasValue) q += $"driverId={driverId}&";
            if (restaurantId.HasValue) q += $"restaurantId={restaurantId}&";
            return await Get<SettlementReportDto>(q.TrimEnd('&', '?'));
        }

        // ── Payments ──────────────────────────────────────────────────────
        public async Task<PagedResult<PaymentDto>?> GetPayments(int page = 1, int size = 20) => await Get<PagedResult<PaymentDto>>($"payments/admin?page={page}&pageSize={size}");

        // ── Ratings ───────────────────────────────────────────────────────
        public async Task<PagedResult<RatingDto>?> GetRatings(int page = 1, int size = 20) => await Get<PagedResult<RatingDto>>($"ratings/admin?page={page}&pageSize={size}");

        // ── Coupons ───────────────────────────────────────────────────────
        public async Task<List<CouponDto>?> GetCoupons() => await Get<List<CouponDto>>("coupons/admin");
        public async Task<(bool ok, string? error)> CreateCoupon(CreateCouponDto dto) { var r = await Post<object>("coupons", dto); return (r.ok, r.error); }
        public async Task<(bool ok, string? error)> UpdateCoupon(int id, CreateCouponDto dto) => await Put($"coupons/{id}", dto);
        public async Task<(bool ok, string? error)> DeleteCoupon(int id) => await Delete($"coupons/{id}");

        // ── Deals ─────────────────────────────────────────────────────────
        public async Task<List<DealDto>?> GetDeals() => await Get<List<DealDto>>("deals/admin");
        public async Task<(bool ok, string? error)> CreateDeal(CreateDealDto dto) { var r = await Post<object>("deals", dto); return (r.ok, r.error); }
        public async Task<(bool ok, string? error)> UpdateDeal(int id, CreateDealDto dto) => await Put($"deals/{id}", dto);
        public async Task<(bool ok, string? error)> DeleteDeal(int id) => await Delete($"deals/{id}");

        // ── Banners ───────────────────────────────────────────────────────
        public async Task<List<BannerDto>?> GetBanners() => await Get<List<BannerDto>>("banners/admin");
        public async Task<(bool ok, string? error)> CreateBanner(CreateBannerDto dto) { var r = await Post<object>("banners", dto); return (r.ok, r.error); }
        public async Task<(bool ok, string? error)> UpdateBanner(int id, CreateBannerDto dto) => await Put($"banners/{id}", dto);
        public async Task<(bool ok, string? error)> DeleteBanner(int id) => await Delete($"banners/{id}");

        // ── Notifications ─────────────────────────────────────────────────
        public async Task<PagedResult<NotificationDto>?> GetNotifications(int page = 1, int size = 20) => await Get<PagedResult<NotificationDto>>($"notifications?page={page}&pageSize={size}");
        public async Task<(bool ok, string? error, int count)> SendNotification(SendNotificationDto dto)
        {
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
                var res = await Client().PostAsync("notifications/send", content);
                var json = await res.Content.ReadAsStringAsync();
                if (!res.IsSuccessStatusCode)
                {
                    try { var err = JsonSerializer.Deserialize<JsonElement>(json, _opts); return (false, err.TryGetProperty("message", out var m) ? m.GetString() : res.ReasonPhrase, 0); }
                    catch { return (false, res.ReasonPhrase, 0); }
                }
                var doc = JsonSerializer.Deserialize<JsonElement>(json, _opts);
                var count = doc.TryGetProperty("count", out var c) ? c.GetInt32() : 0;
                return (true, null, count);
            }
            catch (Exception ex) { return (false, ex.Message, 0); }
        }
    }
}