using System.Xml.Linq;
using AutoHub.Models.News;

namespace AutoHub.Services.News;

/// <summary>
/// Hybrid multi-source RSS provider (NEWS_PROVIDER=hybrid_rss).
/// Fans out to every feed configured in NEWS_RSS_FEEDS in parallel, normalizes
/// items through <see cref="NewsNormalizer"/> (guaranteed background image),
/// de-duplicates by URL and orders newest-first.
/// </summary>
public sealed class HybridRssProvider : INewsProvider
{
    private readonly HttpClient _http;
    private readonly NewsConfig _config;
    private readonly ILogger<HybridRssProvider> _logger;

    public string Name => "hybrid_rss";

    // Friendly display names shown on the cards and the VERIFIED SOURCES footer.
    private static readonly Dictionary<string, string> SourceNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["autodaily.vn"] = "AUTODAILY",
        ["forum.autodaily.vn"] = "AUTODAILY",
        ["cartimes.tapchicongthuong.vn"] = "CARTIMES",
        ["tapchicongthuong.vn"] = "CARTIMES",
        ["www.caranddriver.com"] = "CAR AND DRIVER",
        ["caranddriver.com"] = "CAR AND DRIVER",
    };

    public HybridRssProvider(HttpClient http, NewsConfig config, ILogger<HybridRssProvider> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;

        // Browser-like UA: several VN publishers reject the default .NET user agent.
        if (!_http.DefaultRequestHeaders.Contains("User-Agent"))
            _http.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0 Safari/537.36");
    }

    public async Task<IReadOnlyList<AutomotiveNewsItem>> FetchAsync(int maxResults, CancellationToken ct = default)
    {
        var feeds = _config.RssFeeds
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(f => Uri.TryCreate(f, UriKind.Absolute, out var u) &&
                        (u.Scheme == Uri.UriSchemeHttps || u.Scheme == Uri.UriSchemeHttp))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (feeds.Length == 0)
        {
            _logger.LogWarning("NEWS_RSS_FEEDS is not configured for hybrid_rss provider");
            return [];
        }

        // Fan-out: fetch every source concurrently; a failing feed never blocks the others.
        var tasks = feeds.Select(url => FetchFeedAsync(url, maxResults, ct));
        var results = await Task.WhenAll(tasks);

        var merged = NewsNormalizer.DedupeAndSort(results.SelectMany(r => r));

        _logger.LogInformation(
            "hybrid_rss fetched {Total} unique articles from {Feeds} feeds", merged.Count, feeds.Length);

        return merged.Take(maxResults).ToList();
    }

    private async Task<IReadOnlyList<AutomotiveNewsItem>> FetchFeedAsync(string feedUrl, int maxResults, CancellationToken ct)
    {
        try
        {
            using var response = await _http.GetAsync(feedUrl, ct);
            response.EnsureSuccessStatusCode();

            // Decode as UTF-8 explicitly: some feeds declare a wrong/broad charset
            // which would corrupt Vietnamese diacritics via ReadAsStringAsync.
            var bytes = await response.Content.ReadAsByteArrayAsync(ct);
            var xml = System.Text.Encoding.UTF8.GetString(bytes);
            var doc = XDocument.Parse(xml);

            var feedBaseUri = new Uri(feedUrl);
            var sourceName = ResolveSourceName(feedBaseUri);

            // RSS 2.0 <item> + Atom <entry> support.
            var elements = doc.Descendants("item")
                .Concat(doc.Descendants("entry"))
                .Take(Math.Max(maxResults * 2, 12));

            var items = new List<AutomotiveNewsItem>();

            foreach (var el in elements)
            {
                var item = ParseElement(el, feedBaseUri, sourceName);
                if (item != null)
                    items.Add(item);
            }

            return items;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "hybrid_rss failed to fetch feed: {FeedUrl}", feedUrl);
            return [];
        }
    }

    private AutomotiveNewsItem? ParseElement(XElement el, Uri baseUri, string sourceName)
    {
        var title = HtmlDecode(el.Elements().FirstOrDefault(e => e.Name.LocalName == "title")?.Value);
        var link = ExtractLink(el);

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(link))
            return null;

        if (!Uri.TryCreate(link, UriKind.Absolute, out var linkUri) ||
            (linkUri.Scheme != Uri.UriSchemeHttps && linkUri.Scheme != Uri.UriSchemeHttp))
            return null;

        var publishedAt = ExtractPublishedAt(el);
        var rawBody = el.Elements().FirstOrDefault(e => e.Name.LocalName == "encoded")?.Value
                      ?? el.Elements().FirstOrDefault(e => e.Name.LocalName == "description")?.Value
                      ?? el.Elements().FirstOrDefault(e => e.Name.LocalName == "summary")?.Value
                      ?? el.Elements().FirstOrDefault(e => e.Name.LocalName == "content")?.Value;

        // Background image: media tags → enclosure → first <img> in body. Never empty.
        var imageUrl = NewsNormalizer.ExtractImageUrl(el, linkUri) ?? NewsNormalizer.FallbackImage;

        return new AutomotiveNewsItem
        {
            Id = ComputeId(link),
            SourceName = sourceName,
            SourceUrl = $"https://{linkUri.Host}",
            Title = title,
            Excerpt = NewsNormalizer.CleanExcerpt(rawBody),
            ArticleUrl = link,
            ImageUrl = imageUrl,
            PublishedAt = publishedAt,
            Category = Categorize(title),
        };
    }

    private static string? ExtractLink(XElement el)
    {
        // RSS 2.0: <link>text</link>
        var rssLink = el.Element("link")?.Value?.Trim();
        if (!string.IsNullOrEmpty(rssLink))
            return rssLink;

        // Atom: <link href="..." rel="alternate|"/>
        var atomLink = el.Elements()
            .Where(e => e.Name.LocalName == "link")
            .OrderBy(l => l.Attribute("rel")?.Value == "alternate" ? 0 : 1)
            .FirstOrDefault()
            ?.Attribute("href")?.Value;

        return string.IsNullOrWhiteSpace(atomLink) ? null : atomLink.Trim();
    }

    private static DateTime ExtractPublishedAt(XElement el)
    {
        foreach (var field in new[] { "pubDate", "published", "updated", "date" })
        {
            var raw = el.Elements().FirstOrDefault(e => e.Name.LocalName == field)?.Value;
            if (DateTime.TryParse(raw, System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
                    out var dt))
                return dt == default ? DateTime.UtcNow : dt;
        }

        return DateTime.UtcNow;
    }

    private static string? HtmlDecode(string? value) =>
        string.IsNullOrWhiteSpace(value) ? value : System.Net.WebUtility.HtmlDecode(value).Trim();

    private static string ResolveSourceName(Uri feedUri) =>
        SourceNames.TryGetValue(feedUri.Host, out var name)
            ? name
            : feedUri.Host.Replace("www.", "", StringComparison.OrdinalIgnoreCase).ToUpperInvariant();

    private static string ComputeId(string url) =>
        Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(url))
        ).ToLowerInvariant()[..16];

    private static string Categorize(string title)
    {
        var text = title.ToLowerInvariant();

        if (text.Contains("ev") || text.Contains("điện") || text.Contains("electric") ||
            text.Contains("pin") || text.Contains("battery") || text.Contains("sạc") || text.Contains("charging"))
            return "technology";
        if (text.Contains("hiệu năng") || text.Contains("performance") || text.Contains("racing") ||
            text.Contains("đua") || text.Contains("track"))
            return "performance";
        if (text.Contains("doanh số") || text.Contains("market") || text.Contains("bán") ||
            text.Contains("sale") || text.Contains("registration"))
            return "market";

        return "brand";
    }
}
