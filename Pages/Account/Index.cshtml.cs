using AutoHub.Data;
using AutoHub.Models.Entities;
using AutoHub.Services;
using AutoHub.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AutoHub.Pages.Account;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ICurrentUser _current;
    private readonly IUserService _users;
    private readonly IFavoriteService _favorites;
    private readonly IConsultationService _consultations;
    private readonly ITestDriveService _testDrives;
    private readonly AutoHubDbContext _db;

    public IndexModel(ICurrentUser current, IUserService users, IFavoriteService favorites,
        IConsultationService consultations, ITestDriveService testDrives, AutoHubDbContext db)
    {
        _current = current;
        _users = users;
        _favorites = favorites;
        _consultations = consultations;
        _testDrives = testDrives;
        _db = db;
    }

    public UserProfileDto? Profile { get; set; }
    public int FavoriteCount { get; set; }
    public int ConsultationCount { get; set; }
    public int TestDriveCount { get; set; }
    public int UnreadNotifications { get; set; }
    public IReadOnlyList<FavoriteEntity> Favorites { get; set; } = Array.Empty<FavoriteEntity>();
    public IReadOnlyList<ConsultationRequestEntity> RecentConsultations { get; set; } = Array.Empty<ConsultationRequestEntity>();
    public IReadOnlyList<TestDriveBookingEntity> RecentTestDrives { get; set; } = Array.Empty<TestDriveBookingEntity>();

    public async Task OnGetAsync()
    {
        var uid = _current.UserId!;
        Profile = await _users.GetProfileAsync(uid);

        var favIds = await _favorites.GetFavoriteIdsAsync(uid);
        FavoriteCount = favIds.Count;
        Favorites = await _db.Favorites.AsNoTracking()
            .Include(f => f.Vehicle).ThenInclude(v => v.Model)
            .Where(f => f.UserId == uid)
            .OrderByDescending(f => f.CreatedAt)
            .Take(6)
            .ToListAsync();

        var consults = await _consultations.GetMineAsync(uid);
        ConsultationCount = consults.Count;
        RecentConsultations = consults.Take(5).ToList();

        var drives = await _testDrives.GetMineAsync(uid);
        TestDriveCount = drives.Count;
        RecentTestDrives = drives.Take(5).ToList();

        UnreadNotifications = await _db.UserNotifications.CountAsync(n => n.UserId == uid && !n.IsRead);
    }
}
