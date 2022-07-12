using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infoware.EntityFrameworkCore.MemoryCache
{
    public class EFCoreMemoryCache : IEFCoreMemoryCache
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<EFCoreMemoryCache> _logger;
        private readonly Dictionary<string, string> _cachedKeys = new();
        private readonly List<string> _removeCachePendings = new();

        public EFCoreMemoryCache(IMemoryCache memoryCache, ILogger<EFCoreMemoryCache> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public bool TryGetValue<TItem>(string key, out TItem value)
        {
            _cachedKeys.Remove(key);
            if (_memoryCache.TryGetValue(key, out value))
            {
                _logger.LogInformation("Requesting from cache {key}", key);
                _cachedKeys.TryAdd(key, "");
                return true;
            }
            else
            {
                _logger.LogInformation("No cached yet {key}, requesting to database", key);
            }
            return false;
        }

        public void RemoveKeysStartsWith(string expression)
        {
            _logger.LogInformation("Removing from cache {expression}", expression);
            var keys = _cachedKeys.Where(x => x.Key.StartsWith(expression)).Select(x => x.Key).ToList();
            foreach (var key in keys)
            {
                _cachedKeys.Remove(key);
                _memoryCache.Remove(key);
            }
        }

        public void PendingRemoveKeysStartsWith(string expression)
        {
            _logger.LogInformation("Add Pending remove from cache {expression}", expression);
            _removeCachePendings.Add(expression);
        }

        public void Set<TItem>(string key, TItem value, TimeSpan absoluteExpirationRelativeToNow)
        {
            _logger.LogInformation("Setting cache {key}", key);
            _memoryCache.Set(key, value, absoluteExpirationRelativeToNow);
            _cachedKeys.TryAdd(key, "");
        }

        public void RemoveAllPendingKeys()
        {
            _logger.LogInformation("Starting remove cache pendings");
            foreach (var pending in _removeCachePendings)
            {
                RemoveKeysStartsWith(pending);
            }
        }
    }
}
