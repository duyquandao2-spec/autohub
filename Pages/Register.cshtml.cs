using System.ComponentModel.DataAnnotations;
using AutoHub.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoHub.Pages;

public class RegisterModel : PageModel
{
    private readonly IAuthService _auth;

    public RegisterModel(IAuthService auth) => _auth = auth;

    [BindProperty]
    public string UserName { get; set; } = string.Empty;

    [BindProperty]
    public string FullName { get; set; } = string.Empty;

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Phone { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public string ConfirmPassword { get; set; } = string.Empty;

    [BindProperty]
    public string? PreferredContactMethod { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public bool StatusIsError { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Password != ConfirmPassword)
        {
            StatusMessage = "Mật khẩu xác nhận không khớp.";
            StatusIsError = true;
            return Page();
        }

        var result = await _auth.RegisterAsync(new RegisterRequest
        {
            UserName = UserName,
            FullName = FullName,
            Email = Email,
            Phone = Phone,
            Password = Password,
            PreferredContactMethod = string.IsNullOrWhiteSpace(PreferredContactMethod) ? null : PreferredContactMethod,
        }, emailConfirmed: true);

        if (!result.Succeeded)
        {
            StatusMessage = result.Error;
            StatusIsError = true;
            return Page();
        }

        await _auth.LoginAsync(UserName, Password, rememberMe: true);
        return RedirectToPage("/Account/Index");
    }
}
