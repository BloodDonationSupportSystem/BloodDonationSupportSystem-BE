using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<MemoryCacheService> _logger;
        private readonly ConcurrentDictionary<string, byte> _cacheKeys;
        private readonly JsonSerializerOptions _jsonOptions;

        public MemoryCacheService(
            IMemoryCache memoryCache,
            ILogger<MemoryCacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
            _cacheKeys = new ConcurrentDictionary<string, byte>();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };
        }

        public Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                if (_memoryCache.TryGetValue(key, out T? cachedValue))
                {
                    _logger.LogDebug("Cache hit for key: {Key}", key);
                    return Task.FromResult(cachedValue);
                }

                _logger.LogDebug("Cache miss for key: {Key}", key);
                return Task.FromResult<T?>(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cache for key: {Key}", key);
                return Task.FromResult<T?>(null);
            }
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            try
            {
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(10),
                    SlidingExpiration = TimeSpan.FromMinutes(2)
                };

                _memoryCache.Set(key, value, cacheOptions);
                _cacheKeys.TryAdd(key, 0);

                _logger.LogDebug("Cached value for key: {Key} with expiration: {Expiration}", 
                    key, expiration ?? TimeSpan.FromMinutes(10));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache for key: {Key}", key);
            }

            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            try
            {
                _memoryCache.Remove(key);
                _cacheKeys.TryRemove(key, out _);
                _logger.LogDebug("Removed cache for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache for key: {Key}", key);
            }

            return Task.CompletedTask;
        }

        public Task RemoveByPrefixAsync(string prefix)
        {
            try
            {
                var keysToRemove = new System.Collections.Generic.List<string>();
                
                foreach (var key in _cacheKeys.Keys)
                {
                    if (key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        keysToRemove.Add(key);
                    }
                }

                foreach (var key in keysToRemove)
                {
                    _memoryCache.Remove(key);
                    _cacheKeys.TryRemove(key, out _);
                }

                _logger.LogDebug("Removed {Count} cache entries with prefix: {Prefix}", 
                    keysToRemove.Count, prefix);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache by prefix: {Prefix}", prefix);
            }

            return Task.CompletedTask;
        }
    }
}
