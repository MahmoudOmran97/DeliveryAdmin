using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryAdmin.Controllers;

[Authorize]
public class CustomersController : Controller
{
    public IActionResult Index(string? role, int page = 1) =>
        RedirectToAction("Index", "Users", new { role, page });
}
