using DeliveryAdmin.Resources;
using DeliveryAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DeliveryAdmin.Controllers
{
    [Authorize]
    public class DashboardController : LocalizedController
    {
        private readonly ApiService _api;
        public DashboardController(ApiService api, IStringLocalizer<SharedResource> localizer) : base(localizer) => _api = api;

        public async Task<IActionResult> Index()
        {
            SetTitle("Dashboard_Title");

            // Fetch all data in parallel
            var ordersTask      = _api.GetOrders(1, 100);
            var restaurantsTask = _api.GetRestaurants(1, 100);
            var driversTask     = _api.GetDrivers(1, 100);
            var usersTask       = _api.GetUsers(1, 1);              // total only
            var customersTask   = _api.GetUsers(1, 1, "Customer");  // total only
            var productsTask    = _api.SearchProducts("", null, 1, 1); // total only

            await Task.WhenAll(ordersTask, restaurantsTask, driversTask, usersTask, customersTask, productsTask);

            var orders      = await ordersTask;
            var restaurants = await restaurantsTask;
            var drivers     = await driversTask;
            var users       = await usersTask;
            var customers   = await customersTask;
            var products    = await productsTask;

            // KPI numbers
            ViewBag.TotalOrders      = orders?.Total ?? 0;
            ViewBag.TotalRestaurants = restaurants?.Total ?? 0;
            ViewBag.TotalDrivers     = drivers?.Total ?? 0;
            ViewBag.OnlineDrivers    = drivers?.Data?.Count(d => d.IsOnline) ?? 0;
            ViewBag.TotalUsers       = users?.Total ?? 0;
            ViewBag.TotalCustomers   = customers?.Total ?? 0;
            ViewBag.TotalProducts    = products?.Total ?? 0;
            ViewBag.ActiveUsers      = users?.Data?.Count(u => u.IsActive) ?? 0;

            var allOrders = orders?.Data ?? new();
            ViewBag.PendingOrders   = allOrders.Count(o => o.Status == "Pending");
            ViewBag.OnWayOrders     = allOrders.Count(o => o.Status == "OnTheWay");
            ViewBag.DeliveredOrders = allOrders.Count(o => o.Status == "Delivered");
            ViewBag.Revenue         = allOrders.Where(o => o.Status == "Delivered").Sum(o => o.TotalAmount);
            ViewBag.RecentOrders    = allOrders.Take(8).ToList();
            ViewBag.OpenRestaurants = restaurants?.Data?.Count(r => r.IsOpen) ?? 0;

            // Status counts for chart
            var statuses = new[] { "Pending", "Accepted", "Preparing", "ReadyForPickup", "OnTheWay", "Delivered", "Cancelled" };
            ViewBag.StatusCounts = statuses.Select(s => allOrders.Count(o => o.Status == s)).ToArray();

            return View();
        }
    }
}
