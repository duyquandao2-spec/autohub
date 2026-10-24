using AutoHub.Models;
using AutoHub.Models.Entities;
using AutoHub.Services;
using AutoHub.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoHub.Pages.Admin.TestDrives;

[Authorize(Policy = Policies.AppointmentManagement)]
public class IndexModel : PageModel
{
    private readonly ITestDriveService _testDrives;
    private readonly ISettingsService _settings;

    public IndexModel(ITestDriveService testDrives, ISettingsService settings)
    {
        _testDrives = testDrives;
        _settings = settings;
    }

    public IPagedResult<TestDriveListView> Paged { get; set; } = null!;
    public IReadOnlyList<ShowroomEntity> Showrooms { get; set; } = Array.Empty<ShowroomEntity>();

    [FromQuery(Name = "page")]
    public int PageNumber { get; set; } = 1;

    [FromQuery]
    public string? Status { get; set; }

    [FromQuery]
    public int? Showroom { get; set; }

    [FromQuery]
    public string? Search { get; set; }

    public async Task OnGetAsync()
    {
        Showrooms = await _settings.GetShowroomsAsync();
        Paged = await _testDrives.GetPagedAsync(PageNumber, 15, Status, Search, Showroom);
    }
}
