using Autofac;
using Microsoft.Extensions.Logging;
using Surging.Cloud.Caching;
using Surging.Cloud.Caching.Configurations;
using Surging.Cloud.Codec.MessagePack;
using Surging.Cloud.Consul;
using Surging.Cloud.Consul.Configurations;
using Surging.Cloud.CPlatform;
using Surging.Cloud.CPlatform.Configurations;
using Surging.Cloud.CPlatform.Utilities;
using Surging.Cloud.DotNetty;
using Surging.Cloud.EventBusKafka.Configurations;
//using Surging.Cloud.EventBusKafka;
using Surging.Cloud.Log4net;
using Surging.Cloud.Nlog;
using Surging.Cloud.Protocol.Http;
using Surging.Cloud.ProxyGenerator;
using Surging.Cloud.ServiceHosting;
using Surging.Cloud.ServiceHosting.Internal.Implementation;
using Surging.Cloud.System.Intercept;
using Surging.Cloud.Zookeeper.Configurations;
using Surging.Services.ServiceHost;
using System;
//using Surging.Cloud.Zookeeper;
//using Surging.Cloud.Zookeeper.Configurations;
using System.Text;

namespace Surging.Services.Server
{
    public class Program
    {
        static void Main(string[] args)
        {
            //  Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var host = new ServiceHostBuilder()
                .RegisterServices(builder =>
                {
                    builder.AddMicroService(option =>
                    {
                        option.AddServiceRuntime()
                        .AddRelateService()
                        .AddConfigurationWatch()
                        //option.UseZooKeeperManager(new ConfigInfo("127.0.0.1:2181")); 
                        .AddServiceEngine(typeof(SurgingServiceEngine))
                        .AddClientIntercepted(typeof(CacheProviderInterceptor));
                        builder.Register(p => new CPlatformContainer(ServiceLocator.Current));
                    });
                })
                .ConfigureLogging(logger =>
                {
                    logger.AddConfiguration(
                        Core.CPlatform.AppConfig.GetSection("Logging"));
                })
                .UseServer(options => { })
                .UseConsoleLifetime()
                .Configure(build =>
                {

#if DEBUG
                    build.AddCacheFile("${cachePath}|/app/configs/cacheSettings.json", optional: false, reloadOnChange: true);
                    build.AddCPlatformFile("${surgingPath}|/app/configs/surgingSettings.json", optional: false, reloadOnChange: true);
                    build.AddEventBusFile("${eventBusPath}|/app/configs/eventBusSettings.json", optional: false, reloadOnChange: true);
                    build.AddConsulFile("${consulPath}|/app/configs/consul.json", optional: false, reloadOnChange: true);
                    build.AddZookeeperFile("${zookeeperPath}|/app/configs/zookeeper.json", optional: false, reloadOnChange: true);
#else
                    build.AddCacheFile("${cachePath}|configs/cacheSettings.json", optional: false, reloadOnChange: true);
                    build.AddCPlatformFile("${surgingPath}|configs/surgingSettings.json", optional: false, reloadOnChange: true);
                    build.AddEventBusFile("${eventBusPath}|configs/eventBusSettings.json", optional: false);
                    build.AddConsulFile("${consulPath}|configs/consul.json", optional: false, reloadOnChange: true);
                    build.AddZookeeperFile("${zookeeperPath}|configs/zookeeper.json", optional: false, reloadOnChange: true);
#endif
                })
                .UseStartup<Startup>()
                .Build();

            using (host.Run())
            {
                Console.WriteLine($"服务端启动成功，{DateTime.Now}。");
            }
        }
    }
}
