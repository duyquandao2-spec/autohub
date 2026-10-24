using AutoHub.Models;
using AutoHub.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoHub.Pages.Admin.Users;

[Authorize(Policy = Policies.UserManagement)]
public class DetailModel : PageModel
{
    private readonly IUserService _users;
    private readonly ICurrentUser _current;

    public DetailModel(IUserService users, ICurrentUser current)
    {
        _users = users;
        _current = current;
    }

    public new UserAdminDto? User { get; set; }
    public UserProfileDto? Profile { get; set; }
    public string CurrentUserId => _current.UserId ?? string.Empty;

    [BindProperty]
    public string Id { get; set; } = string.Empty;

    [BindProperty]
    public string Role { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(string id)
    {
        User = await _users.GetUserAsync(id);
        if (User is null) return NotFound();
        Profile = await _users.GetProfileAsync(id);
        return Page();
    }

    public async Task<IActionResult> OnPostRoleAsync()
    {
        if (Id == CurrentUserId) return RedirectToPage();
        await _users.AssignRoleAsync(CurrentUserId, Id, Role);
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostDeactivateAsync(bool deactivate)
    {
        if (Id == CurrentUserId) return RedirectToPage();
        await _users.SetDeactivatedAsync(CurrentUserId, Id, deactivate);
        return RedirectToPage(new { id = Id });
    }
}
