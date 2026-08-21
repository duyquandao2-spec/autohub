namespace AutoHub.Models.News;

public sealed class AutomotiveNewsItem
{
    public string Id { get; set; } = string.Empty;
    public string SourceName { get; set; } = string.Empty;
    public string? SourceUrl { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Excerpt { get; set; }
    public string ArticleUrl { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTime PublishedAt { get; set; }
    public string Category { get; set; } = "market";
    public FeaturedMetric? FeaturedMetric { get; set; }
}

public sealed class FeaturedMetric
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool Verified { get; set; }
}
