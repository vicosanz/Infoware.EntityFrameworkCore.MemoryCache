namespace Infoware.MemoryCache
{
    public interface ICache
    {
        bool TryGetValue<TItem>(string key, out TItem value);
        Task<TItem?> GetAsync<TItem>(string key, CancellationToken cancellationToken);
        Task<TItem?> GetOrCreateAsync<TItem>(string key, Func<TItem> factory, TimeSpan absoluteExpirationRelativeToNow, CancellationToken cancellationToken = default);
        Task<TItem?> GetOrCreateAsync<TItem>(string key, Func<CancellationToken, Task<TItem>> factory, TimeSpan absoluteExpirationRelativeToNow, CancellationToken cancellationToken = default);
        void Set<TItem>(string key, TItem value, TimeSpan absoluteExpirationRelativeToNow);
        void RemoveKey(string? key);
        Task RemoveKeyAsync(string? key);
        void RemoveKeysStartsWith(string expression);
        Task RemoveKeysStartsWithAsync(string expression);
        void AddPendingRemoveKeysStartsWith(string expression);
        void RemoveAllPendingKeys();
        Task RemoveAllPendingKeysAsync();
        Task IfExistsDoAsync<TItem>(string key, Func<TItem, Task> function, CancellationToken cancellationToken = default);
        Task IfExistsDoAsync<TItem>(string key, Action<TItem> function, CancellationToken cancellationToken = default);
        Task SetAsync<TItem>(string key, TItem result, TimeSpan absoluteExpirationRelativeToNow, CancellationToken cancellationToken = default);
    }
}