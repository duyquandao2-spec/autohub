using AutoHub.Data;
using AutoHub.Models.Entities;
using AutoHub.Services.Auth;
using Microsoft.EntityFrameworkCore;

namespace AutoHub.Services;

public interface IContentService
{
    Task<IPagedResult<NewsArticleEntity>> GetArticlesAsync(int page, int pageSize, string? status, string? kind, string? search);
    Task<NewsArticleEntity> CreateAsync(NewsArticleEntity article);
    Task<NewsArticleEntity?> GetArticleAsync(int id);
    Task<bool> UpdateArticleAsync(NewsArticleEntity article);
    Task<bool> SetStatusAsync(int id, string status);
    Task<IReadOnlyList<NewsCategoryEntity>> GetCategoriesAsync();
    Task<NewsArticleEntity?> GetPublishedBySlugAsync(string slug);
}

public sealed class ContentService : IContentService
{
    private readonly AutoHubDbContext _db;

    public ContentService(AutoHubDbContext db) => _db = db;

    public async Task<IPagedResult<NewsArticleEntity>> GetArticlesAsync(int page, int pageSize, string? status, string? kind, string? search)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.NewsArticles.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(a => a.Status == status);
        if (!string.IsNullOrWhiteSpace(kind)) query = query.Where(a => a.Kind == kind);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(a => a.Title.Contains(term) || (a.AuthorName != null && a.AuthorName.Contains(term)));
        }

        var totalItems = await query.CountAsync();
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling((double)totalItems / pageSize);
        page = totalPages == 0 ? 1 : Math.Min(page, totalPages);

        var items = await query.OrderByDescending(a => a.UpdatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<NewsArticleEntity>(items, page, pageSize, totalItems, totalPages);
    }

    public async Task<NewsArticleEntity> CreateAsync(NewsArticleEntity article)
    {
        article.CreatedAt = DateTime.UtcNow;
        article.UpdatedAt = article.CreatedAt;
        _db.NewsArticles.Add(article);
        await _db.SaveChangesAsync();
        return article;
    }

    public async Task<NewsArticleEntity?> GetArticleAsync(int id)
        => await _db.NewsArticles.Include(a => a.Category).FirstOrDefaultAsync(a => a.Id == id);

    public async Task<bool> UpdateArticleAsync(NewsArticleEntity article)
    {
        var existing = await _db.NewsArticles.FindAsync(article.Id);
        if (existing is null) return false;

        existing.Title = article.Title;
        existing.Slug = article.Slug;
        existing.Excerpt = article.Excerpt;
        existing.Content = article.Content;
        existing.CoverImagePath = article.CoverImagePath;
        existing.CategoryId = article.CategoryId;
        existing.Kind = article.Kind;
        existing.Status = article.Status;
        existing.AuthorName = article.AuthorName;
        if (article.Status == "PUBLISHED") existing.PublishedAt = article.PublishedAt ?? DateTime.UtcNow;
        existing.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetStatusAsync(int id, string status)
    {
        var article = await _db.NewsArticles.FindAsync(id);
        if (article is null) return false;

        article.Status = status;
        if (status == "PUBLISHED" && article.PublishedAt is null) article.PublishedAt = DateTime.UtcNow;
        if (status == "ARCHIVED") article.IsDeleted = true;
        article.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyList<NewsCategoryEntity>> GetCategoriesAsync()
        => await _db.NewsCategories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();

    public async Task<NewsArticleEntity?> GetPublishedBySlugAsync(string slug)
        => await _db.NewsArticles.AsNoTracking()
            .Include(a => a.Category)
            .FirstOrDefaultAsync(a => a.Slug == slug && a.Status == "PUBLISHED" && !a.IsDeleted);
}
