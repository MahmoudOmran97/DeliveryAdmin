using DeliveryAdmin.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DeliveryAdmin.Controllers;

public abstract class LocalizedController : Controller
{
    protected readonly IStringLocalizer<SharedResource> L;

    protected LocalizedController(IStringLocalizer<SharedResource> localizer) => L = localizer;

    protected void SetTitle(string key) => ViewData["Title"] = L[key];
}
