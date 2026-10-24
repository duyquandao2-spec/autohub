using AutoHub.Data;
using AutoHub.Models;
using AutoHub.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoHub.Services.Auth;

public interface IUserService
{
    Task<UserProfileDto?> GetProfileAsync(string userId);
    Task<IPagedResult<UserAdminDto>> GetUsersAsync(int page, int pageSize, string? search, string? role, string? status);
    Task<UserAdminDto?> GetUserAsync(string userId);
    Task<bool> SetDeactivatedAsync(string actorId, string targetUserId, bool deactivated);
    Task<bool> AssignRoleAsync(string actorId, string targetUserId, string role);
    Task<ICollection<string>> GetRolesAsync(string userId);
    Task MarkNotificationReadAsync(string userId, int notificationId);
}

public sealed record UserProfileDto(string Id, string FullName, string Email, string? Phone,
    DateTime CreatedAt, string? PreferredContactMethod, DateOnly? DateOfBirth, bool MarketingConsent,
    IList<string> Roles);

public sealed record UserAdminDto(string Id, string FullName, string Email, string? Phone,
    bool IsDeactivated, bool EmailConfirmed, DateTime CreatedAt, IList<string> Roles);

public interface IPagedResult<T>
{
    IReadOnlyList<T> Items { get; }
    int CurrentPage { get; }
    int PageSize { get; }
    int TotalItems { get; }
    int TotalPages { get; }
}

public sealed class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly AutoHubDbContext _db;

    public UserService(UserManager<ApplicationUser> users, AutoHubDbContext db)
    {
        _users = users;
        _db = db;
    }

    public async Task<UserProfileDto?> GetProfileAsync(string userId)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user is null) return null;

        var roles = await _users.GetRolesAsync(user);
        return new UserProfileDto(user.Id, user.FullName, user.Email ?? "", user.PhoneNumber,
            user.CreatedAt, user.PreferredContactMethod, user.DateOfBirth, user.MarketingConsent, roles);
    }

    public async Task<IPagedResult<UserAdminDto>> GetUsersAsync(int page, int pageSize, string? search, string? role, string? status)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _users.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(u => (u.FullName != null && u.FullName.Contains(term)) ||
                                     (u.Email != null && u.Email.Contains(term)) ||
                                     (u.PhoneNumber != null && u.PhoneNumber.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var deactivated = status.Equals("deactivated", StringComparison.OrdinalIgnoreCase);
            query = query.Where(u => u.IsDeactivated == deactivated);
        }

        var totalItems = await query.CountAsync();
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling((double)totalItems / pageSize);
        page = totalPages == 0 ? 1 : Math.Min(page, totalPages);

        var rows = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = new List<UserAdminDto>();
        foreach (var u in rows)
        {
            var roles = await _users.GetRolesAsync(u);
            if (!string.IsNullOrWhiteSpace(role) &&
                !roles.Contains(role, StringComparer.OrdinalIgnoreCase))
                continue;
            items.Add(new UserAdminDto(u.Id, u.FullName, u.Email ?? "", u.PhoneNumber,
                u.IsDeactivated, u.EmailConfirmed, u.CreatedAt, roles));
        }

        return new PagedResult<UserAdminDto>(items, page, pageSize, totalItems, totalPages);
    }

    public async Task<UserAdminDto?> GetUserAsync(string userId)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user is null) return null;
        var roles = await _users.GetRolesAsync(user);
        return new UserAdminDto(user.Id, user.FullName, user.Email ?? "", user.PhoneNumber,
            user.IsDeactivated, user.EmailConfirmed, user.CreatedAt, roles);
    }

    public async Task<bool> SetDeactivatedAsync(string actorId, string targetUserId, bool deactivated)
    {
        if (actorId == targetUserId) return false;
        var user = await _users.FindByIdAsync(targetUserId);
        if (user is null) return false;

        user.IsDeactivated = deactivated;
        user.UpdatedAt = DateTime.UtcNow;
        var result = await _users.UpdateAsync(user);
        if (result.Succeeded && deactivated)
        {
            await _db.UserNotifications.AddAsync(new UserNotification
            {
                UserId = targetUserId,
                Type = "ACCOUNT",
                Title = "Tài khoản của bạn đã bị vô hiệu hóa",
                Message = "Vui lòng liên hệ bộ phận hỗ trợ nếu bạn cho rằng đây là nhầm lẫn.",
            });
            await _db.SaveChangesAsync();
        }
        return result.Succeeded;
    }

    public async Task<bool> AssignRoleAsync(string actorId, string targetUserId, string role)
    {
        if (role == RoleNames.SuperAdmin || role == RoleNames.Administrator)
            return false;

        var user = await _users.FindByIdAsync(targetUserId);
        if (user is null || actorId == targetUserId) return false;

        var current = (await _users.GetRolesAsync(user)).ToList();
        var result = await _users.RemoveFromRolesAsync(user, current);
        if (!result.Succeeded) return false;

        result = await _users.AddToRoleAsync(user, role);
        return result.Succeeded;
    }

    public async Task<ICollection<string>> GetRolesAsync(string userId)
    {
        var user = await _users.FindByIdAsync(userId);
        return user is null ? Array.Empty<string>() : await _users.GetRolesAsync(user);
    }

    public async Task MarkNotificationReadAsync(string userId, int notificationId)
    {
        var n = await _db.UserNotifications.FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId);
        if (n is not null)
        {
            n.IsRead = true;
            await _db.SaveChangesAsync();
        }
    }
}

public sealed class PagedResult<T> : IPagedResult<T>
{
    public PagedResult(IReadOnlyList<T> items, int page, int pageSize, int totalItems, int totalPages)
    {
        Items = items; CurrentPage = page; PageSize = pageSize; TotalItems = totalItems; TotalPages = totalPages;
    }
    public IReadOnlyList<T> Items { get; }
    public int CurrentPage { get; }
    public int PageSize { get; }
    public int TotalItems { get; }
    public int TotalPages { get; }
}
