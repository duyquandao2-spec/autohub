using AutoHub.Data;
using AutoHub.Models;
using AutoHub.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AutoHub.Pages.Admin.Vehicles;

[Authorize(Policy = Policies.VehicleWrite)]
public class IndexModel : PageModel
{
    private readonly AutoHubDbContext _db;

    public IndexModel(AutoHubDbContext db) => _db = db;

    public sealed class VehicleRow(int variantId, string name, string slug, string model, string? brand,
        decimal price, string currency, string status, bool available, bool featured, bool newArrival)
    {
        public int VariantId { get; } = variantId;
        public string Name { get; } = name;
        public string Slug { get; } = slug;
        public string Model { get; } = model;
        public string? Brand { get; } = brand;
        public decimal Price { get; } = price;
        public string Currency { get; } = currency;
        public string Status { get; } = status;
        public bool Available { get; } = available;
        public bool Featured { get; } = featured;
        public bool NewArrival { get; } = newArrival;
    }

    public List<VehicleRow> Vehicles { get; set; } = new();
    public int Total { get; private set; }
    public int TotalPages { get; private set; } = 1;

    [FromQuery(Name = "page")]
    public int PageNumber { get; set; } = 1;

    [FromQuery]
    public string? Search { get; set; }

    [FromQuery]
    public string? Status { get; set; }

    public async Task OnGetAsync()
    {
        const int pageSize = 20;
        var query = _db.VehicleVariants.AsNoTracking().Include(v => v.Model).ThenInclude(m => m.Brand).AsQueryable();

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var term = Search.Trim();
            query = query.Where(v => v.VariantName.Contains(term) || v.Model.Name.Contains(term));
        }
        if (!string.IsNullOrWhiteSpace(Status))
            query = query.Where(v => v.Status == Status);

        Total = await query.CountAsync();
        var pageCount = Total == 0 ? 1 : (int)Math.Ceiling((double)Total / pageSize);
        PageNumber = Math.Clamp(PageNumber, 1, pageCount);
        TotalPages = pageCount;

        var rows = await query.OrderBy(v => v.VariantName)
            .Skip((PageNumber - 1) * pageSize).Take(pageSize)
            .Select(v => new VehicleRow(v.VariantId, v.VariantName, v.Slug, v.Model.Name, v.Model.Brand.Name,
                v.BasePrice, v.CurrencyCode, v.Status, v.IsAvailable, v.IsFeatured, v.IsNewArrival))
            .ToListAsync();

        Vehicles = rows;
        ViewData["TotalPages"] = pageCount;
    }

    public async Task<IActionResult> OnPostToggleAvailableAsync(int id)
    {
        var v = await _db.VehicleVariants.FindAsync(id);
        if (v is not null) { v.IsAvailable = !v.IsAvailable; v.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleFeaturedAsync(int id)
    {
        var v = await _db.VehicleVariants.FindAsync(id);
        if (v is not null) { v.IsFeatured = !v.IsFeatured; v.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostNewArrivalAsync(int id)
    {
        var v = await _db.VehicleVariants.FindAsync(id);
        if (v is not null) { v.IsNewArrival = !v.IsNewArrival; v.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var v = await _db.VehicleVariants
            .Include(x => x.Specification)
            .Include(x => x.VariantMedia)
            .Include(x => x.VariantColors)
            .FirstOrDefaultAsync(x => x.VariantId == id);

        if (v != null)
        {
            if (v.Specification != null) _db.VehicleSpecifications.Remove(v.Specification);
            if (v.VariantMedia.Any()) _db.VariantMedia.RemoveRange(v.VariantMedia);
            if (v.VariantColors.Any()) _db.VariantColors.RemoveRange(v.VariantColors);

            _db.VehicleVariants.Remove(v);
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã xóa xe {v.VariantName} thành công.";
        }
        return RedirectToPage();
    }
}