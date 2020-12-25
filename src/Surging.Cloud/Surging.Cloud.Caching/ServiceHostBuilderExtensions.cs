using Microsoft.Extensions.Configuration;
using Surging.Cloud.Caching.Models;
using Surging.Cloud.ServiceHosting.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using System.Reflection;
using Surging.Cloud.Caching.Interfaces;
using Surging.Cloud.CPlatform.Cache;
using Surging.Cloud.Caching.Configurations;

namespace Surging.Cloud.Caching
{
    public static class ServiceHostBuilderExtensions
    {
        public static IServiceHostBuilder UseServiceCache(this IServiceHostBuilder hostBuilder)
        {
            return hostBuilder.MapServices(mapper =>
            {
                var serviceCacheProvider = mapper.Resolve<ICacheNodeProvider>();
                var addressDescriptors = serviceCacheProvider.GetServiceCaches().ToList();
                mapper.Resolve<IServiceCacheManager>().SetCachesAsync(addressDescriptors);
                mapper.Resolve<IConfigurationWatchProvider>();
            });
        }
        
    }
}
