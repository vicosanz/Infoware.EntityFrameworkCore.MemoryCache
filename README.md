# Infoware.EntityFrameworkCore.MemoryCache
Cache in memory specific queries

[![NuGet Badge](https://buildstats.info/nuget/Infoware.EntityFrameworkCore.MemoryCache)](https://www.nuget.org/packages/Infoware.EntityFrameworkCore.MemoryCache/)

[![publish to nuget](https://github.com/vicosanz/Infoware.EntityFrameworkCore.MemoryCache/actions/workflows/main.yml/badge.svg)](https://github.com/vicosanz/Infoware.EntityFrameworkCore.MemoryCache/actions/workflows/main.yml)

Complement for Infoware.UnitOfWork

[![NuGet Badge](https://buildstats.info/nuget/Infoware.UnitOfWork)](https://www.nuget.org/packages/Infoware.UnitOfWork/)

Usage:
```csharp

    public class UserService
    {
        private readonly IEFCoreMemoryCache _memoryCache;
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository, IEFCoreMemoryCache memoryCache)
        {
            _userRepository = userRepository;
            _memoryCache = memoryCache;
        }

        public async Task<IPagedList<UserView>> GetAll(int page, int rowsPerPage)
        {
            string cacheKey = "user_all";
            var query = from user in _userRepository.GetAll()
                    select new UserView()
                    {
                        Id = user.Id,
                        Name = user.Name
                    };

            return await query.Cacheable(_memoryCache, cacheKey, TimeSpan.FromMinutes(15))
                .ToPagedListAsync(pageIndex: page, pageSize: rowsPerPage, cancellationToken: cancellationToken);

        }
    }
```

query.Cacheable interrupts the query and tries to check if there is a previous execution cached with cacheKey; otherwise, the query is executed and cached with the configured TimeSpan.

If you want to invalidate the cache, e.g. if your repository add a new User, you must to execute RemoveKeysStartsWith method
```csharp

    public class UserRepository<TContext> : Repository<TContext, User>
    {
        private readonly IEFCoreMemoryCache _memoryCache;

        public UserRepository(IUserRepository userRepository)
        {
            _memoryCache = memoryCache;
        }

        public override User Insert(User entity)
        {
            _memoryCache.RemoveKeysStartsWith("user_");
            return base.Insert(User);
        }
    }
```

If you want to ensure to invalidate cache when changes are saved execute RemoveAllPendingKeys in SaveChanges, and in each insert create a pending remove with PendingRemoveKeysStartsWith

```csharp

    public class UserRepository<TContext> : Repository<TContext, User>
    {
        private readonly IEFCoreMemoryCache _memoryCache;

        public UserRepository(IUserRepository userRepository)
        {
            _memoryCache = memoryCache;
        }

        public override User Insert(User entity)
        {
            _memoryCache.PendingRemoveKeysStartsWith("user_");
            return base.Insert(User);
        }
    }

    public class DocumentosContext : DbContext
    {
        ...


        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var result = await base.SaveChangesAsync(cancellationToken);
            _memoryCache.RemoveAllPendingKeys();
            return result;
        }

    }

```
