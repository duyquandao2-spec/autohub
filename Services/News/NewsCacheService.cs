using AutoHub.Models.News;
using Microsoft.Extensions.Caching.Memory;

namespace AutoHub.Services.News;

public interface INewsCacheService
{
    IReadOnlyList<AutomotiveNewsItem>? GetCached(string key);
    void Set(string key, IReadOnlyList<AutomotiveNewsItem> items, TimeSpan ttl);
    DateTime? GetLastFetchedAt(string key);
    void SetLastFetchedAt(string key, DateTime time);
}

public sealed class NewsCacheService : INewsCacheService
{
    private readonly IMemoryCache _cache;

    public NewsCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public IReadOnlyList<AutomotiveNewsItem>? GetCached(string key)
    {
        _cache.TryGetValue(key, out IReadOnlyList<AutomotiveNewsItem>? items);
        return items;
    }

    public void Set(string key, IReadOnlyList<AutomotiveNewsItem> items, TimeSpan ttl)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl,
            Priority = CacheItemPriority.Normal,
        };
        _cache.Set(key, items, options);
    }

    public DateTime? GetLastFetchedAt(string key)
    {
        _cache.TryGetValue($"{key}:fetchedAt", out DateTime time);
        return time == default ? null : time;
    }

    public void SetLastFetchedAt(string key, DateTime time)
    {
        _cache.Set($"{key}:fetchedAt", time, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24),
        });
    }
}
