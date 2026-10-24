using AutoHub.Data;
using AutoHub.Models.Entities;
using AutoHub.Services.Auth;
using Microsoft.EntityFrameworkCore;

namespace AutoHub.Services;

public interface IConsultationService
{
    Task<ConsultationRequestEntity> CreateAsync(ConsultationRequestEntity request);
    Task<IPagedResult<ConsultationListView>> GetPagedAsync(int page, int pageSize, string? status, string? search, int? vehicleId, string? assignedStaffId);
    Task<ConsultationRequestEntity?> GetAsync(int id);
    Task<List<ConsultationRequestEntity>> GetMineAsync(string userId);
    Task<UpdateResult> UpdateAsync(int id, string status, string? assignedStaffId);
    Task<bool> AddStaffNoteAsync(int id, string authorId, string authorName, string note);
}

public sealed record ConsultationListView(int Id, string FullName, string Phone, string? Email,
    DateOnly? PreferredDate, string? PreferredTime, string? ConsultationMethod, string? Budget,
    string Status, string? AssignedStaffName, DateTime CreatedAt, int? VehicleId, string? VehicleName);

public sealed class ConsultationService : IConsultationService
{
    private readonly AutoHubDbContext _db;
    private readonly IAuditService _audit;

    public ConsultationService(AutoHubDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<ConsultationRequestEntity> CreateAsync(ConsultationRequestEntity request)
    {
        request.CreatedAt = DateTime.UtcNow;
        request.UpdatedAt = request.CreatedAt;
        if (string.IsNullOrWhiteSpace(request.Status)) request.Status = "NEW";
        _db.ConsultationRequests.Add(request);
        await _db.SaveChangesAsync();
        return request;
    }

    public async Task<IPagedResult<ConsultationListView>> GetPagedAsync(int page, int pageSize, string? status, string? search, int? vehicleId, string? assignedStaffId)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.ConsultationRequests.AsNoTracking()
            .Include(c => c.Vehicle)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(c => c.Status == status);
        if (vehicleId.HasValue) query = query.Where(c => c.VehicleId == vehicleId);
        if (!string.IsNullOrWhiteSpace(assignedStaffId)) query = query.Where(c => c.AssignedStaffId == assignedStaffId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c => c.FullName.Contains(term) || c.Phone.Contains(term) ||
                                     (c.Email != null && c.Email.Contains(term)));
        }

        var totalItems = await query.CountAsync();
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling((double)totalItems / pageSize);
        page = totalPages == 0 ? 1 : Math.Min(page, totalPages);

        var rows = await query.OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var staffIds = rows.Where(c => c.AssignedStaffId != null).Select(c => c.AssignedStaffId!).Distinct().ToList();
        var staffNames = await _db.Users.AsNoTracking()
            .Where(u => staffIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName);

        var items = rows.Select(c => new ConsultationListView(c.Id, c.FullName, c.Phone, c.Email,
            c.PreferredDate, c.PreferredTime, c.ConsultationMethod, c.Budget, c.Status,
            c.AssignedStaffId != null && staffNames.TryGetValue(c.AssignedStaffId, out var n) ? n : null,
            c.CreatedAt, c.VehicleId, c.Vehicle != null ? c.Vehicle.VariantName : null)).ToList();

        return new PagedResult<ConsultationListView>(items, page, pageSize, totalItems, totalPages);
    }

    public async Task<ConsultationRequestEntity?> GetAsync(int id)
        => await _db.ConsultationRequests
            .Include(c => c.Vehicle)
            .Include(c => c.Notes)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<List<ConsultationRequestEntity>> GetMineAsync(string userId)
        => await _db.ConsultationRequests.AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

    public async Task<UpdateResult> UpdateAsync(int id, string status, string? assignedStaffId)
    {
        var c = await _db.ConsultationRequests.FindAsync(id);
        if (c is null) return new UpdateResult(false, "Yêu cầu tư vấn không tồn tại.");

        var moveError = StatusWorkflow.CheckMove(c.Status, status, StatusWorkflow.Consultation);
        if (moveError is not null) return new UpdateResult(false, moveError);

        var previous = c.Status;
        c.Status = status;
        c.AssignedStaffId = string.IsNullOrWhiteSpace(assignedStaffId) ? null : assignedStaffId;
        c.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _audit.RecordAsync("ConsultationStatusUpdate", "ConsultationRequest", c.Id.ToString(),
            "SUCCESS", $"{previous} → {status} | staff={c.AssignedStaffId ?? "-"}");
        return new UpdateResult(true);
    }

    public async Task<bool> AddStaffNoteAsync(int id, string authorId, string authorName, string note)
    {
        if (string.IsNullOrWhiteSpace(note)) return false;
        if (!await _db.ConsultationRequests.AnyAsync(c => c.Id == id)) return false;

        _db.StaffNotes.Add(new StaffNoteEntity
        {
            ConsultationId = id,
            AuthorId = authorId,
            AuthorName = authorName,
            Note = note.Trim(),
        });
        await _db.SaveChangesAsync();
        return true;
    }
}
