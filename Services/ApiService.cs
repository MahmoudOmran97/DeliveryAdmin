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
            var res = await Client().GetAsync(path);
            if (!res.IsSuccessStatusCode) return default;
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, _opts);
        }

        private async Task<(bool ok, string? error, T? data)> Post<T>(string path, object body)
        {
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var res = await Client().PostAsync(path, content);
            var json = await res.Content.ReadAsStringAsync();
            if (!res.IsSuccessStatusCode)
            {
                try { var err = JsonSerializer.Deserialize<JsonElement>(json, _opts); return (false, err.TryGetProperty("message", out var m) ? m.GetString() : res.ReasonPhrase, default); }
                catch { return (false, res.ReasonPhrase, default); }
            }
            return (true, null, JsonSerializer.Deserialize<T>(json, _opts));
        }

        private async Task<(bool ok, string? error)> Put(string path, object? body = null)
        {
            HttpContent? content = body != null ? new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json") : null;
            var res = await Client().PutAsync(path, content);
            var json = await res.Content.ReadAsStringAsync();
            if (!res.IsSuccessStatusCode)
            {
                try { var err = JsonSerializer.Deserialize<JsonElement>(json, _opts); return (false, err.TryGetProperty("message", out var m) ? m.GetString() : res.ReasonPhrase); }
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
        public async Task<PagedResult<ProductDto>?> SearchProducts(string q = "", int? restaurantId = null, int page = 1, int size = 50)
        {
            var url = $"products/search?q={Uri.EscapeDataString(q)}&page={page}&pageSize={size}";
            if (restaurantId.HasValue) url += $"&restaurantId={restaurantId}";
            return await Get<PagedResult<ProductDto>>(url);
        }
        public async Task<ProductDto?> GetProduct(int id) => await Get<ProductDto>($"products/{id}");
        public async Task<(bool ok, string? error)> CreateProduct(CreateProductDto dto) { var r = await Post<object>("products", dto); return (r.ok, r.error); }
        public async Task<(bool ok, string? error)> UpdateProduct(int id, CreateProductDto dto) => await Put($"products/{id}", dto);
        public async Task<(bool ok, string? error)> ToggleProduct(int id) => await Put($"products/{id}/toggle-availability");
        public async Task<(bool ok, string? error)> DeleteProduct(int id) => await Delete($"products/{id}");

        // ── Drivers ───────────────────────────────────────────────────────
        public async Task<PagedResult<DriverDto>?> GetDrivers(int page = 1, int size = 50) => await Get<PagedResult<DriverDto>>($"drivers/admin?page={page}&pageSize={size}");
        public async Task<(bool ok, string? error)> VerifyDriver(int id) => await Put($"drivers/{id}/verify");

        // ── Users ─────────────────────────────────────────────────────────
        public async Task<PagedResult<UserDto>?> GetUsers(int page = 1, int size = 20, string? role = null)
        {
            var q = $"user/all?page={page}&pageSize={size}";
            if (!string.IsNullOrEmpty(role)) q += $"&role={role}";
            return await Get<PagedResult<UserDto>>(q);
        }

        // ── Orders ────────────────────────────────────────────────────────
        public async Task<PagedResult<OrderDto>?> GetOrders(int page = 1, int size = 20, string? status = null)
        {
            var q = $"orders/admin?page={page}&pageSize={size}";
            if (!string.IsNullOrEmpty(status)) q += $"&status={status}";
            return await Get<PagedResult<OrderDto>>(q);
        }
        public async Task<OrderDto?> GetOrder(int id) => await Get<OrderDto>($"orders/{id}");
        public async Task<(bool ok, string? error)> UpdateOrderStatus(int id, string status) => await Put($"orders/{id}/status", new { status });

        // ── Payments ──────────────────────────────────────────────────────
        public async Task<PagedResult<PaymentDto>?> GetPayments(int page = 1, int size = 20) => await Get<PagedResult<PaymentDto>>($"payments/admin?page={page}&pageSize={size}");

        // ── Ratings ───────────────────────────────────────────────────────
        public async Task<PagedResult<RatingDto>?> GetRatings(int page = 1, int size = 20) => await Get<PagedResult<RatingDto>>($"ratings/admin?page={page}&pageSize={size}");

        // ── Notifications ─────────────────────────────────────────────────
        public async Task<PagedResult<NotificationDto>?> GetNotifications(int page = 1, int size = 20) => await Get<PagedResult<NotificationDto>>($"notifications?page={page}&pageSize={size}");
        public async Task<(bool ok, string? error)> SendNotification(object dto) { var r = await Post<object>("notifications/send", dto); return (r.ok, r.error); }
    }
}
