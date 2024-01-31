using Infoware.EntityFrameworkCore.MemoryCache.Models;
using Infoware.MemoryCache;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Infoware.EntityFrameworkCore.MemoryCache.Interceptors
{
    public class CacheInterceptor : DbCommandInterceptor, IDisposable
    {
        private readonly ICache _eFCoreMemoryCache;

        public CacheInterceptor(ICache eFCoreMemoryCache)
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
                await _eFCoreMemoryCache.IfExistsDoAsync<EFCachedData>(
                    cacheParameters.CacheKey,
                    (cacheEntry) =>
                    {
                        command.CommandText = $"-- Skipping DB call; using cache {cacheParameters.CacheKey}.";

                        result = ProcessCachedData(result, cacheEntry);
                    },
                    cancellationToken
                );
                _semaphore.Release();
            }

            return result;
        }

        private static T ProcessCachedData<T>(T result, EFCachedData cacheResult)
        {
            switch (result)
            {
                case InterceptionResult<DbDataReader>:
                    {
                        if (cacheResult.IsNull || cacheResult.TableRows == null)
                        {
                            using var rows = new EFTableRowsDataReader(new EFTableRows());
                            return (T)Convert.ChangeType(
                                InterceptionResult<DbDataReader>.SuppressWithResult(rows),
                                typeof(T),
                                CultureInfo.InvariantCulture);
                        }

                        using var dataRows = new EFTableRowsDataReader(cacheResult.TableRows);
                        return (T)Convert.ChangeType(
                            InterceptionResult<DbDataReader>.SuppressWithResult(dataRows),
                            typeof(T),
                            CultureInfo.InvariantCulture);
                    }

                case InterceptionResult<int>:
                    {
                        var cachedResult = cacheResult.IsNull ? default : cacheResult.NonQuery;

                        return (T)Convert.ChangeType(
                            InterceptionResult<int>.SuppressWithResult(cachedResult),
                            typeof(T),
                            CultureInfo.InvariantCulture);
                    }

                case InterceptionResult<object>:
                    {
                        var cachedResult = cacheResult.IsNull ? default : cacheResult.Scalar;

                        return (T)Convert.ChangeType(
                            InterceptionResult<object>
                                .SuppressWithResult(cachedResult ?? new object()),
                            typeof(T),
                            CultureInfo.InvariantCulture);
                    }

                default:
                    return result;
            }
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
                if (result is EFTableRowsDataReader)
                {
                    return result;
                }

                await _semaphore.WaitAsync(cancellationToken);
                var data = ProcessDataToCache(result);
                await _eFCoreMemoryCache.SetAsync(cacheParameters.CacheKey, data, 
                    cacheParameters.AbsoluteExpirationRelativeToNow, cancellationToken);
                _semaphore.Release();
                return new EFTableRowsDataReader(data.TableRows!);
            }

            return result;
        }

        private static EFCachedData ProcessDataToCache(DbDataReader dataReader)
        {
            using var dbReaderLoader = new EFDataReaderLoader(dataReader);
            var tableRows = dbReaderLoader.LoadAndClose();

            return new EFCachedData 
            { 
                TableRows = tableRows 
            };
        }

        private static CacheParameters? GetCacheParameters(string commandText)
        {
            try
            {
                Regex regexCacheParameters = new("-- ({\"CacheKey\".*})");
                Regex regexCacheModifier = new("-- \\[\\[(.*)\\]\\]");

                Match match = regexCacheParameters.Match(commandText);
                if (match.Success)
                {
                    CacheParameters? cacheParameters = JsonSerializer.Deserialize<CacheParameters>(match.Groups[1].Value);
                    if (cacheParameters != null)
                    {
                        Match modifierMatch = regexCacheModifier.Match(commandText);
                        if (modifierMatch.Success)
                        {
                            cacheParameters.CacheKey += modifierMatch.Groups[1].Value;
                        }
                    }
                    return cacheParameters;
                }
            }
            catch { }
            return null;
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
