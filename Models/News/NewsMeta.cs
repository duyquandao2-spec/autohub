namespace AutoHub.Models.News;

public sealed class NewsMeta
{
    public string Provider { get; set; } = string.Empty;
    public bool Cached { get; set; }
    public DateTime FetchedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int Count { get; set; }
}
