using Infoware.MemoryCache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Infoware.EntityFrameworkCore.MemoryCache.DependencyInjection
{
    public static class InfowareEFCoreMemoryCacheFactoryServiceCollectionExtensions
    {
        public static IServiceCollection AddInfowareEFCoreMemoryCache(this IServiceCollection services, Action<InfowareEFCoreMemoryCacheOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            services.AddOptions<InfowareEFCoreMemoryCacheOptions>()
                .Configure(setupAction);
            return AddInfowareEFCoreMemoryCache(services);
        }

        public static IServiceCollection AddInfowareEFCoreMemoryCache(this IServiceCollection services, IConfiguration configuration) 
        {
            services.AddOptions<InfowareEFCoreMemoryCacheOptions>()
                .Bind(configuration.GetSection(InfowareEFCoreMemoryCacheOptions.ConfigurationSectionName));
            return AddInfowareEFCoreMemoryCache(services);
        }

        private static IServiceCollection AddInfowareEFCoreMemoryCache(IServiceCollection services) 
        {
            services.AddInfowareMemoryCache();
            return services;
        }
    }
}
