using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infoware.EntityFrameworkCore.MemoryCache
{
    public class EFCoreMemoryCache : IEFCoreCache
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<EFCoreMemoryCache> _logger;
        private readonly ConcurrentDictionary<string, string> _cachedKeys = new();
        private readonly ConcurrentQueue<string> _removeCachePendings = new();

        public EFCoreMemoryCache(IMemoryCache memoryCache, ILogger<EFCoreMemoryCache> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public bool TryGetValue<TItem>(string key, out TItem value)
        {
            if (_memoryCache.TryGetValue(key, out value))
            {
                _logger.LogInformation("Requesting from cache {key}", key);
                _cachedKeys.TryAdd(key, string.Empty);
                return true;
            }
            else
            {
                _logger.LogInformation("No cached yet {key}, requesting from database", key);
                return false;
            }
        }

        public Task<TItem?> GetAsync<TItem>(string key, CancellationToken cancellationToken = default)
        {
            var exists = TryGetValue<TItem>(key, out var result);
            return Task.FromResult(exists ? result : default);
        }

        public async Task IfExistsDoAsync<TItem>(string key, Func<TItem, Task> function, CancellationToken cancellationToken = default)
        {
            if (TryGetValue<TItem>(key, out var result))
            {
                await function(result);
            }
        }

        public Task IfExistsDoAsync<TItem>(string key, Action<TItem> function, CancellationToken cancellationToken = default)
        {
            if (TryGetValue<TItem>(key, out var result))
            {
                function(result);
            }
            return Task.CompletedTask;
        }

        public async Task<TItem?> GetOrCreateAsync<TItem>(string key, Func<TItem> factory,
            TimeSpan absoluteExpirationRelativeToNow, CancellationToken cancellationToken = default)
        {
            if (!TryGetValue<TItem>(key, out var result))
            {
                result = factory();
                if (result != null)
                {
                    await SetAsync(key, result, absoluteExpirationRelativeToNow);
                }
            }
            return result;
        }

        public async Task<TItem?> GetOrCreateAsync<TItem>(string key, Func<Task<TItem>> factory,
            TimeSpan absoluteExpirationRelativeToNow, CancellationToken cancellationToken = default)
        {
            if (!TryGetValue<TItem>(key, out var result))
            {
                result = await factory();
                if (result != null)
                {
                    await SetAsync(key, result, absoluteExpirationRelativeToNow);
                }
            }
            return result;
        }

        public void RemoveKeysStartsWith(string expression)
        {
            _logger.LogInformation("Removing from cache {expression}", expression);
            foreach (var item in _cachedKeys.Where(x => x.Key.StartsWith(expression)))
            {
                RemoveKey(item.Key);
            };
        }

        public async Task RemoveKeysStartsWithAsync(string expression)
        {
            _logger.LogInformation("Removing from cache {expression}", expression);
            var tasks = _cachedKeys
                .Where(x => x.Key.StartsWith(expression))
                .Select(x => RemoveKeyAsync(x.Key));

            await Task.WhenAll(tasks);
        }

        public void RemoveKey(string? key)
        {
            _cachedKeys!.Remove(key, out _);
            _memoryCache.Remove(key);
        }

        public Task RemoveKeyAsync(string? key)
        {
            RemoveKey(key);
            return Task.CompletedTask;
        }

        public void AddPendingRemoveKeysStartsWith(string expression)
        {
            _logger.LogInformation("Add Pending remove from cache {expression}", expression);
            _removeCachePendings.Enqueue(expression);
        }

        public void Set<TItem>(string key, TItem value, TimeSpan absoluteExpirationRelativeToNow)
        {
            _logger.LogInformation("Setting cache {key}", key);
            _memoryCache.Set(key, value, absoluteExpirationRelativeToNow);
            _cachedKeys.TryAdd(key, string.Empty);
        }

        public Task SetAsync<TItem>(string key, TItem result, TimeSpan absoluteExpirationRelativeToNow, 
            CancellationToken cancellationToken = default)
        {
            Set(key, result, absoluteExpirationRelativeToNow);
            return Task.CompletedTask;
        }

        public void RemoveAllPendingKeys()
        {
            _logger.LogInformation("Starting remove cache pendings");
            foreach (var pending in _removeCachePendings)
            {
                RemoveKeysStartsWith(pending);
            }
        }

        public async Task RemoveAllPendingKeysAsync()
        {
            _logger.LogInformation("Starting remove cache pendings");
            var tasks = _removeCachePendings
                .Select(x => RemoveKeysStartsWithAsync(x));

            await Task.WhenAll(tasks);
        }
    }
}
