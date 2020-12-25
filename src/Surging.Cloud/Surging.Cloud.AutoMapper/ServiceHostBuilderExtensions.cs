using Autofac;
using Surging.Cloud.ServiceHosting.Internal;

namespace Surging.Cloud.AutoMapper
{
    public static class ServiceHostBuilderExtensions
    {
        public static IServiceHostBuilder UseAutoMapper(this IServiceHostBuilder hostBuilder)
        {
            return hostBuilder.MapServices(mapper =>
            {
                var autoMapperBootstrap = mapper.Resolve<IAutoMapperBootstrap>();
                autoMapperBootstrap.Initialize();
            });
        }
    }
}
