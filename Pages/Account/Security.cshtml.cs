using AutoHub.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoHub.Pages.Account;

[Authorize]
public class SecurityModel : PageModel
{
    private readonly ICurrentUser _current;
    private readonly IAuthService _auth;

    public SecurityModel(ICurrentUser current, IAuthService auth)
    {
        _current = current;
        _auth = auth;
    }

    [BindProperty]
    public string CurrentPassword { get; set; } = string.Empty;

    [BindProperty]
    public string NewPassword { get; set; } = string.Empty;

    [BindProperty]
    public string ConfirmPassword { get; set; } = string.Empty;

    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public bool StatusIsError { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (NewPassword != ConfirmPassword)
        {
            StatusMessage = "Mật khẩu xác nhận không khớp.";
            StatusIsError = true;
            return Page();
        }

        var result = await _auth.ChangePasswordAsync(_current.UserId!, CurrentPassword, NewPassword);
        if (!result.Succeeded)
        {
            StatusMessage = result.Error;
            StatusIsError = true;
            return Page();
        }

        StatusMessage = "Mật khẩu đã được thay đổi.";
        return RedirectToPage();
    }
}
