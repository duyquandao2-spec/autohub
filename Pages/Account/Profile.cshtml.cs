using AutoHub.Data;
using AutoHub.Models.Entities;
using AutoHub.Services;
using AutoHub.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AutoHub.Pages.Account;

[Authorize]
public class ProfileModel : PageModel
{
    private readonly ICurrentUser _current;
    private readonly IAuthService _auth;
    private readonly IUserService _users;
    private readonly AutoHubDbContext _db;

    public ProfileModel(ICurrentUser current, IAuthService auth, IUserService users, AutoHubDbContext db)
    {
        _current = current;
        _auth = auth;
        _users = users;
        _db = db;
    }

    [BindProperty]
    public string FullName { get; set; } = string.Empty;

    [BindProperty]
    public string? Phone { get; set; }

    [BindProperty]
    public string? PreferredContactMethod { get; set; }

    public UserProfileDto? Profile { get; set; }
    public int FavoriteCount { get; set; }
    public DateTime MemberSince => Profile?.CreatedAt ?? DateTime.UtcNow;

    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public bool StatusIsError { get; set; }

    public async Task OnGetAsync()
    {
        Profile = await _users.GetProfileAsync(_current.UserId!);
        FullName = Profile?.FullName ?? string.Empty;
        Phone = Profile?.Phone;
        PreferredContactMethod = Profile?.PreferredContactMethod;
        FavoriteCount = await _db.Favorites.CountAsync(f => f.UserId == _current.UserId);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Profile = await _users.GetProfileAsync(_current.UserId!);
            return Page();
        }

        var result = await _auth.EditProfileAsync(_current.UserId!, new EditProfileRequest
        {
            FullName = FullName,
            Phone = Phone ?? string.Empty,
            PreferredContactMethod = string.IsNullOrWhiteSpace(PreferredContactMethod) ? null : PreferredContactMethod,
        });

        if (!result.Succeeded)
        {
            StatusMessage = result.Error;
            StatusIsError = true;
            Profile = await _users.GetProfileAsync(_current.UserId!);
            return Page();
        }

        StatusMessage = "Hồ sơ đã được cập nhật.";
        return RedirectToPage();
    }
}
