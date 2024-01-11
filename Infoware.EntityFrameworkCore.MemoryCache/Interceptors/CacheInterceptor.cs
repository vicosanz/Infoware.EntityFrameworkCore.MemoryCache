using Infoware.EntityFrameworkCore.MemoryCache.Models;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data;
using System.Data.Common;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Infoware.EntityFrameworkCore.MemoryCache.Interceptors
{
    public class CacheInterceptor : DbCommandInterceptor, IDisposable
    {
        private readonly IEFCoreCache _eFCoreMemoryCache;

        public CacheInterceptor(IEFCoreCache eFCoreMemoryCache)
        {
            _eFCoreMemoryCache = eFCoreMemoryCache;
        }

        private static readonly SemaphoreSlim _semaphore = new(1);
        private bool disposedValue;

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
            => throw new InvalidOperationException("Sync interception not implemented; use async queries.");

        public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            CacheParameters? cacheParameters = GetCacheParameters(command.CommandText);
            if (cacheParameters != null)
            {
                await _semaphore.WaitAsync(cancellationToken);
                await _eFCoreMemoryCache.IfExistsDoAsync<IList<Dictionary<string, object>>>(
                    cacheParameters.CacheKey,
                    (cacheEntry) =>
                    {
                        command.CommandText = $"-- Skipping DB call; using cache {cacheParameters.CacheKey}.";

                        result = InterceptionResult<DbDataReader>.SuppressWithResult(ToDataReader(cacheEntry)!);
                    },
                    cancellationToken
                );
                _semaphore.Release();
            }

            return result;
        }

        public override async ValueTask<DbDataReader> ReaderExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result,
            CancellationToken cancellationToken = default)
        {
            CacheParameters? cacheParameters = GetCacheParameters(command.CommandText);
            if (cacheParameters != null)
            {
                try
                {
                    var resultsList = await DataReaderToDictionary(result, cancellationToken);

                    await _semaphore.WaitAsync(cancellationToken);
                    if (resultsList.Any())
                    {
                        await _eFCoreMemoryCache.SetAsync(cacheParameters.CacheKey, resultsList, 
                            cacheParameters.AbsoluteExpirationRelativeToNow, cancellationToken);
                        return ToDataReader(resultsList);
                    }
                }
                finally
                {
                    await result.DisposeAsync();
                    _semaphore.Release();
                }
            }

            return result;
        }


        private static CacheParameters? GetCacheParameters(string commandText)
        {
            try
            {
                Regex regexCacheParameters = new("-- ({\"CacheKey\".*})");
                Match match = regexCacheParameters.Match(commandText);
                if (match.Success)
                {
                    return JsonSerializer.Deserialize<CacheParameters>(match.Groups[1].Value);
                }
            }
            catch { }
            return null;
        }

        private static DataTableReader ToDataReader(IList<Dictionary<string, object>>? resultsList)
        {
            var table = new DataTable();
            if (resultsList?.Any() ?? false)
            {
                foreach (var pair in resultsList.First())
                {
                    table.Columns.Add(pair.Key,
                        pair.Value is not null && pair.Value?.GetType() != typeof(DBNull)
                            ? pair.Value!.GetType()
                            : typeof(object));
                }

                foreach (var row in resultsList)
                {
                    table.Rows.Add(row.Values.ToArray());
                }
            }
            return table.CreateDataReader();
        }

        private static async Task<IList<Dictionary<string, object>>> DataReaderToDictionary(
            DbDataReader result, CancellationToken cancellationToken)
        {
            var resultsList = new List<Dictionary<string, object>>();
            if (result.HasRows)
            {
                while (await result.ReadAsync(cancellationToken))
                {
                    var row = new Dictionary<string, object>();
                    for (var i = 0; i < result.FieldCount; i++)
                    {
                        row.TryAdd(result.GetName(i), result.GetValue(i));
                    }

                    resultsList.Add(row);
                }

            }
            await result.CloseAsync();
            return resultsList;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _semaphore.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~CacheInterceptor()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
