using Infoware.EntityFrameworkCore.MemoryCache;
using Infoware.UnitOfWork.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infoware.EntityFrameworkCore.MemoryCache.Extensions
{
    public static class CacheableExtensions
    {
        public static QueryCached<TSource> Cacheable<TSource>(this IQueryable<TSource> source, IEFCoreMemoryCache memoryCache,
            string cacheKey, TimeSpan absoluteExpirationRelativeToNow)
        {
            return new QueryCached<TSource>(source, memoryCache, cacheKey, absoluteExpirationRelativeToNow);
        }
    }
}
