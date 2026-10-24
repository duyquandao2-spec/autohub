using AutoHub.Data;
using AutoHub.Models;
using AutoHub.Models.Entities;
using AutoHub.Models.Identity;
using AutoHub.Services;
using AutoHub.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AutoHub.Pages.Admin.TestDrives;

[Authorize(Policy = Policies.AppointmentManagement)]
public class DetailModel : PageModel
{
    private readonly ITestDriveService _testDrives;
    private readonly ISettingsService _settings;
    private readonly ICurrentUser _current;
    private readonly UserManager<ApplicationUser> _users;
    private readonly AutoHubDbContext _db;

    public DetailModel(ITestDriveService testDrives, ISettingsService settings, ICurrentUser current,
        UserManager<ApplicationUser> users, AutoHubDbContext db)
    {
        _testDrives = testDrives;
        _settings = settings;
        _current = current;
        _users = users;
        _db = db;
    }

    public TestDriveBookingEntity? Booking { get; set; }
    public List<ApplicationUser> Staff { get; set; } = new();
    public IReadOnlyList<ShowroomEntity> Showrooms { get; set; } = Array.Empty<ShowroomEntity>();
    public IReadOnlyList<string> StatusOptions { get; set; } = Array.Empty<string>();
    public bool IsTerminal { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Booking = await _testDrives.GetAsync(id);
        if (Booking is null) return NotFound();
        StatusOptions = BuildStatusOptions(Booking.Status);
        IsTerminal = StatusWorkflow.IsTerminal(Booking.Status, StatusWorkflow.TestDrive);
        await LoadOptionsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateAsync(int id, string status, string? assignedStaffId, int? showroomId)
    {
        var result = await _testDrives.UpdateAsync(id, status, assignedStaffId, showroomId);
        if (!result.Success)
        {
            TempData["Err"] = result.Error;
            return RedirectToPage(new { id });
        }
        TempData["Ok"] = "Đã cập nhật lịch lái thử.";
        return RedirectToPage(new { id });
    }

    private static IReadOnlyList<string> BuildStatusOptions(string current)
    {
        var set = new List<string> { current };
        set.AddRange(StatusWorkflow.Next(current, StatusWorkflow.TestDrive));
        return [.. set.Distinct()];
    }

    private async Task LoadOptionsAsync()
    {
        Showrooms = await _settings.GetShowroomsAsync();
        var set = new HashSet<string>();
        foreach (var role in RoleNames.AppointmentManagers)
            foreach (var u in await _users.GetUsersInRoleAsync(role))
                set.Add(u.Id);
        Staff = await _db.Users.AsNoTracking().Where(u => set.Contains(u.Id)).OrderBy(u => u.FullName).ToListAsync();
    }
}
