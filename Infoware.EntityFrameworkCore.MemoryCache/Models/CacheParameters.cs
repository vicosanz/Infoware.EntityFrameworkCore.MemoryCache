
namespace Infoware.EntityFrameworkCore.MemoryCache.Models
{
    public class CacheParameters
    {
        public string CacheKey { get; set; } = null!;
        public TimeSpan AbsoluteExpirationRelativeToNow { get; set; }
    }
}