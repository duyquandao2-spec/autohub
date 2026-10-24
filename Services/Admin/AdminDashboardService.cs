using AutoHub.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoHub.Services.Admin;

public interface IAdminDashboardService
{
    Task<DashboardSummary> GetSummaryAsync();
}

public sealed record DashboardSummary(
    int TotalVehicles,
    int ActiveVehicles,
    int TotalUsers,
    int NewUsers7d,
    int NewConsultations,
    int PendingConsultations,
    int UpcomingTestDrives,
    int PublishedNews,
    int ContactInquiries,
    IReadOnlyList<SeriesPoint> ConsultationsByDay,
    IReadOnlyList<SeriesPoint> TestDrivesByDay,
    IReadOnlyList<SeriesPoint> NewUsersByDay,
    IReadOnlyList<ConsultationsRecent> RecentConsultations,
    IReadOnlyList<UserRecent> RecentUsers);

public sealed record SeriesPoint(string Label, int Value);
public sealed record ConsultationsRecent(int Id, string FullName, string Status, DateTime CreatedAt);
public sealed record UserRecent(string Id, string FullName, string Email, DateTime CreatedAt);

public sealed class AdminDashboardService : IAdminDashboardService
{
    private readonly AutoHubDbContext _db;

    public AdminDashboardService(AutoHubDbContext db) => _db = db;

    public async Task<DashboardSummary> GetSummaryAsync()
    {
        var now = DateTime.UtcNow;
        var sevenDaysAgo = now.AddDays(-7);

        var recentConsultations = await _db.ConsultationRequests.AsNoTracking()
            .OrderByDescending(c => c.CreatedAt)
            .Take(8)
            .Select(c => new ConsultationsRecent(c.Id, c.FullName, c.Status, c.CreatedAt))
            .ToListAsync();

        var recentUsers = await _db.Users.AsNoTracking()
            .OrderByDescending(u => u.CreatedAt)
            .Take(8)
            .Select(u => new UserRecent(u.Id, u.FullName, u.Email ?? "", u.CreatedAt))
            .ToListAsync();

        var consultationByDay = await _db.ConsultationRequests.AsNoTracking()
            .Where(c => c.CreatedAt >= sevenDaysAgo)
            .GroupBy(c => c.CreatedAt.Date)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync();

        var testDriveByDay = await _db.TestDriveBookings.AsNoTracking()
            .Where(t => t.CreatedAt >= sevenDaysAgo)
            .GroupBy(t => t.CreatedAt.Date)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync();

        var newUsersByDay = await _db.Users.AsNoTracking()
            .Where(u => u.CreatedAt >= sevenDaysAgo)
            .GroupBy(u => u.CreatedAt.Date)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync();

        return new DashboardSummary(
            TotalVehicles: await _db.VehicleVariants.CountAsync(),
            ActiveVehicles: await _db.VehicleVariants.CountAsync(v => v.Status == "ACTIVE" && v.IsAvailable),
            TotalUsers: await _db.Users.CountAsync(),
            NewUsers7d: await _db.Users.CountAsync(u => u.CreatedAt >= sevenDaysAgo),
            NewConsultations: await _db.ConsultationRequests.CountAsync(c => c.Status == "NEW"),
            PendingConsultations: await _db.ConsultationRequests.CountAsync(c => c.Status == "NEW" || c.Status == "CONTACTED"),
            UpcomingTestDrives: await _db.TestDriveBookings.CountAsync(t => t.Status == "PENDING" || t.Status == "CONFIRMED"),
            PublishedNews: await _db.NewsArticles.CountAsync(a => a.Status == "PUBLISHED" && !a.IsDeleted),
            ContactInquiries: await _db.ContactMessages.CountAsync(),
            ConsultationsByDay: Series(sevenDaysAgo, now, consultationByDay),
            TestDrivesByDay: Series(sevenDaysAgo, now, testDriveByDay),
            NewUsersByDay: Series(sevenDaysAgo, now, newUsersByDay),
            RecentConsultations: recentConsultations,
            RecentUsers: recentUsers
        );
    }

    private static IReadOnlyList<SeriesPoint> Series<T>(DateTime from, DateTime to, List<T> grouped) where T : class
    {
        var lookup = new Dictionary<DateTime, int>();
        foreach (var g in grouped)
        {
            var type = typeof(T);
            var key = (DateTime)type.GetProperty("Key")!.GetValue(g)!;
            var count = (int)type.GetProperty("Count")!.GetValue(g)!;
            lookup[key.Date] = count;
        }
        var points = new List<SeriesPoint>();
        for (var d = from.Date; d <= to.Date; d = d.AddDays(1))
        {
            points.Add(new SeriesPoint(d.ToString("MM-dd"), lookup.GetValueOrDefault(d, 0)));
        }
        return points;
    }
}
