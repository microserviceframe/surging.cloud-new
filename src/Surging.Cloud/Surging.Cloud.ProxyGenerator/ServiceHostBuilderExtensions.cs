using Autofac;
using Surging.Cloud.CPlatform.Engines;
using Surging.Cloud.ServiceHosting.Internal;

namespace Surging.Cloud.ProxyGenerator
{
    public static class ServiceHostBuilderExtensions
    {
        public static IServiceHostBuilder UseProxy(this IServiceHostBuilder hostBuilder)
        {
            return hostBuilder.MapServices(mapper =>
            {
                mapper.Resolve<IServiceEngineLifetime>().ServiceEngineStarted.Register(() =>
                 {
                     mapper.Resolve<IServiceProxyFactory>();
                 }); 
            });
        }
    }
}
