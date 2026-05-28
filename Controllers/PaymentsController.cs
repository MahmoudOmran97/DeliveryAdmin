using DeliveryAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryAdmin.Controllers
{
    [Authorize]
    public class PaymentsController : Controller
    {
        private readonly ApiService _api;
        public PaymentsController(ApiService api) => _api = api;

        public async Task<IActionResult> Index(int page = 1)
        {
            var result = await _api.GetPayments(page, 20);
            var data = result?.Data ?? new();
            ViewBag.Page = page; ViewBag.TotalPages = (int)Math.Ceiling((result?.Total ?? 0) / 20.0);
            ViewBag.Total = result?.Total ?? 0;
            ViewBag.TotalRevenue = data.Sum(p => p.Amount);
            ViewBag.CashCount = data.Count(p => p.Provider == "Cash");
            ViewBag.CardCount = data.Count(p => p.Provider != "Cash");
            return View(data);
        }
    }
}
