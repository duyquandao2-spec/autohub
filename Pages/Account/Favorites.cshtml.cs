using AutoHub.Data;
using AutoHub.Models.Entities;
using AutoHub.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AutoHub.Pages.Account;

[Authorize]
public class FavoritesModel : PageModel
{
    private readonly ICurrentUser _current;
    private readonly IFavoriteService _favorites;
    private readonly AutoHubDbContext _db;

    public FavoritesModel(ICurrentUser current, IFavoriteService favorites, AutoHubDbContext db)
    {
        _current = current;
        _favorites = favorites;
        _db = db;
    }

    public List<FavoriteEntity> Favorites { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostAsync(int? remove)
    {
        if (remove.HasValue)
        {
            await _favorites.RemoveAsync(_current.UserId!, remove.Value);
            StatusMessage = "Đã bỏ xe khỏi danh sách yêu thích.";
        }
        await LoadAsync();
        return Page();
    }

    private async Task LoadAsync()
    {
        Favorites = await _db.Favorites.AsNoTracking()
            .Include(f => f.Vehicle).ThenInclude(v => v.Model)
            .Where(f => f.UserId == _current.UserId!)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }
}
