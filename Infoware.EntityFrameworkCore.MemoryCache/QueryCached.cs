using Infoware.UnitOfWork.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infoware.EntityFrameworkCore.MemoryCache
{
    public class QueryCached<TSource>
    {
        private readonly IQueryable<TSource> _source;
        private readonly IEFCoreCache _memoryCache;
        private readonly string _cacheKey;
        private readonly TimeSpan _slidingExpiration;

        public QueryCached(IQueryable<TSource> source, IEFCoreCache memoryCache, string cacheKey,
            TimeSpan slidingExpiration)
        {
            _source = source;
            _memoryCache = memoryCache;
            _cacheKey = cacheKey;
            _slidingExpiration = slidingExpiration;
        }

        public async Task<TSource?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
        {
            if (!_memoryCache.TryGetValue(_cacheKey, out TSource? result))
            {
                result = await _source.FirstOrDefaultAsync(cancellationToken: cancellationToken);
                _memoryCache.Set(_cacheKey, result, _slidingExpiration);
            }
            return result;
        }

        public async Task<IPagedList<TSource>> ToPagedListAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
        {
            if (!_memoryCache.TryGetValue(_cacheKey, out IPagedList<TSource> result))
            {
                result = await _source.ToPagedListAsync(pageIndex, pageSize, cancellationToken: cancellationToken);
                _memoryCache.Set(_cacheKey, result, _slidingExpiration);
            }
            return result;
        }

        public async Task<IList<TSource>> ToListAsync(CancellationToken cancellationToken = default)
        {
            if (!_memoryCache.TryGetValue(_cacheKey, out IList<TSource> result))
            {
                result = await _source.ToListAsync(cancellationToken: cancellationToken);
                _memoryCache.Set(_cacheKey, result, _slidingExpiration);
            }
            return result;
        }

    }
}