namespace AutoHub.Models.Entities;

public class NewsArticleEntity
{
    public int Id { get; set; }
    public int? CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Excerpt { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? CoverImagePath { get; set; }
    public string? AuthorId { get; set; }
    public string? AuthorName { get; set; }
    public string Kind { get; set; } = "ARTICLE";
    public string Status { get; set; } = "DRAFT";
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }

    public NewsCategoryEntity? Category { get; set; }
}
