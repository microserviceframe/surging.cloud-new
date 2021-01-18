using System;
using Autofac;
using Microsoft.Extensions.Hosting;
using Surging.Cloud.CPlatform;
using Surging.Cloud.CPlatform.Exceptions;
using Surging.Cloud.ProxyGenerator;
using Surging.Cloud.ProxyGenerator.Interceptors;
using Surging.Cloud.System.Intercept;

namespace Surging.Cloud.System
{
    public static class ServiceHostBuilderExtensions
    {
        public static IHostBuilder UseCacheInterceptor(this IHostBuilder hostBuilder, Type type) 
        {
            if (!typeof(CacheInterceptor).IsAssignableFrom(type))
            {
                throw new CPlatformException($"设置的服务引擎类型必须继承IServiceEngine接口");
            }
  
            return hostBuilder.ConfigureContainer<ContainerBuilder>(containerBuilder =>
            {
                containerBuilder.GetServiceBuilder().AddClientIntercepted(type);
            });
        }
        
        public static IHostBuilder UseCacheInterceptor<T>(this IHostBuilder hostBuilder) where T: CacheInterceptor
        {
            return hostBuilder.UseCacheInterceptor(typeof(T));
        }
           
        public static IHostBuilder UseDefaultCacheInterceptor(this IHostBuilder hostBuilder)
        {
            return hostBuilder.UseCacheInterceptor(typeof(CacheProviderInterceptor));
        }

    }
}