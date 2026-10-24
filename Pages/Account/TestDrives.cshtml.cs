using AutoHub.Models.Entities;
using AutoHub.Services;
using AutoHub.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoHub.Pages.Account;

[Authorize]
public class TestDrivesModel : PageModel
{
    private readonly ICurrentUser _current;
    private readonly ITestDriveService _testDrives;

    public TestDrivesModel(ICurrentUser current, ITestDriveService testDrives)
    {
        _current = current;
        _testDrives = testDrives;
    }

    public List<TestDriveBookingEntity> TestDrives { get; set; } = new();

    public async Task OnGetAsync()
    {
        TestDrives = await _testDrives.GetMineAsync(_current.UserId!);
    }
}
