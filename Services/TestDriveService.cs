using AutoHub.Data;
using AutoHub.Models.Entities;
using AutoHub.Services.Auth;
using Microsoft.EntityFrameworkCore;

namespace AutoHub.Services;

public interface ITestDriveService
{
    Task<TestDriveBookingEntity> CreateAsync(TestDriveBookingEntity booking);
    Task<IPagedResult<TestDriveListView>> GetPagedAsync(int page, int pageSize, string? status, string? search, int? showroomId);
    Task<TestDriveBookingEntity?> GetAsync(int id);
    Task<List<TestDriveBookingEntity>> GetMineAsync(string userId);
    Task<UpdateResult> UpdateAsync(int id, string status, string? assignedStaffId, int? showroomId);
}

public sealed record UpdateResult(bool Success, string? Error = null);

public sealed record TestDriveListView(int BookingId, string FullName, string Phone, string? Email,
    int? VariantId, string? VehicleName, DateOnly PreferredDate, string TimeSlot, string Status,
    string? ShowroomName, string? AssignedStaffName, int? ShowroomId, string? AssignedStaffId, DateTime CreatedAt);

public sealed class TestDriveService : ITestDriveService
{
    private readonly AutoHubDbContext _db;
    private readonly IAuditService _audit;

    public TestDriveService(AutoHubDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<TestDriveBookingEntity> CreateAsync(TestDriveBookingEntity booking)
    {
        booking.CreatedAt = DateTime.UtcNow;
        booking.UpdatedAt = booking.CreatedAt;
        if (string.IsNullOrWhiteSpace(booking.Status)) booking.Status = "PENDING";
        _db.TestDriveBookings.Add(booking);
        await _db.SaveChangesAsync();
        return booking;
    }

    public async Task<IPagedResult<TestDriveListView>> GetPagedAsync(int page, int pageSize, string? status, string? search, int? showroomId)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.TestDriveBookings.AsNoTracking()
            .Include(b => b.Variant)
            .Include(b => b.Showroom)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(b => b.Status == status);
        if (showroomId.HasValue) query = query.Where(b => b.ShowroomId == showroomId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(b => b.FullName.Contains(term) || b.Phone.Contains(term) ||
                                     (b.Email != null && b.Email.Contains(term)));
        }

        var totalItems = await query.CountAsync();
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling((double)totalItems / pageSize);
        page = totalPages == 0 ? 1 : Math.Min(page, totalPages);

        var rows = await query.OrderByDescending(b => b.PreferredDate)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var staffIds = rows.Where(b => b.AssignedStaffId != null).Select(b => b.AssignedStaffId!).Distinct().ToList();
        var staffNames = await _db.Users.AsNoTracking()
            .Where(u => staffIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName);

        var items = rows.Select(b => new TestDriveListView(b.BookingId, b.FullName, b.Phone, b.Email,
            b.VariantId, b.Variant != null ? b.Variant.VariantName : null, b.PreferredDate, b.TimeSlot,
            b.Status, b.Showroom?.Name,
            b.AssignedStaffId != null && staffNames.TryGetValue(b.AssignedStaffId, out var n) ? n : null,
            b.ShowroomId, b.AssignedStaffId, b.CreatedAt)).ToList();

        return new PagedResult<TestDriveListView>(items, page, pageSize, totalItems, totalPages);
    }

    public async Task<TestDriveBookingEntity?> GetAsync(int id)
        => await _db.TestDriveBookings
            .Include(b => b.Variant)
            .Include(b => b.Showroom)
            .FirstOrDefaultAsync(b => b.BookingId == id);

    public async Task<List<TestDriveBookingEntity>> GetMineAsync(string userId)
        => await _db.TestDriveBookings.AsNoTracking()
            .Include(b => b.Variant)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

    public async Task<UpdateResult> UpdateAsync(int id, string status, string? assignedStaffId, int? showroomId)
    {
        var b = await _db.TestDriveBookings.FindAsync(id);
        if (b is null) return new UpdateResult(false, "Lịch lái thử không tồn tại.");

        var moveError = StatusWorkflow.CheckMove(b.Status, status, StatusWorkflow.TestDrive);
        if (moveError is not null) return new UpdateResult(false, moveError);

        var previous = b.Status;
        b.Status = status;
        if (assignedStaffId is not null)
            b.AssignedStaffId = string.IsNullOrWhiteSpace(assignedStaffId) ? null : assignedStaffId;
        if (showroomId.HasValue) b.ShowroomId = showroomId;
        b.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _audit.RecordAsync("TestDriveStatusUpdate", "TestDriveBooking", b.BookingId.ToString(),
            "SUCCESS", $"{previous} → {status} | staff={b.AssignedStaffId ?? "-"} | showroom={b.ShowroomId?.ToString() ?? "-"}");
        return new UpdateResult(true);
    }
}
