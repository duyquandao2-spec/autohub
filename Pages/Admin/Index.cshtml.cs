using System.Linq;
using AutoHub.Models;
using AutoHub.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoHub.Pages.Admin;
[Authorize(Policy = Policies.AdminArea)]
public class IndexModel : PageModel
{
    private readonly IAdminDashboardService _dashboard;

    public IndexModel(IAdminDashboardService dashboard) => _dashboard = dashboard;

    public DashboardSummary Summary { get; set; } = null!;
    public int ConsultationMax { get; private set; }
    public int TestDriveMax { get; private set; }
    public int UsersMax { get; private set; }

    public async Task OnGetAsync()
    {
        Summary = await _dashboard.GetSummaryAsync();
        ConsultationMax = Max(Summary.ConsultationsByDay);
        TestDriveMax = Max(Summary.TestDrivesByDay);
        UsersMax = Max(Summary.NewUsersByDay);
    }

    private static int Max(IReadOnlyList<SeriesPoint> points) => points.Count == 0 ? 1 : System.Math.Max(1, points.Select(p => p.Value).Max());
}
