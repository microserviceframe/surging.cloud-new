using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Surging.Cloud.CPlatform.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Surging.Cloud.CPlatform.Engines;
using Surging.Cloud.CPlatform.Exceptions;
using Surging.Cloud.CPlatform.Utilities;

namespace Surging.Cloud.CPlatform
{
    public static class ServiceHostBuilderExtensions
    {
        public static IHostBuilder RegisterMicroServices(this IHostBuilder hostBuilder)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", AppConfig.ServerOptions.Environment.ToString());
            return hostBuilder
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>(containerBuilder =>
                {
                    containerBuilder.GetServiceBuilder()
                                .AddServiceRuntime()
                                .AddRelateServiceRuntime();
                        
                })
                .ConfigureLogging(logging =>
                {
                    if (AppConfig.Configuration.GetSection("Logging").Exists())
                    {
                        logging.AddConfiguration(AppConfig.Configuration.GetSection("Logging"));
                    }
                })
                ;
        }

        public static IHostBuilder UseEngine<T>(this IHostBuilder hostBuilder) where T: IServiceEngine
        {
            return hostBuilder.UseEngine(typeof(T));
        }
        
        public static IHostBuilder UseEngine(this IHostBuilder hostBuilder, Type type)
        {
            if (!typeof(IServiceEngine).IsAssignableFrom(type))
            {
                throw new CPlatformException($"设置的服务引擎类型必须继承IServiceEngine接口");
            }

            return hostBuilder.ConfigureContainer<ContainerBuilder>(containerBuilder =>
            {
                containerBuilder.GetServiceBuilder().AddServiceEngine(type);
            });
        }

        public static IHostBuilder UseServer(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices((hostContext,services) =>
            {
                services.AddHostedService<ServerHostedService>();
            });
        }

        public static IHostBuilder UseServer(this IHostBuilder hostBuilder, Action<SurgingServerOptions> options)
        {
            var serverOptions = new SurgingServerOptions();
            options.Invoke(serverOptions);
            AppConfig.ServerOptions = serverOptions;
            return hostBuilder.UseServer();
        }

        public static IHostBuilder UseClient(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices((hostContext,services) =>
            {
                services.AddHostedService<ClientHostedService>();
            });
        }

    }
}
