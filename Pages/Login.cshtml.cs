using System.ComponentModel.DataAnnotations;
using AutoHub.Services.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoHub.Pages;

public class LoginModel : PageModel
{
    private readonly IAuthService _auth;

    public LoginModel(IAuthService auth) => _auth = auth;

    [BindProperty]
    public string UserName { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public bool RememberMe { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public bool StatusIsError { get; set; }

    public string? ReturnUrl { get; set; }

    public async Task<IActionResult> OnGetAsync(string? returnUrl)
    {
        ReturnUrl = returnUrl;
        if (User.Identity?.IsAuthenticated == true) return RedirectToPage("/Account/Index");
        await HttpContext.SignOutAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl)
    {
        ReturnUrl = returnUrl;
        if (!ModelState.IsValid)
        {
            StatusMessage = "Vui lòng nhập tên đăng nhập và mật khẩu.";
            StatusIsError = true;
            return Page();
        }

        var result = await _auth.LoginAsync(UserName, Password, RememberMe);
        if (!result.Succeeded)
        {
            StatusMessage = result.Error;
            StatusIsError = true;
            return Page();
        }

        var safe = returnUrl;
        if (!string.IsNullOrWhiteSpace(safe) && Url.IsLocalUrl(safe) && safe != "/")
            return LocalRedirect(safe);

        return RedirectToPage("/Account/Index");
    }
}