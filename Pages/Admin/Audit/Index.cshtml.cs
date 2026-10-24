using AutoHub.Data;
using AutoHub.Models;
using AutoHub.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AutoHub.Pages.Admin.Audit;

[Authorize(Policy = Policies.AuditView)]
public class IndexModel : PageModel
{
    private readonly AutoHubDbContext _db;

    public IndexModel(AutoHubDbContext db) => _db = db;

    public List<AuditLogEntity> Logs { get; set; } = new();
    public int Total { get; private set; }
    public int TotalPages { get; private set; } = 1;

    [FromQuery(Name = "page")]
    public int PageNumber { get; set; } = 1;

    [FromQuery]
    public string? Search { get; set; }

    [FromQuery]
    public string? Result { get; set; }

    public async Task OnGetAsync()
    {
        const int pageSize = 40;
        var query = _db.AuditLogs.AsNoTracking().Include(l => l.User).AsQueryable();

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var term = Search.Trim();
            query = query.Where(l => (l.UserName != null && l.UserName.Contains(term)) ||
                                     l.Action.Contains(term) ||
                                     (l.EntityType != null && l.EntityType.Contains(term)));
        }
        if (!string.IsNullOrWhiteSpace(Result))
            query = query.Where(l => l.Result == Result);

        Total = await query.CountAsync();
        var pageCount = Total == 0 ? 1 : (int)Math.Ceiling((double)Total / pageSize);
        PageNumber = Math.Clamp(PageNumber, 1, pageCount);
        TotalPages = pageCount;

        Logs = await query.OrderByDescending(l => l.Timestamp)
            .Skip((PageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync();

        ViewData["TotalPages"] = pageCount;
    }
}