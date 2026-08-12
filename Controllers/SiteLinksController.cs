using DeliveryAdmin.Models;
using DeliveryAdmin.Resources;
using DeliveryAdmin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DeliveryAdmin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SiteLinksController : LocalizedController
    {
        private readonly ApiService _api;

        public SiteLinksController(
            ApiService api,
            IStringLocalizer<SharedResource> localizer) : base(localizer)
        {
            _api = api;
        }

        public async Task<IActionResult> Index()
        {
            SetTitle("SiteLinks_Title");
            var links = await _api.GetSiteLinksAdmin() ?? new List<SiteLinkAdminDto>();
            return View(links.OrderBy(x => x.SortOrder).ThenBy(x => x.Key).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(
            string key,
            string? title,
            string? url,
            string? icon,
            bool isActive,
            int sortOrder)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                TempData["Error"] = L["SiteLinks_ValidationKey"].Value;
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(url) ||
                !Uri.TryCreate(url.Trim(), UriKind.Absolute, out var parsedUrl) ||
                (parsedUrl.Scheme != Uri.UriSchemeHttp && parsedUrl.Scheme != Uri.UriSchemeHttps))
            {
                TempData["Error"] = L["SiteLinks_ValidationUrl"].Value;
                return RedirectToAction(nameof(Index));
            }

            var (ok, error) = await _api.UpdateSiteLink(key.Trim(), new UpdateSiteLinkRequest
            {
                Title = title?.Trim() ?? string.Empty,
                Url = url.Trim(),
                Icon = string.IsNullOrWhiteSpace(icon) ? null : icon.Trim(),
                IsActive = isActive,
                SortOrder = sortOrder
            });

            TempData[ok ? "Success" : "Error"] = ok
                ? L["SiteLinks_SaveSuccess"].Value
                : (error ?? L["SiteLinks_SaveError"].Value);

            return RedirectToAction(nameof(Index));
        }
    }
}
