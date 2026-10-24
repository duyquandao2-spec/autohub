using AutoHub.Models;
using AutoHub.Models.Entities;
using AutoHub.Services;
using AutoHub.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoHub.Pages.Admin.News;

[Authorize(Policy = Policies.ContentManagement)]
public class IndexModel : PageModel
{
    private readonly IContentService _content;

    public IndexModel(IContentService content) => _content = content;

    public IPagedResult<NewsArticleEntity> Paged { get; set; } = null!;

    [FromQuery(Name = "page")]
    public int PageNumber { get; set; } = 1;

    [FromQuery]
    public string? Status { get; set; }

    [FromQuery]
    public string? Search { get; set; }

    public async Task OnGetAsync()
    {
        Paged = await _content.GetArticlesAsync(PageNumber, 20, Status, null, Search);
    }

    public async Task<IActionResult> OnPostPublishAsync(int id)
    {
        await _content.SetStatusAsync(id, "PUBLISHED");
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostArchiveAsync(int id)
    {
        await _content.SetStatusAsync(id, "ARCHIVED");
        return RedirectToPage();
    }
}
