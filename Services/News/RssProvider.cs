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

                var xml = await response.Content.ReadAsStringAsync(ct);
                var doc = XDocument.Parse(xml);

                var ns = doc.Root?.Name.Namespace ?? XNamespace.None;

                var items = doc.Descendants("item")
                    .Concat(doc.Descendants(ns + "item"))
                    .Take(maxResults)
                    .Select(ParseItem)
                    .Where(i => i != null)
                    .Cast<AutomotiveNewsItem>();

                allItems.AddRange(items);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch RSS feed: {FeedUrl}", feedUrl);
            }
        }

        return allItems
            .OrderByDescending(i => i.PublishedAt)
            .Take(maxResults)
            .ToList();
    }

    private static AutomotiveNewsItem? ParseItem(XElement item)
    {
        var title = item.Element("title")?.Value;
        var link = item.Element("link")?.Value;
        var pubDate = item.Element("pubDate")?.Value;
        var description = item.Element("description")?.Value;

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(link))
            return null;

        if (!Uri.TryCreate(link, UriKind.Absolute, out var linkUri))
            return null;

        string? imageUrl = null;
        var mediaContent = item.Element(XNamespace.None + "content") ??
                           item.Element(XNamespace.Get("http://search.yahoo.com/mrss/") + "content");
        var mediaThumbnail = item.Element(XNamespace.None + "thumbnail") ??
                             item.Element(XNamespace.Get("http://search.yahoo.com/mrss/") + "thumbnail");
        var enclosure = item.Element("enclosure");

        if (mediaContent?.Attribute("url")?.Value is { Length: > 0 } mcUrl &&
            Uri.TryCreate(mcUrl, UriKind.Absolute, out _))
            imageUrl = mcUrl;
        else if (mediaThumbnail?.Attribute("url")?.Value is { Length: > 0 } mtUrl &&
                 Uri.TryCreate(mtUrl, UriKind.Absolute, out _))
            imageUrl = mtUrl;
        else if (enclosure?.Attribute("url")?.Value is { Length: > 0 } enUrl &&
                 Uri.TryCreate(enUrl, UriKind.Absolute, out _) &&
                 (enclosure.Attribute("type")?.Value?.StartsWith("image") == true))
            imageUrl = enUrl;

        if (!DateTime.TryParse(pubDate, out var publishedAt))
            publishedAt = DateTime.UtcNow;

        var id = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(link))
        ).ToLowerInvariant()[..16];

        var cleanDescription = System.Net.WebUtility.HtmlDecode(description);
        if (cleanDescription != null && cleanDescription.Length > 180)
            cleanDescription = cleanDescription[..177] + "...";

        return new AutomotiveNewsItem
        {
            Id = id,
            SourceName = linkUri.Host,
            SourceUrl = $"https://{linkUri.Host}",
            Title = title,
            Excerpt = cleanDescription,
            ArticleUrl = link,
            ImageUrl = imageUrl,
            PublishedAt = publishedAt,
            Category = "brand",
        };
    }
}
