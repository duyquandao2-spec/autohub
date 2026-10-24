using System.Text.RegularExpressions;
using AutoHub.Models;
using AutoHub.Models.Entities;
using AutoHub.Services;
using AutoHub.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoHub.Pages.Admin.News;

[Authorize(Policy = Policies.ContentManagement)]
public class EditModel : PageModel
{
    private readonly IContentService _content;

    public EditModel(IContentService content) => _content = content;

    [FromRoute]
    public int? Id { get; set; }

    public bool IsNew => !Id.HasValue;
    public IReadOnlyList<NewsCategoryEntity> Categories { get; set; } = Array.Empty<NewsCategoryEntity>();
    public NewsArticleEntity? Existing { get; set; }

    [BindProperty]
    public string Title { get; set; } = string.Empty;

    [BindProperty]
    public string Slug { get; set; } = string.Empty;

    [BindProperty]
    public string? Excerpt { get; set; }

    [BindProperty]
    public new string Content { get; set; } = string.Empty;

    [BindProperty]
    public string? CoverImagePath { get; set; }

    [BindProperty]
    public int? CategoryId { get; set; }

    [BindProperty]
    public string Kind { get; set; } = "ARTICLE";

    [BindProperty]
    public string Status { get; set; } = "DRAFT";

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        Categories = await _content.GetCategoriesAsync();
        if (!id.HasValue) return Page();

        Existing = await _content.GetArticleAsync(id.Value);
        if (Existing is null) return NotFound();

        Title = Existing.Title; Slug = Existing.Slug; Excerpt = Existing.Excerpt;
        Content = Existing.Content; CoverImagePath = Existing.CoverImagePath;
        CategoryId = Existing.CategoryId; Kind = Existing.Kind; Status = Existing.Status;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Categories = await _content.GetCategoriesAsync();
        if (string.IsNullOrWhiteSpace(Title))
        {
            ModelState.AddModelError(string.Empty, "Tiêu đề không được để trống.");
            return Page();
        }

        if (string.IsNullOrWhiteSpace(Slug)) Slug = Slugify(Title);

        var article = new NewsArticleEntity
        {
            Title = Title.Trim(),
            Slug = Slug,
            Excerpt = Excerpt,
            Content = Content ?? string.Empty,
            CoverImagePath = CoverImagePath,
            CategoryId = CategoryId,
            Kind = Kind ?? "ARTICLE",
            Status = Status ?? "DRAFT",
            AuthorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            AuthorName = User.Identity?.Name,
            PublishedAt = (Status == "PUBLISHED") ? DateTime.UtcNow : null,
        };

        if (IsNew)
            await _content.CreateAsync(article);
        else
        {
            article.Id = Id!.Value;
            await _content.UpdateArticleAsync(article);
        }

        TempData["Ok"] = "Đã lưu bài viết.";
        return RedirectToPage("/Admin/News/Index");
    }

    private static string Slugify(string text)
    {
        var s = text.ToLowerInvariant().Trim();
        s = Regex.Replace(s, @"[^a-z0-9\s-]", "");
        s = Regex.Replace(s, @"[\s_]+", "-");
        return Regex.Replace(s, @"-+", "-").Trim('-');
    }
}