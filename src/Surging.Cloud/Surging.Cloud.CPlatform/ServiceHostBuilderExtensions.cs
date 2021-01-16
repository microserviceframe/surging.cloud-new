using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Surging.Cloud.CPlatform.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
                    var builder = new ServiceBuilder(containerBuilder);
                        builder.AddServiceRuntime()
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
