using Autofac;
using Surging.Cloud.ApiGateWay.Aggregation;
using Surging.Cloud.ApiGateWay.OAuth;
using Surging.Cloud.ApiGateWay.ServiceDiscovery;
using Surging.Cloud.ApiGateWay.ServiceDiscovery.Implementation;
using Surging.Cloud.CPlatform;
using Surging.Cloud.CPlatform.Routing;
using Surging.Cloud.CPlatform.Runtime.Client.HealthChecks;
using Surging.Cloud.CPlatform.Runtime.Client.HealthChecks.Implementation;
using Surging.Cloud.ProxyGenerator;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.ApiGateWay
{
   public static  class ContainerBuilderExtensions
    {
        /// <summary>
        /// 添加网关中间件
        /// </summary>
        /// <param name="builder">服务构建者</param>
        /// <param name="config"></param>
        /// <returns>服务构建者</returns>
        public static IServiceBuilder AddApiGateWay(this IServiceBuilder builder, ConfigInfo config=null)
        {
            var services = builder.Services;
            services.RegisterType<FaultTolerantProvider>().As<IFaultTolerantProvider>().SingleInstance();
            services.RegisterType<DefaultHealthCheckService>().As<IHealthCheckService>().SingleInstance();
            services.RegisterType<ServiceDiscoveryProvider>().As<IServiceDiscoveryProvider>().SingleInstance();
            services.RegisterType<ServiceRegisterProvider>().As<IServiceRegisterProvider>().SingleInstance();
            services.RegisterType<ServiceSubscribeProvider>().As<IServiceSubscribeProvider>().SingleInstance();
            services.RegisterType<ServiceCacheProvider>().As<IServiceCacheProvider>().SingleInstance();
            services.RegisterType<ServicePartProvider>().As<IServicePartProvider>().SingleInstance();
            if (config != null)
            {
                AppConfig.DefaultExpired = config.DefaultExpired;
                AppConfig.AuthorizationRoutePath = config.AuthorizationRoutePath;
                AppConfig.AuthorizationServiceKey = config.AuthorizationServiceKey;
            }
            builder.Services.Register(provider =>
            {
                var serviceProxyProvider = provider.Resolve<IServiceProxyProvider>();
                var serviceRouteProvider = provider.Resolve<IServiceRouteProvider>();
                var serviceProvider = provider.Resolve<CPlatformContainer>();
                return new AuthorizationServerProvider(serviceProxyProvider, serviceRouteProvider, serviceProvider);
            }).As<IAuthorizationServerProvider>().SingleInstance();
            return builder;
        }
    }
}
