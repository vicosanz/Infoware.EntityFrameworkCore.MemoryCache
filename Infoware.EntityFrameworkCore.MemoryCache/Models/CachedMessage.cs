namespace Infoware.EntityFrameworkCore.MemoryCache.Models
{
    public class CachedMessage
    {
        public int Id { get; set; }
        public string Message { get; set; } = null!;
    }
}