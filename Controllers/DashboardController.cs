using DeliveryAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryAdmin.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApiService _api;
        public DashboardController(ApiService api) => _api = api;

        public async Task<IActionResult> Index()
        {
            var orders = await _api.GetOrders(1, 100);
            var restaurants = await _api.GetRestaurants(1, 100);
            var drivers = await _api.GetDrivers(1, 100);

            ViewBag.TotalOrders = orders?.Total ?? 0;
            ViewBag.TotalRestaurants = restaurants?.Total ?? 0;
            ViewBag.TotalDrivers = drivers?.Total ?? 0;
            ViewBag.OnlineDrivers = drivers?.Data?.Count(d => d.IsOnline) ?? 0;

            var allOrders = orders?.Data ?? new();
            ViewBag.PendingOrders = allOrders.Count(o => o.Status == "Pending");
            ViewBag.OnWayOrders = allOrders.Count(o => o.Status == "OnTheWay");
            ViewBag.DeliveredOrders = allOrders.Count(o => o.Status == "Delivered");
            ViewBag.Revenue = allOrders.Where(o => o.Status == "Delivered").Sum(o => o.TotalAmount);
            ViewBag.RecentOrders = allOrders.Take(8).ToList();
            ViewBag.OpenRestaurants = restaurants?.Data?.Count(r => r.IsOpen) ?? 0;

            // Status counts for chart
            var statuses = new[] { "Pending", "Accepted", "Preparing", "ReadyForPickup", "OnTheWay", "Delivered", "Cancelled" };
            ViewBag.StatusCounts = statuses.Select(s => allOrders.Count(o => o.Status == s)).ToArray();

            return View();
        }
    }
}
