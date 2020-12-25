using Autofac;
using Microsoft.Extensions.Logging;
using Surging.Cloud.Caching;
using Surging.Cloud.Caching.Configurations;
using Surging.Cloud.Codec.MessagePack;
using Surging.Cloud.Consul;
using Surging.Cloud.Consul.Configurations;
using Surging.Cloud.CPlatform;
using Surging.Cloud.CPlatform.Configurations;
using Surging.Cloud.CPlatform.DependencyResolution;
using Surging.Cloud.CPlatform.Utilities;
using Surging.Cloud.DotNetty;
using Surging.Cloud.EventBusRabbitMQ;
using Surging.Cloud.EventBusRabbitMQ.Configurations;
using Surging.Cloud.Log4net;
using Surging.Cloud.Nlog;
using Surging.Cloud.ProxyGenerator;
using Surging.Cloud.ServiceHosting;
using Surging.Cloud.ServiceHosting.Internal.Implementation;
using Surging.Cloud.System.Intercept;
using Surging.IModuleServices.Common;
using System;
using System.Diagnostics;
//using Surging.Cloud.Zookeeper;
//using Surging.Cloud.Zookeeper.Configurations;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AppConfig = Surging.Cloud.CPlatform.AppConfig;

namespace Surging.Services.Client
{
    public class Program
    {
        private static int _endedConnenctionCount = 0;
        private static DateTime begintime;
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var host = new ServiceHostBuilder()
                .RegisterServices(builder =>
                {
                    builder.AddMicroService(option =>
                    {
                        option.AddClient()
                        .AddCache();
                        builder.Register(p => new CPlatformContainer(ServiceLocator.Current));
                    });
                })
                .ConfigureLogging(logger =>
                {
                    logger.AddConfiguration(
                        AppConfig.GetSection("Logging"));
                })
                .Configure(build => {
#if DEBUG
                    build.AddCacheFile("${cachePath}|/app/configs/cacheSettings.json", optional: false, reloadOnChange: true);
                    build.AddCPlatformFile("${surgingPath}|/app/configs/surgingSettings.json", optional: false, reloadOnChange: true);
                    // build.AddEventBusFile("${eventBusPath}|/app/configs/eventBusSettings.json", optional: false);


#else
                    build.AddCacheFile("${cachePath}|configs/cacheSettings.json", optional: false, reloadOnChange: true);                      
                    build.AddCPlatformFile("${surgingPath}|configs/surgingSettings.json", optional: false,reloadOnChange: true);                    
                    build.AddEventBusFile("configs/eventBusSettings.json", optional: false);
#endif
                })
                .UseClient()
                .UseProxy()
                .UseStartup<Startup>()
                .Build();

            using (host.Run())
            {
                // Startup.Test(ServiceLocator.GetService<IServiceProxyFactory>());
                //Startup.TestRabbitMq(ServiceLocator.GetService<IServiceProxyFactory>());
                // Startup.TestForRoutePath(ServiceLocator.GetService<IServiceProxyProvider>());
                /// test Parallel 
                //var connectionCount = 300000;
                //StartRequest(connectionCount);
                //Console.ReadLine();
            }
        }

        private static void StartRequest(int connectionCount)
        {
            // var service = ServiceLocator.GetService<IServiceProxyFactory>(); 
            var sw = new Stopwatch();
            sw.Start();
            var userProxy = ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy<IUserService>("User");
            ServiceResolver.Current.Register("User", userProxy);
            var service = ServiceLocator.GetService<IServiceProxyFactory>();
            userProxy = ServiceResolver.Current.GetService<IUserService>("User");
            sw.Stop();
            Console.WriteLine($"代理所花{sw.ElapsedMilliseconds}ms");
            ThreadPool.SetMinThreads(100, 100);
            Parallel.For(0, connectionCount / 6000, new ParallelOptions() { MaxDegreeOfParallelism = 50 }, async u =>
               {
                   for (var i = 0; i < 6000; i++)
                       await Test(userProxy, connectionCount);
               });
        }

        public static async Task Test(IUserService userProxy,int connectionCount)
        {
            var a =await userProxy.GetDictionary();
            IncreaseSuccessConnection(connectionCount);
        }
        
        private static void IncreaseSuccessConnection(int connectionCount)
        {
            Interlocked.Increment(ref _endedConnenctionCount);
            if (_endedConnenctionCount == 1)
                begintime = DateTime.Now;
            if (_endedConnenctionCount >= connectionCount)
                Console.WriteLine($"结束时间{(DateTime.Now - begintime).TotalMilliseconds}");
        }
    }
}
