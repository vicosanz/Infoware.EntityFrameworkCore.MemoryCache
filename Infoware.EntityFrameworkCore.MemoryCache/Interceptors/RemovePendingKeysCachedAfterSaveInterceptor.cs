using Infoware.EntityFrameworkCore.MemoryCache.Models;
using Infoware.MemoryCache;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Memory;
using System.Data;
using System.Data.Common;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Infoware.EntityFrameworkCore.MemoryCache.Interceptors
{
    public class RemovePendingKeysCachedAfterSaveInterceptor : SaveChangesInterceptor
    {
        private readonly ICache _eFCoreMemoryCache;

        public RemovePendingKeysCachedAfterSaveInterceptor(ICache eFCoreMemoryCache)
        {
            _eFCoreMemoryCache = eFCoreMemoryCache;
        }

        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            _eFCoreMemoryCache.RemoveAllPendingKeys();
            return base.SavedChanges(eventData, result);
        }

    }
}
