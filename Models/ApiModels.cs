namespace DeliveryAdmin.Models
{
    // ── Auth ──────────────────────────────────
    public class LoginDto { public string Email { get; set; } = ""; public string Password { get; set; } = ""; }
    public class RegisterDto
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "Admin";
    }
    public class AuthResponse { public string? Token { get; set; } public int Id { get; set; } public string? FullName { get; set; } public string? Email { get; set; } public string? Role { get; set; } }

    // رقم واتساب الأدمن اللي بيظهر لصاحب المحل في زر "تواصل مع الدعم"
    public class SupportContactDto { public int Id { get; set; } public string? Phone { get; set; } public string? FullName { get; set; } public string? ProfileImageUrl { get; set; } }

    // ── Restaurant ────────────────────────────
    public class RestaurantDto
    {
        public int Id { get; set; } public string Name { get; set; } = ""; public string? Description { get; set; }
        public string Address { get; set; } = ""; public double Latitude { get; set; } public double Longitude { get; set; }
        public string? Phone { get; set; } public string? ImageUrl { get; set; } public string? CoverImageUrl { get; set; }
        public double Rating { get; set; } public int TotalRatings { get; set; }
        public decimal DeliveryFee { get; set; } public decimal MinOrderAmount { get; set; }
        public int EstimatedTime { get; set; } public bool IsOpen { get; set; } public bool IsActive { get; set; }
        public int? OwnerUserId { get; set; } public string? OwnerName { get; set; } public string? OwnerEmail { get; set; }
        public string StoreType { get; set; } = "Restaurant";
        public DateTime CreatedAt { get; set; }
    }
    public class CreateRestaurantDto
    {
        public string Name { get; set; } = ""; public string? Description { get; set; } public string Address { get; set; } = "";
        public double Latitude { get; set; } public double Longitude { get; set; } public string? Phone { get; set; }
        public decimal DeliveryFee { get; set; } public decimal MinOrderAmount { get; set; } public int EstimatedTime { get; set; } = 30;
        public string? ImageUrl { get; set; } public string? CoverImageUrl { get; set; }
        public int? OwnerUserId { get; set; }
        public string StoreType { get; set; } = "Restaurant";
    }
    public class UpdateRestaurantDto : CreateRestaurantDto { public bool IsOpen { get; set; } }

    // خيارات نوع المحل المتاحة في المنصة
    public static class StoreTypes
    {
        public static readonly (string Value, string LabelAr, string Emoji)[] All = new[]
        {
            ("Restaurant", "مطعم", "🍽️"),
            ("Pharmacy", "صيدلية", "💊"),
            ("Grocery", "بقالة", "🛒"),
            ("Supermarket", "سوبر ماركت", "🏬"),
            ("Vegetables", "خضار وفاكهة", "🥦"),
            ("Drinks", "مشروبات", "🥤"),
            ("Accessories", "إكسسوارات", "🎁"),
        };
    }

    // ── Category ──────────────────────────────
    public class CategoryDto { public int Id { get; set; } public string Name { get; set; } = ""; public string? ImageUrl { get; set; } public int SortOrder { get; set; } public int RestaurantId { get; set; } public int ProductCount { get; set; } }
    public class CreateCategoryDto { public int RestaurantId { get; set; } public string Name { get; set; } = ""; public string? ImageUrl { get; set; } }
    public class UpdateCategoryDto { public string Name { get; set; } = ""; public string? ImageUrl { get; set; } public int SortOrder { get; set; } }

    // ── Product ───────────────────────────────
    public class ProductDto
    {
        public int Id { get; set; } public string Name { get; set; } = ""; public string? Description { get; set; }
        public decimal Price { get; set; } public decimal? DiscountedPrice { get; set; } public string? ImageUrl { get; set; }
        public int PreparationTime { get; set; } public int? Calories { get; set; } public bool IsAvailable { get; set; }
        public string? CategoryName { get; set; } public int? RestaurantId { get; set; } public string? RestaurantName { get; set; }
        public object? Category { get; set; }
        public List<ProductVariantDto>? Variants { get; set; }
    }
    public class CreateProductDto
    {
        public int CategoryId { get; set; } public string Name { get; set; } = ""; public string? Description { get; set; }
        public decimal Price { get; set; } public decimal? DiscountedPrice { get; set; } public string? ImageUrl { get; set; }
        public int PreparationTime { get; set; } = 15; public int? Calories { get; set; }
    }

    // منتج بأكتر من سعر (مقاس وسط/كبير، شريط/علبة، إلخ)
    public class ProductVariantDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int SortOrder { get; set; }
    }

    // ── Driver ────────────────────────────────
    public class DriverDto
    {
        public int Id { get; set; } public int UserId { get; set; } public string? UserName { get; set; } public string? FullName { get; set; } public string? Email { get; set; }
        public string VehicleType { get; set; } = ""; public string LicensePlate { get; set; } = ""; public string? NationalId { get; set; }
        public double Rating { get; set; } public int TotalRatings { get; set; } public int TotalDeliveries { get; set; }
        public bool IsOnline { get; set; } public bool IsAvailable { get; set; } public bool IsVerified { get; set; }
        public double? CurrentLatitude { get; set; } public double? CurrentLongitude { get; set; } public DateTime JoinedAt { get; set; }
        public string? Phone { get; set; }
    }

    public class AdminUpdateDriverDto
    {
        public string? VehicleType { get; set; }
        public string? LicensePlate { get; set; }
        public string? NationalId { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsAvailable { get; set; }
    }

    // سواق متصل بمحل معين (بورتال صاحب المحل — MyStore/Drivers)
    public class StoreDriverDto
    {
        public int Id { get; set; } public string? UserName { get; set; } public string? FullName { get; set; }
        public string? Phone { get; set; } public string VehicleType { get; set; } = ""; public string LicensePlate { get; set; } = "";
        public double Rating { get; set; } public int TotalDeliveries { get; set; }
        public bool IsOnline { get; set; } public bool IsAvailable { get; set; }
        public double? CurrentLatitude { get; set; } public double? CurrentLongitude { get; set; }
        public int DeliveriesForThisStore { get; set; }
        public int? CurrentOrderId { get; set; } public string? CurrentOrderStatus { get; set; }
    }
    public class StoreDriversResult { public int Total { get; set; } public List<StoreDriverDto> Data { get; set; } = new(); }

    // ── User ──────────────────────────────────
    public class UserDto
    {
        public int Id { get; set; } public string FullName { get; set; } = ""; public string Email { get; set; } = "";
        public string Phone { get; set; } = ""; public string Role { get; set; } = ""; public string? Address { get; set; }
        public string? ProfileImageUrl { get; set; } public bool IsActive { get; set; } public DateTime CreatedAt { get; set; }
        public int? RestaurantId { get; set; } public string? RestaurantName { get; set; }
    }

    public class CreateUserDto
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "Customer";
        public string? Address { get; set; }
        public int? RestaurantId { get; set; }
        // بيانات الطيار (تُستخدم فقط لما Role = Driver)
        public string? VehicleType { get; set; }
        public string? LicensePlate { get; set; }
        public string? NationalId { get; set; }
    }

    public class UpdateUserDto
    {
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Role { get; set; }
        public string? Password { get; set; }
        public bool? IsActive { get; set; }
        public int? RestaurantId { get; set; }
    }

    public class SendNotificationDto
    {
        public int? UserId { get; set; }
        public string? Role { get; set; }
        public string Title { get; set; } = "";
        public string Body { get; set; } = "";
        public string? Type { get; set; } = "General";
        public int? OrderId { get; set; }

        // نفس نظام التوجيه بتاع البانرات: "order/5", "restaurant/12", "chat/5",
        // "category/food", أو رابط خارجي https://... — فاضي = من غير توجيه.
        public string? ActionUrl { get; set; }
    }

    // ── Order ─────────────────────────────────
    public class OrderDto
    {
        public int Id { get; set; } public string Status { get; set; } = ""; public string? CustomerName { get; set; } public string? RestaurantName { get; set; }
        public decimal SubTotal { get; set; } public decimal DeliveryFee { get; set; } public decimal Discount { get; set; } public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = ""; public string PaymentStatus { get; set; } = "";
        public string DeliveryAddress { get; set; } = ""; public string? DeliveryNotes { get; set; }
        public int? ItemCount { get; set; } public DateTime CreatedAt { get; set; } public DateTime? DeliveredAt { get; set; }
        public RestaurantDto? Restaurant { get; set; } public DriverInfo? Driver { get; set; }
        public List<OrderItemDto>? Items { get; set; }
    }
    public class OrderItemDto { public int Id { get; set; } public string ProductName { get; set; } = ""; public int Quantity { get; set; } public decimal UnitPrice { get; set; } public decimal TotalPrice { get; set; } public string? Notes { get; set; } }
    public class DriverInfo { public int Id { get; set; } public string? Name { get; set; } public string? Phone { get; set; } public double Rating { get; set; } }
    public class UpdateStatusDto { public string Status { get; set; } = ""; }

    // ── Payment ───────────────────────────────
    public class PaymentDto { public int Id { get; set; } public int OrderId { get; set; } public string Provider { get; set; } = ""; public decimal Amount { get; set; } public string Currency { get; set; } = "EGP"; public string Status { get; set; } = ""; public string? TransactionId { get; set; } public DateTime? PaidAt { get; set; } public DateTime CreatedAt { get; set; } public string? RestaurantName { get; set; } }

    // ── Rating ────────────────────────────────
    public class RatingDto { public int Id { get; set; } public string? CustomerName { get; set; } public string? RestaurantName { get; set; } public string? DriverName { get; set; } public int RestaurantRating { get; set; } public int? FoodRating { get; set; } public double? DriverRating { get; set; } public string? Comment { get; set; } public DateTime CreatedAt { get; set; } }
    public class StoreRatingsResult { public int Total { get; set; } public int Page { get; set; } public int PageSize { get; set; } public double AvgRestaurant { get; set; } public double AvgFood { get; set; } public List<RatingDto> Data { get; set; } = new(); }

    // ── Notification ──────────────────────────
    public class NotificationDto { public int Id { get; set; } public string Title { get; set; } = ""; public string Body { get; set; } = ""; public string Type { get; set; } = ""; public bool IsRead { get; set; } public int? OrderId { get; set; } public string? ActionUrl { get; set; } public DateTime CreatedAt { get; set; } }

    // ── Coupons ───────────────────────────────
    public class CouponDto
    {
        public int Id { get; set; } public string Code { get; set; } = ""; public string Title { get; set; } = ""; public string? Description { get; set; }
        public string DiscountType { get; set; } = "Fixed"; public decimal DiscountValue { get; set; }
        public decimal? MinOrderAmount { get; set; } public decimal? MaxDiscount { get; set; }
        public int? RestaurantId { get; set; } public string? RestaurantName { get; set; }
        public int? UsageLimit { get; set; } public int UsedCount { get; set; }
        public bool IsActive { get; set; } = true; public DateTime? ExpiresAt { get; set; } public DateTime CreatedAt { get; set; }
        public bool IsExpired { get; set; }
    }
    public class CreateCouponDto
    {
        public string Code { get; set; } = ""; public string Title { get; set; } = ""; public string? Description { get; set; }
        public string DiscountType { get; set; } = "Fixed"; public decimal DiscountValue { get; set; }
        public decimal? MinOrderAmount { get; set; } public decimal? MaxDiscount { get; set; }
        public int? RestaurantId { get; set; } public int? UsageLimit { get; set; }
        public bool IsActive { get; set; } = true; public DateTime? ExpiresAt { get; set; }
    }

    // ── Deals ─────────────────────────────────
    public class DealDto
    {
        public int Id { get; set; } public string Title { get; set; } = ""; public string? Description { get; set; } public string? ImageUrl { get; set; }
        public int? RestaurantId { get; set; } public string? RestaurantName { get; set; }
        public int? ProductId { get; set; } public string? ProductName { get; set; }
        public decimal? OriginalPrice { get; set; } public decimal? DiscountedPrice { get; set; } public int? DiscountPercent { get; set; }
        public string? BadgeText { get; set; } public string? BadgeColor { get; set; }
        public bool IsActive { get; set; } = true; public int SortOrder { get; set; } public DateTime? ExpiresAt { get; set; } public DateTime CreatedAt { get; set; }
        public bool IsExpired { get; set; }
    }
    public class CreateDealDto
    {
        public string Title { get; set; } = ""; public string? Description { get; set; } public string? ImageUrl { get; set; }
        public int? RestaurantId { get; set; } public int? ProductId { get; set; }
        public decimal? OriginalPrice { get; set; } public decimal? DiscountedPrice { get; set; } public int? DiscountPercent { get; set; }
        public string? BadgeText { get; set; } public string? BadgeColor { get; set; }
        public bool IsActive { get; set; } = true; public int SortOrder { get; set; } public DateTime? ExpiresAt { get; set; }
    }

    // ── Banners ───────────────────────────────
    public class BannerDto
    {
        public int Id { get; set; } public string Title { get; set; } = ""; public string? SubTitle { get; set; } public string? ImageUrl { get; set; }
        public string? ActionUrl { get; set; } public string? BackgroundColor { get; set; } public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true; public DateTime? StartsAt { get; set; } public DateTime? EndsAt { get; set; } public DateTime CreatedAt { get; set; }
    }
    public class CreateBannerDto
    {
        public string Title { get; set; } = ""; public string? SubTitle { get; set; } public string? ImageUrl { get; set; }
        public string? ActionUrl { get; set; } public string? BackgroundColor { get; set; } public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true; public DateTime? StartsAt { get; set; } public DateTime? EndsAt { get; set; }
    }

    // ── Paged ─────────────────────────────────
    public class PagedResult<T> { public int Total { get; set; } public int Page { get; set; } public int PageSize { get; set; } public List<T> Data { get; set; } = new(); }

    // نتيجة إنشاء عنصر جديد (فيها الـ Id بس)
    public class CreatedIdResult { public int Id { get; set; } public string? Name { get; set; } }

    // ── Settlements ───────────────────────────
    public class SettlementReportDto
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public SettlementSummaryDto Summary { get; set; } = new();
        public List<DriverSettlementDto> Drivers { get; set; } = new();
        public List<RestaurantSettlementDto> Restaurants { get; set; } = new();
    }

    public class SettlementSummaryDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalRestaurantPayout { get; set; }
        public decimal TotalDeliveryFees { get; set; }
        public decimal TotalCashCollected { get; set; }
    }

    public class DriverSettlementDto
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; } = "";
        public int OrderCount { get; set; }
        public decimal CashCollected { get; set; }
        public decimal DeliveryEarnings { get; set; }
        public decimal RestaurantDue { get; set; }
        public int CashOrders { get; set; }
        public int CardOrders { get; set; }
    }

    public class RestaurantSettlementDto
    {
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; } = "";
        public int OrderCount { get; set; }
        public decimal PayoutAmount { get; set; }
        public decimal TotalSales { get; set; }
        public decimal DeliveryFees { get; set; }
        public int CashOrders { get; set; }
        public int CardOrders { get; set; }
    }

    // ── Delivery Settings ─────────────────────
    public class DeliverySettingsDto
    {
        public int Id { get; set; }
        public double FreeRadiusKm { get; set; }
        public decimal ExtraFeePerKm { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
    public class UpdateDeliverySettingsDto
    {
        public double FreeRadiusKm { get; set; }
        public decimal ExtraFeePerKm { get; set; }
    }

    // ── Prescription Chat (بورتال صاحب الصيدلية) ─────────────
    public class PrescriptionRequestDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int RestaurantId { get; set; }
        public string? RestaurantName { get; set; } // بيرجع بس من إندبوينت الأدمن /prescriptionrequests/admin
        public string ImageUrl { get; set; } = "";
        public string? Notes { get; set; }
        public string Status { get; set; } = "Pending"; // Pending/Priced/Confirmed/Ordered/Cancelled
        public decimal? AgreedPrice { get; set; }
        public int? OrderId { get; set; }
        public DateTime CreatedAt { get; set; }

        // ✅ رابط كامل للصورة يشتغل من دومين الأدمن مهما كان الرابط اللي راجع من الـ API نسبي
        public string? FullImageUrl =>
            string.IsNullOrWhiteSpace(ImageUrl) ? null
            : ImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? ImageUrl
            : "https://deliveryappapi.runasp.net" + (ImageUrl.StartsWith("/") ? ImageUrl : "/" + ImageUrl);
    }

    // نتيجة إندبوينت /prescriptionrequests/admin (باجينيشن + items)
    public class PrescriptionAdminResult
    {
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<PrescriptionRequestDto> Items { get; set; } = new();
    }

    public class PrescriptionMessageDto
    {
        public int Id { get; set; }
        public string SenderRole { get; set; } = "Customer"; // Customer/Pharmacy
        public string Message { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    // ── شات الطلب بين العميل والدليفري (للعرض من الأدمن فقط، read-only) ──
    public class OrderChatMessageDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int SenderId { get; set; }
        public string Message { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public string SenderRole { get; set; } = "Unknown"; // Customer/Driver/Unknown
    }

    public class SetPrescriptionPriceRequest
    {
        public decimal Price { get; set; }
    }

    public class SendPrescriptionMessageRequest
    {
        public string Message { get; set; } = "";
    }
}
