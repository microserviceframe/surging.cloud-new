using Surging.Cloud.CPlatform;
using Surging.Cloud.CPlatform.Convertibles;
using Surging.Cloud.CPlatform.Routing;
using Surging.Cloud.CPlatform.Runtime.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Cloud.ProxyGenerator.Implementation
{
   public class RemoteServiceProxy: ServiceProxyBase
    {
        public RemoteServiceProxy(string serviceKey, CPlatformContainer serviceProvider)
           :this(serviceProvider.GetInstances<IRemoteInvokeService>(),
        serviceProvider.GetInstances<ITypeConvertibleService>(),serviceKey,serviceProvider,
        serviceProvider.GetInstances<IServiceRouteProvider>())
        {
            
        }

        public RemoteServiceProxy(IRemoteInvokeService remoteInvokeService,
            ITypeConvertibleService typeConvertibleService, String serviceKey,
            CPlatformContainer serviceProvider, IServiceRouteProvider serviceRouteProvider
            ):base(remoteInvokeService, typeConvertibleService, serviceKey, serviceProvider)
        {

        }

       public new async Task<T> Invoke<T>(IDictionary<string, object> parameters, string serviceId)
       {
           return await base.Invoke<T>(parameters, serviceId);
       }

    }
}
