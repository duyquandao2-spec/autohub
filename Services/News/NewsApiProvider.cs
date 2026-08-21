using AutoHub.Models.News;

namespace AutoHub.Services.News;

public interface INewsProvider
{
    string Name { get; }
    Task<IReadOnlyList<AutomotiveNewsItem>> FetchAsync(int maxResults, CancellationToken ct = default);
}

public sealed class NewsApiProvider : INewsProvider
{
    private readonly HttpClient _http;
    private readonly NewsConfig _config;
    private readonly ILogger<NewsApiProvider> _logger;

    public string Name => "newsapi";

    public NewsApiProvider(HttpClient http, NewsConfig config, ILogger<NewsApiProvider> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AutomotiveNewsItem>> FetchAsync(int maxResults, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_config.NewsApiKey))
        {
            _logger.LogWarning("NEWS_API_KEY is not configured");
            return [];
        }

        var endpoint = new Uri("https://newsapi.org/v2/everything");
        var query = System.Web.HttpUtility.ParseQueryString(string.Empty);
        query["q"] = "(electric vehicle OR performance car OR automotive OR EV battery OR charging infrastructure)";
        query["searchIn"] = "title,description";
        query["sortBy"] = "publishedAt";
        query["pageSize"] = maxResults.ToString();
        query["language"] = "en";

        var url = new Uri(endpoint, $"?{query}");

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("X-Api-Key", _config.NewsApiKey);

        using var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var doc = System.Text.Json.JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("articles", out var articles))
            return [];

        var allowedDomains = _config.AllowedDomains
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var items = new List<AutomotiveNewsItem>();

        foreach (var article in articles.EnumerateArray())
        {
            var sourceName = article.TryGetProperty("source", out var src) && src.TryGetProperty("name", out var name)
                ? name.GetString() ?? "Unknown" : "Unknown";

            var title = article.TryGetProperty("title", out var t) ? t.GetString() : null;
            var description = article.TryGetProperty("description", out var d) ? d.GetString() : null;
            var articleUrl = article.TryGetProperty("url", out var u) ? u.GetString() : null;
            var imageUrl = article.TryGetProperty("urlToImage", out var img) ? img.GetString() : null;
            var publishedAtStr = article.TryGetProperty("publishedAt", out var pa) ? pa.GetString() : null;

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(articleUrl))
                continue;

            if (!Uri.TryCreate(articleUrl, UriKind.Absolute, out var articleUri) ||
                (articleUri.Scheme != "http" && articleUri.Scheme != "https"))
                continue;

            if (imageUrl != null && !Uri.TryCreate(imageUrl, UriKind.Absolute, out _))
                imageUrl = null;

            if (allowedDomains.Count > 0 &&
                !allowedDomains.Contains(articleUri.Host) &&
                !string.Equals(sourceName, "Reuters", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!DateTime.TryParse(publishedAtStr, out var publishedAt))
                publishedAt = DateTime.UtcNow;

            var id = ComputeId(articleUrl);
            var category = Categorize(title, description);

            items.Add(new AutomotiveNewsItem
            {
                Id = id,
                SourceName = sourceName,
                SourceUrl = $"https://{articleUri.Host}",
                Title = title,
                Excerpt = description?.Length > 180 ? description[..177] + "..." : description,
                ArticleUrl = articleUrl,
                ImageUrl = imageUrl,
                PublishedAt = publishedAt,
                Category = category,
            });
        }

        return items;
    }

    private static string ComputeId(string url)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(url));
        return Convert.ToHexString(hash).ToLowerInvariant()[..16];
    }

    private static string Categorize(string title, string? description)
    {
        var text = $"{title} {description}".ToLowerInvariant();

        if (text.Contains("ev") || text.Contains("electric") || text.Contains("battery") || text.Contains("charging"))
            return "technology";
        if (text.Contains("performance") || text.Contains("racing") || text.Contains("track"))
            return "performance";
        if (text.Contains("market") || text.Contains("sale") || text.Contains("registration") || text.Contains("revenue"))
            return "market";

        return "brand";
    }
}
