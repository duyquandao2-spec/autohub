using AutoHub.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoHub.Pages.Auth;

public class ResetPasswordModel : PageModel
{
    private readonly IAuthService _auth;

    public ResetPasswordModel(IAuthService auth) => _auth = auth;

    [FromQuery]
    public string? Email { get; set; }

    [FromQuery]
    public string? Token { get; set; }

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
        if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Token))
        {
            StatusMessage = "Liên kết đặt lại mật khẩu không hợp lệ.";
            StatusIsError = true;
            return Page();
        }

        if (NewPassword != ConfirmPassword)
        {
            StatusMessage = "Mật khẩu xác nhận không khớp.";
            StatusIsError = true;
            return Page();
        }

        var result = await _auth.ResetPasswordAsync(Email, Token, NewPassword);
        if (!result.Succeeded)
        {
            StatusMessage = result.Error;
            StatusIsError = true;
            return Page();
        }

        StatusMessage = "Mật khẩu đã được đặt lại. Hãy đăng nhập bằng mật khẩu mới.";
        return RedirectToPage("/Login");
    }
}
