using GastroErp.Application.Common.Interfaces;
using GastroErp.Infrastructure.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GastroErp.Infrastructure.Cache;

/// <summary>
/// خدمة الـ Cache باستخدام الذاكرة (MemoryCache)
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly CacheOptions _cacheOptions;

    public MemoryCacheService(IMemoryCache memoryCache, IOptions<CacheOptions> cacheOptions)
    {
        _memoryCache = memoryCache;
        _cacheOptions = cacheOptions.Value;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default)
    {
        var options = new MemoryCacheEntryOptions
        {
            SlidingExpiration = slidingExpiration ?? TimeSpan.FromMinutes(_cacheOptions.DefaultExpirationMinutes)
        };

        _memoryCache.Set(key, value, options);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(key);
        return Task.CompletedTask;
    }
}
