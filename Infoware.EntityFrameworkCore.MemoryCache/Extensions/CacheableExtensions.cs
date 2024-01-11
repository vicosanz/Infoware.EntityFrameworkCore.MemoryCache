namespace Infoware.EntityFrameworkCore.MemoryCache.Extensions
{
    public static class CacheableExtensions
    {
        public static QueryCached<T> Cacheable<T>(this IQueryable<T> source, IEFCoreCache memoryCache,
            string cacheKey, TimeSpan absoluteExpirationRelativeToNow)
        {
            return new QueryCached<T>(source, memoryCache, cacheKey, absoluteExpirationRelativeToNow);
        }
    }
}
