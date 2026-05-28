using DeliveryAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryAdmin.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApiService _api;
        public OrdersController(ApiService api) => _api = api;

        public async Task<IActionResult> Index(string? status, int page = 1)
        {
            var result = await _api.GetOrders(page, 20, status);
            var all = result?.Data ?? new();
            ViewBag.Status = status; ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling((result?.Total ?? 0) / 20.0);
            ViewBag.Total = result?.Total ?? 0;
            ViewBag.PendingCount = 0; // Would need separate API call
            return View(all);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _api.GetOrder(id);
            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var (ok, error) = await _api.UpdateOrderStatus(id, status);
            TempData[ok ? "Success" : "Error"] = ok ? $"Order #{id} → {status}" : error;
            return RedirectToAction("Details", new { id });
        }
    }
}
