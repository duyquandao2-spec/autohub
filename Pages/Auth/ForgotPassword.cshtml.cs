using AutoHub.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoHub.Pages.Auth;

public class ForgotPasswordModel : PageModel
{
    private readonly IAuthService _auth;

    public ForgotPasswordModel(IAuthService auth) => _auth = auth;

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [TempData]
    public bool Sent { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        await _auth.ForgotPasswordAsync(Email);
        Sent = true;
        return RedirectToPage();
    }
}
