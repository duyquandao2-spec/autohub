using AutoHub.Models;
using AutoHub.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoHub.Pages;

public sealed class IndexModel(IVehicleService vehicleService) : PageModel
{
    private const int PageSize = 8;

    public PaginationResult<Vehicle> PagedResult { get; private set; } = default!;
    public IReadOnlyList<string> Brands { get; private set; } = [];
    public string ActiveBrand { get; private set; } = "ALL";
    public string? SearchQuery { get; private set; }
    public List<int> VisiblePages { get; private set; } = [];

    public void OnGet(string? brand, int p = 1, string? q = null)
    {
        ActiveBrand = string.IsNullOrWhiteSpace(brand) ? "ALL" : brand;
        SearchQuery = string.IsNullOrWhiteSpace(q) ? null : q;
        Brands = vehicleService.GetBrands();
        PagedResult = vehicleService.GetVehiclesPaged(p, PageSize, ActiveBrand, SearchQuery);
        VisiblePages = BuildVisiblePages(PagedResult.CurrentPage, PagedResult.TotalPages);
    }

    private static List<int> BuildVisiblePages(int current, int total)
    {
        if (total <= 0) return [];

        var pages = new List<int>();
        int start = Math.Max(1, current - 2);
        int end = Math.Min(total, current + 2);

        if (start > 1) pages.Add(1);
        if (start > 2) pages.Add(-1);
        for (int i = start; i <= end; i++) pages.Add(i);
        if (end < total - 1) pages.Add(-2);
        if (end < total) pages.Add(total);

        return pages;
    }

    public string BuildPageUrl(int page)
    {
        var routeValues = new Dictionary<string, string?> { ["p"] = page.ToString() };
        if (ActiveBrand != "ALL") routeValues["brand"] = ActiveBrand;
        if (SearchQuery != null) routeValues["q"] = SearchQuery;

        var query = string.Join("&", routeValues
            .Where(kv => kv.Value != null)
            .Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value!)}"));

        return $"/?{query}";
    }
}
