using AutoHub.Data;
using AutoHub.Models;
using AutoHub.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoHub.Services.Auth;

public interface IFavoriteService
{
    Task<List<int>> GetFavoriteIdsAsync(string userId);
    Task<bool> AddAsync(string userId, int vehicleId);
    Task<bool> RemoveAsync(string userId, int vehicleId);
}

public sealed class FavoriteService : IFavoriteService
{
    private readonly AutoHubDbContext _db;

    public FavoriteService(AutoHubDbContext db) => _db = db;

    public async Task<List<int>> GetFavoriteIdsAsync(string userId)
        => await _db.Favorites.AsNoTracking()
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => f.VehicleId)
            .ToListAsync();

    public async Task<bool> AddAsync(string userId, int vehicleId)
    {
        if (!await _db.VehicleVariants.AnyAsync(v => v.VariantId == vehicleId)) return false;
        if (await _db.Favorites.AnyAsync(f => f.UserId == userId && f.VehicleId == vehicleId)) return false;

        _db.Favorites.Add(new FavoriteEntity { UserId = userId, VehicleId = vehicleId });
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveAsync(string userId, int vehicleId)
    {
        var fav = await _db.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && f.VehicleId == vehicleId);
        if (fav is null) return false;

        _db.Favorites.Remove(fav);
        await _db.SaveChangesAsync();
        return true;
    }
}
