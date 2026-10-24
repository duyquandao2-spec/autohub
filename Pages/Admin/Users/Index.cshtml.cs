using AutoHub.Models;
using AutoHub.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoHub.Pages.Admin.Users;

[Authorize(Policy = Policies.UserManagement)]
public class IndexModel : PageModel
{
    private readonly IUserService _users;
    private readonly ICurrentUser _current;

    public IndexModel(IUserService users, ICurrentUser current)
    {
        _users = users;
        _current = current;
    }

    public IPagedResult<UserAdminDto> Paged { get; set; } = null!;

    [FromQuery(Name = "page")]
    public int PageNumber { get; set; } = 1;

    [FromQuery]
    public string? Search { get; set; }

    [FromQuery]
    public string? Role { get; set; }

    [FromQuery]
    public string? Status { get; set; }

    public string CurrentUserId => _current.UserId ?? string.Empty;

    public static readonly string[] AssignableRoles = { RoleNames.Customer, RoleNames.Sales, RoleNames.ContentEditor, RoleNames.Manager };

    public async Task OnGetAsync()
    {
        Paged = await _users.GetUsersAsync(PageNumber, 25, Search, Role, Status);
    }

    public async Task<IActionResult> OnPostRoleAsync(string id, string role)
    {
        if (!AssignableRoles.Contains(role)) return RedirectToPage();
        await _users.AssignRoleAsync(_current.UserId!, id, role);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeactivateAsync(string id, bool deactivate)
    {
        await _users.SetDeactivatedAsync(_current.UserId!, id, deactivate);
        return RedirectToPage();
    }
}
