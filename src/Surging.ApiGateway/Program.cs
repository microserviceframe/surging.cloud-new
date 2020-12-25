using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Logging;
using Surging.Cloud.ApiGateWay;
using Surging.Cloud.Codec.MessagePack;
using Surging.Cloud.Consul;
using Surging.Cloud.Consul.Configurations;
using Surging.Cloud.CPlatform;
using Surging.Cloud.CPlatform.Utilities;
using Surging.Cloud.DotNetty;
using Surging.Cloud.ProxyGenerator;
using Surging.Cloud.ServiceHosting;
using System;
using System.IO;


namespace Surging.ApiGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseUrls("http://*:729")
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .Build();
            host.Run();
          
        }
    }
}
