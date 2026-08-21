namespace AutoHub.Models.News;

public sealed class NewsConfig
{
    public string Provider { get; set; } = "newsapi";
    public string? NewsApiKey { get; set; }
    public string? GNewsApiKey { get; set; }
    public int CacheTtlSeconds { get; set; } = 1200;
    public int FetchTimeoutMs { get; set; } = 8000;
    public int MaxResults { get; set; } = 6;
    public int PublicLimit { get; set; } = 3;
    public string AllowedDomains { get; set; } = string.Empty;
    public string RssFeeds { get; set; } = string.Empty;
    public string? AdminRefreshSecret { get; set; }
}
