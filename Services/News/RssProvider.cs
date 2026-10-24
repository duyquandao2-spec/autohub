using System.Xml.Linq;
using AutoHub.Models.News;

namespace AutoHub.Services.News;

public sealed class RssProvider : INewsProvider
{
    private readonly HttpClient _http;
    private readonly NewsConfig _config;
    private readonly ILogger<RssProvider> _logger;

    public string Name => "rss";

    public RssProvider(HttpClient http, NewsConfig config, ILogger<RssProvider> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AutomotiveNewsItem>> FetchAsync(int maxResults, CancellationToken ct = default)
    {
        var feeds = _config.RssFeeds
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (feeds.Length == 0)
        {
            _logger.LogWarning("NEWS_RSS_FEEDS is not configured");
            return [];
        }

        var allItems = new List<AutomotiveNewsItem>();

        foreach (var feedUrl in feeds)
        {
            try
            {
                if (!Uri.TryCreate(feedUrl, UriKind.Absolute, out var feedUri))
                    continue;

                using var response = await _http.GetAsync(feedUri, ct);
                response.EnsureSuccessStatusCode();

                var bytes = await response.Content.ReadAsByteArrayAsync(ct);
                var xml = System.Text.Encoding.UTF8.GetString(bytes);
                var doc = XDocument.Parse(xml);

                var items = doc.Descendants("item")
                    .Concat(doc.Descendants("entry"))
                    .Take(maxResults)
                    .Select(el => ParseItem(el, feedUri))
                    .Where(i => i != null)
                    .Cast<AutomotiveNewsItem>();

                allItems.AddRange(items);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch RSS feed: {FeedUrl}", feedUrl);
            }
        }

        return NewsNormalizer.DedupeAndSort(allItems).Take(maxResults).ToList();
    }

    private static AutomotiveNewsItem? ParseItem(XElement item, Uri baseUri)
    {
        var title = item.Element("title")?.Value?.Trim();
        var link = item.Element("link")?.Value ?? ExtractAtomLink(item);
        var pubDate = item.Element("pubDate")?.Value;
        var description = item.Element("description")?.Value;

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(link))
            return null;

        if (!Uri.TryCreate(link, UriKind.Absolute, out var linkUri))
            return null;

        // Background image via shared normalizer; internal APEX fallback when missing.
        var imageUrl = NewsNormalizer.ExtractImageUrl(item, linkUri) ?? NewsNormalizer.FallbackImage;

        if (!DateTime.TryParse(pubDate, out var publishedAt))
            publishedAt = DateTime.UtcNow;

        var id = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(link))
        ).ToLowerInvariant()[..16];

        return new AutomotiveNewsItem
        {
            Id = id,
            SourceName = linkUri.Host,
            SourceUrl = $"https://{linkUri.Host}",
            Title = title,
            Excerpt = NewsNormalizer.CleanExcerpt(description),
            ArticleUrl = link,
            ImageUrl = imageUrl,
            PublishedAt = publishedAt,
            Category = "brand",
        };
    }

    private static string? ExtractAtomLink(XElement item) =>
        item.Elements()
            .FirstOrDefault(e => e.Name.LocalName == "link")
            ?.Attribute("href")?.Value;
}
