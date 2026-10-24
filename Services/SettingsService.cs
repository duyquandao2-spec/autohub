using AutoHub.Data;
using AutoHub.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoHub.Services;

public interface ISettingsService
{
    Task<string?> GetAsync(string key);
    Task<Dictionary<string, string>> GetAllAsync(string? group = null);
    Task<bool> SetAsync(string key, string? value, string? group = null);
    Task<IReadOnlyList<ShowroomEntity>> GetShowroomsAsync(bool activeOnly = false);
    Task<bool> SaveShowroomAsync(ShowroomEntity showroom);
}

public sealed class SettingsService : ISettingsService
{
    private readonly AutoHubDbContext _db;

    public SettingsService(AutoHubDbContext db) => _db = db;

    public async Task<string?> GetAsync(string key)
        => (await _db.SiteSettings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == key))?.Value;

    public async Task<Dictionary<string, string>> GetAllAsync(string? group = null)
    {
        var query = _db.SiteSettings.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(group)) query = query.Where(s => s.Group == group);
        return await query.ToDictionaryAsync(s => s.Key, s => s.Value ?? string.Empty);
    }

    public async Task<bool> SetAsync(string key, string? value, string? group = null)
    {
        var setting = await _db.SiteSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting is null)
        {
            _db.SiteSettings.Add(new SiteSettingEntity { Key = key, Value = value, Group = group });
        }
        else
        {
            setting.Value = value;
            setting.Group = group ?? setting.Group;
            setting.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyList<ShowroomEntity>> GetShowroomsAsync(bool activeOnly = false)
    {
        var query = _db.Showrooms.AsNoTracking().AsQueryable();
        if (activeOnly) query = query.Where(s => s.IsActive);
        return await query.OrderBy(s => s.Name).ToListAsync();
    }

    public async Task<bool> SaveShowroomAsync(ShowroomEntity showroom)
    {
        if (showroom.Id == 0)
        {
            showroom.CreatedAt = DateTime.UtcNow;
            _db.Showrooms.Add(showroom);
        }
        else
        {
            var existing = await _db.Showrooms.FindAsync(showroom.Id);
            if (existing is null) return false;
            existing.Name = showroom.Name;
            existing.Code = showroom.Code;
            existing.Address = showroom.Address;
            existing.Phone = showroom.Phone;
            existing.Email = showroom.Email;
            existing.BusinessHours = showroom.BusinessHours;
            existing.IsActive = showroom.IsActive;
        }
        await _db.SaveChangesAsync();
        return true;
    }
}
