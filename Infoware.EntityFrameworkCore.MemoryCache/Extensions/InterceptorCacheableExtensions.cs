using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Text.Json;
using Infoware.EntityFrameworkCore.MemoryCache.Models;

namespace Infoware.EntityFrameworkCore.MemoryCache.Extensions
{
    public static class InterceptorCacheableExtensions
    {
        public static IQueryable<T> CacheableTagWith<T>(this IQueryable<T> source, 
            [NotParameterized] string cacheKey, 
            [NotParameterized] TimeSpan absoluteExpirationRelativeToNow)
        {
            CacheParameters parameters = new()
            {
                CacheKey = cacheKey,
                AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow,
            };

            return source.TagWith(JsonSerializer.Serialize(parameters));
        }

    }
}
