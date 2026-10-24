using AutoHub.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoHub.Pages;

public class LogoutModel : PageModel
{
    private readonly IAuthService _auth;

    public LogoutModel(IAuthService auth) => _auth = auth;

    public async Task<IActionResult> OnGetAsync()
    {
        await _auth.SignOutAsync();
        return RedirectToPage("/Index");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _auth.SignOutAsync();
        return RedirectToPage("/Index");
    }
}
