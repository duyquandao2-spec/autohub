using AutoHub.Models.News;

namespace AutoHub.Services.News;

public sealed class GNewsProvider : INewsProvider
{
    private readonly HttpClient _http;
    private readonly NewsConfig _config;
    private readonly ILogger<GNewsProvider> _logger;

    public string Name => "gnews";

    public GNewsProvider(HttpClient http, NewsConfig config, ILogger<GNewsProvider> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AutomotiveNewsItem>> FetchAsync(int maxResults, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_config.GNewsApiKey))
        {
            _logger.LogWarning("GNEWS_API_KEY is not configured");
            return [];
        }

        var endpoint = new Uri("https://gnews.io/api/v4/search");
        var query = System.Web.HttpUtility.ParseQueryString(string.Empty);
        query["q"] = "electric vehicle OR automotive OR performance car";
        query["lang"] = "en";
        query["max"] = maxResults.ToString();
        query["apikey"] = _config.GNewsApiKey;

        var url = new Uri(endpoint, $"?{query}");

        using var response = await _http.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var doc = System.Text.Json.JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("articles", out var articles))
            return [];

        var items = new List<AutomotiveNewsItem>();

        foreach (var article in articles.EnumerateArray())
        {
            var sourceName = article.TryGetProperty("source", out var src) && src.TryGetProperty("name", out var name)
                ? name.GetString() ?? "Unknown" : "Unknown";
            var title = article.TryGetProperty("title", out var t) ? t.GetString() : null;
            var description = article.TryGetProperty("description", out var d) ? d.GetString() : null;
            var articleUrl = article.TryGetProperty("url", out var u) ? u.GetString() : null;
            var imageUrl = article.TryGetProperty("image", out var img) ? img.GetString() : null;
            var publishedAtStr = article.TryGetProperty("publishedAt", out var pa) ? pa.GetString() : null;

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(articleUrl))
                continue;

            if (!Uri.TryCreate(articleUrl, UriKind.Absolute, out var articleUri) ||
                (articleUri.Scheme != "http" && articleUri.Scheme != "https"))
                continue;

            if (imageUrl != null && !Uri.TryCreate(imageUrl, UriKind.Absolute, out _))
                imageUrl = null;

            if (!DateTime.TryParse(publishedAtStr, out var publishedAt))
                publishedAt = DateTime.UtcNow;

            var id = Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(articleUrl))
            ).ToLowerInvariant()[..16];

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
                Category = "brand",
            });
        }

        return items;
    }
}
