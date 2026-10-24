using AutoHub.Data;
using AutoHub.Models.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace AutoHub.Services.Auth;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest req, bool emailConfirmed);
    Task<AuthResult> LoginAsync(string userInput, string password, bool rememberMe);
    Task SignOutAsync();
    Task<AuthResult> ForgotPasswordAsync(string email);
    Task<AuthResult> ResetPasswordAsync(string email, string token, string newPassword);
    Task<AuthResult> ChangePasswordAsync(string userId, string current, string newPassword);
    Task<AuthResult> EditProfileAsync(string userId, EditProfileRequest req);
}

public sealed record AuthResult(bool Succeeded, string? Error = null);

public sealed class RegisterRequest
{
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public string? PreferredContactMethod { get; set; }
    public bool MarketingConsent { get; set; }
}

public sealed class EditProfileRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? PreferredContactMethod { get; set; }
}

public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly AutoHubDbContext _db;

    public AuthService(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn, AutoHubDbContext db)
    {
        _users = users;
        _signIn = signIn;
        _db = db;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest req, bool emailConfirmed)
    {
        if (string.IsNullOrWhiteSpace(req.FullName) || string.IsNullOrWhiteSpace(req.UserName) ||
            string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            return new AuthResult(false, "Vui lòng điền đầy đủ thông tin.");

        var email = req.Email.Trim();
        var userName = req.UserName.Trim();
        if (await _users.FindByEmailAsync(email) is not null)
            return new AuthResult(false, "Email đã được sử dụng.");

        if (await _users.FindByNameAsync(userName) is not null)
            return new AuthResult(false, "Tên đăng nhập đã được sử dụng.");

        if (string.IsNullOrWhiteSpace(req.Phone))
            return new AuthResult(false, "Vui lòng nhập số điện thoại.");

        var user = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            FullName = req.FullName.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(req.Phone) ? null : req.Phone.Trim(),
            DateOfBirth = req.DateOfBirth,
            PreferredContactMethod = req.PreferredContactMethod,
            MarketingConsent = req.MarketingConsent,
            EmailConfirmed = emailConfirmed,
        };

        var result = await _users.CreateAsync(user, req.Password);
        if (!result.Succeeded)
            return new AuthResult(false, string.Join(" ", result.Errors.Select(e => e.Description)));

        await _users.AddToRoleAsync(user, Models.RoleNames.Customer);
        return new AuthResult(true);
    }

    public async Task<AuthResult> LoginAsync(string userInput, string password, bool rememberMe)
    {
        if (string.IsNullOrWhiteSpace(userInput) || string.IsNullOrWhiteSpace(password))
            return new AuthResult(false, "Thông tin đăng nhập không chính xác.");

        var user = await _users.FindByNameAsync(userInput.Trim());
        if (user is null)
            return new AuthResult(false, "Thông tin đăng nhập không chính xác.");

        if (user.IsDeactivated)
            return new AuthResult(false, "Tài khoản đã bị vô hiệu hóa.");

        var result = await _signIn.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            user.UpdatedAt = DateTime.UtcNow;
            await _users.UpdateAsync(user);
            return new AuthResult(true);
        }

        if (result.IsLockedOut)
            return new AuthResult(false, "Tài khoản bị khóa tạm thời do đăng nhập sai nhiều lần.");

        return new AuthResult(false, "Thông tin đăng nhập không chính xác.");
    }

    public Task SignOutAsync() => _signIn.SignOutAsync();

    public async Task<AuthResult> ForgotPasswordAsync(string email)
    {
        // Always return success to avoid account enumeration; only act if user exists.
        if (string.IsNullOrWhiteSpace(email)) return new AuthResult(true);

        var user = await _users.FindByEmailAsync(email.Trim());
        if (user is null || user.IsDeactivated) return new AuthResult(true);

        var token = await _users.GeneratePasswordResetTokenAsync(user);
        // In development we log the token to console for manual reset.
        Console.WriteLine($"[Auth] Password reset token for {user.Email}: {token}");
        return new AuthResult(true);
    }

    public async Task<AuthResult> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _users.FindByEmailAsync(email.Trim());
        if (user is null)
            return new AuthResult(false, "Không thể đặt lại mật khẩu.");

        var result = await _users.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
            return new AuthResult(false, string.Join(" ", result.Errors.Select(e => e.Description)));

        // Invalidate existing sessions by re-signing out.
        await _signIn.SignOutAsync();
        return new AuthResult(true);
    }

    public async Task<AuthResult> ChangePasswordAsync(string userId, string current, string newPassword)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user is null) return new AuthResult(false, "Tài khoản không tồn tại.");

        var result = await _users.ChangePasswordAsync(user, current, newPassword);
        if (!result.Succeeded)
            return new AuthResult(false, string.Join(" ", result.Errors.Select(e => e.Description)));

        await _signIn.RefreshSignInAsync(user);
        return new AuthResult(true);
    }

    public async Task<AuthResult> EditProfileAsync(string userId, EditProfileRequest req)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user is null) return new AuthResult(false, "Tài khoản không tồn tại.");

        if (!string.IsNullOrWhiteSpace(req.FullName)) user.FullName = req.FullName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(req.Phone) ? null : req.Phone.Trim();
        user.PreferredContactMethod = req.PreferredContactMethod;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _users.UpdateAsync(user);
        if (!result.Succeeded)
            return new AuthResult(false, string.Join(" ", result.Errors.Select(e => e.Description)));

        return new AuthResult(true);
    }
}
