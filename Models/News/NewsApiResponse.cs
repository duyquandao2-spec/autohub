namespace AutoHub.Models.News;

public sealed class NewsApiResponse
{
    public List<AutomotiveNewsItem> Data { get; set; } = [];
    public NewsMeta Meta { get; set; } = new();
}

public sealed class NewsMeta
{
    public string Provider { get; set; } = string.Empty;
    public bool Cached { get; set; }
    public DateTime FetchedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int Count { get; set; }
}

public sealed class NewsErrorResponse
{
    public NewsError Error { get; set; } = new();
}

public sealed class NewsError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
