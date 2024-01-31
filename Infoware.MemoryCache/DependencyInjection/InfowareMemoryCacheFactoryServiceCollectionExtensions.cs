using Infoware.MemoryCache;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infoware.EntityFrameworkCore.MemoryCache.DependencyInjection
{
    public static class InfowareMemoryCacheFactoryServiceCollectionExtensions
    {
        public static IServiceCollection AddInfowareMemoryCache(this IServiceCollection services, Action<InfowareMemoryCacheOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            services.AddOptions<InfowareMemoryCacheOptions>()
                .Configure(setupAction);
            return AddInfowareMemoryCache(services);
        }

        public static IServiceCollection AddInfowareMemoryCache(this IServiceCollection services, IConfiguration configuration) 
        {
            services.AddOptions<InfowareMemoryCacheOptions>()
                .Bind(configuration.GetSection(InfowareMemoryCacheOptions.ConfigurationSectionName));
            return AddInfowareMemoryCache(services);
        }

        public static IServiceCollection AddInfowareMemoryCache(this IServiceCollection services) 
        {
            services.AddMemoryCache();
            services.AddSingleton<ICache, InfowareMemoryCache>();
            return services;
        }
    }
}
