using Autofac;
using Surging.Cloud.CPlatform;
using Surging.Cloud.CPlatform.Convertibles;
using Surging.Cloud.CPlatform.Engines;
using Surging.Cloud.CPlatform.Module;
using Surging.Cloud.CPlatform.Routing;
using Surging.Cloud.CPlatform.Runtime.Client;
using Surging.Cloud.ProxyGenerator.Implementation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Surging.Cloud.CPlatform.Runtime.Server;
using Surging.Cloud.ProxyGenerator.Diagnostics;
using Surging.Cloud.CPlatform.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Surging.Cloud.ProxyGenerator
{
   public class ServiceProxyModule: EnginePartModule
    {
        public override void Initialize(AppModuleContext context)
        {
            var serviceProvider = context.ServiceProvoider;
            serviceProvider.GetInstances<IServiceProxyFactory>();
            if (AppConfig.ServerOptions.ReloadOnChange)
            {
                new ServiceRouteWatch(serviceProvider,
                        () =>
                        {
                            var builder = new ContainerBuilder();
                            var result = serviceProvider.GetInstances<IServiceEngineBuilder>().ReBuild(builder);
                            if (result != null)
                            {
                                builder.Update(serviceProvider.Current.ComponentRegistry);
                                serviceProvider.GetInstances<IServiceEntryManager>().UpdateEntries(serviceProvider.GetInstances<IEnumerable<IServiceEntryProvider>>());
                                serviceProvider.GetInstances<IServiceRouteProvider>().RegisterRoutes(0);
                                serviceProvider.GetInstances<IServiceProxyFactory>();
                            }
                        });
            }
        }

        /// <summary>
        /// Inject dependent third-party components
        /// </summary>
        /// <param name="builder"></param>
        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            base.RegisterBuilder(builder);
            builder.RegisterType<RpcTransportDiagnosticProcessor>().As<ITracingDiagnosticProcessor>().SingleInstance();
        }
    }
}
