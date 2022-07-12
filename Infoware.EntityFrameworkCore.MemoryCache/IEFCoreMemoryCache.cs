using Microsoft.EntityFrameworkCore;

namespace Infoware.EntityFrameworkCore.MemoryCache
{
    public interface IEFCoreMemoryCache
    {
        void RemoveKeysStartsWith(string expression);
        void Set<TItem>(string key, TItem value, TimeSpan absoluteExpirationRelativeToNow);
        bool TryGetValue<TItem>(string key, out TItem value);
        void RemoveAllPendingKeys();
        void PendingRemoveKeysStartsWith(string expression);
    }
}