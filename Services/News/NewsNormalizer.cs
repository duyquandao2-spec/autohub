using System.Xml.Linq;
using AutoHub.Models.News;

namespace AutoHub.Services.News;

/// <summary>
/// Shared normalization pipeline for RSS/Feed items:
/// image extraction (media:content, media:thumbnail, enclosure, first &lt;img&gt;),
/// URL validation, de-duplication by article URL and newest-first ordering.
/// Guarantees every item carries a usable background image (internal APEX fallback).
/// </summary>
public static class NewsNormalizer
{
    private static readonly XNamespace MediaNs = "http://search.yahoo.com/mrss/";

    /// <summary>Internal, always-available fallback used when a feed item has no image.</summary>
    public const string FallbackImage = "/Images/news/news-fallback.svg";

    /// <summary>
    /// Extracts the best background image for an RSS item.
    /// Priority: media:content → media:thumbnail → enclosure(image) → first &lt;img src&gt; in HTML body.
    /// </summary>
    public static string? ExtractImageUrl(XElement item, Uri baseUri)
    {
        var candidates = new[]
        {
            item.Elements().FirstOrDefault(e => e.Name.LocalName == "content" && e.Attribute("url") != null),
            item.Elements().FirstOrDefault(e => e.Name.LocalName == "thumbnail" && e.Attribute("url") != null),
            item.Elements().FirstOrDefault(e => e.Name.LocalName == "enclosure"
                && e.Attribute("url") != null
                && (e.Attribute("type")?.Value?.StartsWith("image", StringComparison.OrdinalIgnoreCase) ?? true)),
        };

        foreach (var el in candidates)
        {
            var url = el?.Attribute("url")?.Value;
            if (IsValidHttpImage(url))
                return Resolve(url!, baseUri);
        }

        // Fallback: scan the first <img> inside description / content:encoded / summary.
        foreach (var field in new[] { "encoded", "description", "summary", "content" })
        {
            var html = item.Elements()
                .FirstOrDefault(e => e.Name.LocalName == field)?.Value;
            if (string.IsNullOrWhiteSpace(html))
                continue;

            var img = FirstImgSrc(html);
            if (img != null && IsValidHttpImage(img))
                return Resolve(img, baseUri);
        }

        return null;
    }

    /// <summary>Returns the first <c>&lt;img src="..."&gt;</c> URL found in an HTML fragment.</summary>
    public static string? FirstImgSrc(string html)
    {
        var span = html.AsSpan();
        var lower = html.ToLowerInvariant();

        var idx = lower.IndexOf("<img", StringComparison.Ordinal);
        while (idx >= 0 && idx < span.Length)
        {
            var srcIdx = lower.IndexOf("src=", idx, StringComparison.Ordinal);
            if (srcIdx < 0) return null;

            var start = srcIdx + 4;
            if (start >= span.Length) return null;

            var quote = span[start];
            if (quote is '"' or '\'')
            {
                var end = lower.IndexOf(quote, start + 1);
                if (end < 0) return null;
                return html.Substring(start + 1, end - start - 1).Trim();
            }

            // Unquoted attribute value.
            var end2 = start;
            while (end2 < span.Length && !char.IsWhiteSpace(span[end2]) && span[end2] != '>')
                end2++;
            return html.Substring(start, end2 - start).Trim();
        }

        return null;
    }

    /// <summary>Validates the URL is absolute http(s) and looks like an image resource.</summary>
    public static bool IsValidHttpImage(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        url = url.Trim();

        // Skip data URIs, tracking pixels and lazy-load placeholders.
        if (url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            return false;
        if (url.Contains("1x1", StringComparison.OrdinalIgnoreCase) ||
            url.Contains("pixel", StringComparison.OrdinalIgnoreCase) ||
            url.Contains("spacer", StringComparison.OrdinalIgnoreCase))
            return false;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        if (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
            return false;

        var ext = Path.GetExtension(uri.AbsolutePath);

        // Known image extensions are always accepted.
        if (ext is ".jpg" or ".jpeg" or ".png" or ".webp" or ".gif" or ".avif" or ".svg")
            return true;

        // Known non-image resources are rejected.
        if (ext is ".html" or ".htm" or ".php" or ".js" or ".css" or ".aspx")
            return false;

        // Extension-less CDN URLs (e.g. .../images/xxx?crop=...) are accepted.
        return true;
    }

    /// <summary>Resolves protocol-relative ("//cdn...") and root-relative ("/img/...") URLs against the feed/article base.</summary>
    public static string Resolve(string url, Uri baseUri)
    {
        url = url.Trim();
        if (url.StartsWith("//"))
            return "https:" + url;
        if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var rel) && !rel.IsAbsoluteUri && baseUri != null)
            return new Uri(baseUri, rel).ToString();
        return url;
    }

    /// <summary>Strips HTML tags and decodes entities, then trims to a safe excerpt length.</summary>
    public static string? CleanExcerpt(string? html, int maxLength = 180)
    {
        if (string.IsNullOrWhiteSpace(html))
            return null;

        var text = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*>", " ");
        text = System.Net.WebUtility.HtmlDecode(text);
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();

        if (text.Length <= maxLength)
            return text.Length == 0 ? null : text;

        return text[..(maxLength - 3)].TrimEnd() + "...";
    }

    /// <summary>Removes duplicate articles by normalized URL and orders them newest-first.</summary>
    public static IReadOnlyList<AutomotiveNewsItem> DedupeAndSort(IEnumerable<AutomotiveNewsItem> items)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        return items
            .Where(i => seen.Add(NormalizeUrlKey(i.ArticleUrl)))
            .OrderByDescending(i => i.PublishedAt)
            .ToList();
    }

    private static string NormalizeUrlKey(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return url.Trim().ToLowerInvariant();

        var query = string.Join("&", System.Web.HttpUtility.ParseQueryString(uri.Query).AllKeys
            .Where(k => k != null && !k.StartsWith("utm_", StringComparison.OrdinalIgnoreCase) && k != "fbclid" && k != "gclid")
            .Select(k => $"{k}={System.Web.HttpUtility.ParseQueryString(uri.Query)[k]}"));

        var path = uri.AbsolutePath.TrimEnd('/');
        return $"{uri.Host}{path}{(query.Length > 0 ? "?" + query : "")}".ToLowerInvariant();
    }
}
