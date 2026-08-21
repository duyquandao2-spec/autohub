using AutoHub.Models.News;

namespace AutoHub.Services.News;

public interface INewsService
{
    Task<(IReadOnlyList<AutomotiveNewsItem> Items, NewsMeta Meta)> GetNewsAsync(
        int limit, string? category, bool forceRefresh, CancellationToken ct = default);

    Task<Dictionary<string, object>> GetHealthAsync();
}

public sealed class NewsService : INewsService
{
    private readonly INewsProvider _provider;
    private readonly INewsCacheService _cache;
    private readonly NewsConfig _config;
    private readonly ILogger<NewsService> _logger;

    private const string CacheKey = "automotive-news";

    public NewsService(
        INewsProvider provider,
        INewsCacheService cache,
        NewsConfig config,
        ILogger<NewsService> logger)
    {
        _provider = provider;
        _cache = cache;
        _config = config;
        _logger = logger;
    }

    public async Task<(IReadOnlyList<AutomotiveNewsItem> Items, NewsMeta Meta)> GetNewsAsync(
        int limit, string? category, bool forceRefresh, CancellationToken ct = default)
    {
        limit = Math.Clamp(limit, 1, 6);
        var now = DateTime.UtcNow;
        var ttl = TimeSpan.FromSeconds(_config.CacheTtlSeconds);

        if (!forceRefresh)
        {
            var cached = _cache.GetCached(CacheKey);
            if (cached != null)
            {
                var expiresAt = (_cache.GetLastFetchedAt(CacheKey) ?? now) + ttl;
                var filtered = FilterByCategory(cached, category);
                var limited = filtered.Take(limit).ToList();

                return (limited, new NewsMeta
                {
                    Provider = _provider.Name,
                    Cached = true,
                    FetchedAt = _cache.GetLastFetchedAt(CacheKey) ?? now,
                    ExpiresAt = expiresAt,
                    Count = limited.Count,
                });
            }
        }

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromMilliseconds(_config.FetchTimeoutMs));

            var items = await FetchWithRetryAsync(_provider, _config.MaxResults, cts.Token);
            var fetchedAt = DateTime.UtcNow;

            _cache.Set(CacheKey, items, ttl);
            _cache.SetLastFetchedAt(CacheKey, fetchedAt);

            var filteredResult = FilterByCategory(items, category);
            var limitedResult = filteredResult.Take(limit).ToList();

            return (limitedResult, new NewsMeta
            {
                Provider = _provider.Name,
                Cached = false,
                FetchedAt = fetchedAt,
                ExpiresAt = fetchedAt + ttl,
                Count = limitedResult.Count,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch news from provider {Provider}", _provider.Name);

            var staleCache = _cache.GetCached(CacheKey);
            if (staleCache != null)
            {
                _logger.LogWarning("Returning stale cache due to provider failure");
                var staleFiltered = FilterByCategory(staleCache, category);
                var staleLimited = staleFiltered.Take(limit).ToList();

                return (staleLimited, new NewsMeta
                {
                    Provider = _provider.Name,
                    Cached = true,
                    FetchedAt = _cache.GetLastFetchedAt(CacheKey) ?? DateTime.MinValue,
                    ExpiresAt = DateTime.MinValue,
                    Count = staleLimited.Count,
                });
            }

            return ([], new NewsMeta
            {
                Provider = _provider.Name,
                Cached = false,
                FetchedAt = now,
                ExpiresAt = now,
                Count = 0,
            });
        }
    }

    public Task<Dictionary<string, object>> GetHealthAsync()
    {
        var lastFetch = _cache.GetLastFetchedAt(CacheKey);
        var ttl = TimeSpan.FromSeconds(_config.CacheTtlSeconds);
        var cacheAge = lastFetch.HasValue ? (DateTime.UtcNow - lastFetch.Value).TotalSeconds : -1;

        return Task.FromResult<Dictionary<string, object>>(new()
        {
            ["ok"] = true,
            ["provider"] = _provider.Name,
            ["cacheAgeSeconds"] = Math.Max(0, (int)cacheAge),
            ["lastSuccessfulFetch"] = lastFetch?.ToString("o") ?? "",
        });
    }

    private async Task<IReadOnlyList<AutomotiveNewsItem>> FetchWithRetryAsync(
        INewsProvider provider, int maxResults, CancellationToken ct)
    {
        const int maxRetries = 1;
        for (var attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                return await provider.FetchAsync(maxResults, ct);
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                _logger.LogWarning(ex, "Retry {Attempt}/{MaxRetries} for provider {Provider}",
                    attempt + 1, maxRetries, provider.Name);
                await Task.Delay(200, ct);
            }
        }

        throw new InvalidOperationException($"Provider {provider.Name} failed after {maxRetries + 1} attempts");
    }

    private static IReadOnlyList<AutomotiveNewsItem> FilterByCategory(
        IReadOnlyList<AutomotiveNewsItem> items, string? category)
    {
        if (string.IsNullOrWhiteSpace(category) || category.Equals("all", StringComparison.OrdinalIgnoreCase))
            return items;

        return items.Where(i =>
            string.Equals(i.Category, category, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}
