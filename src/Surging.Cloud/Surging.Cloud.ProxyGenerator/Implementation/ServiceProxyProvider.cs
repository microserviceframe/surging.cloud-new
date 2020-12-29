using System;
using Surging.Cloud.CPlatform;
using Surging.Cloud.CPlatform.DependencyResolution;
using Surging.Cloud.CPlatform.Exceptions;
using Surging.Cloud.CPlatform.Routing;
using Surging.Cloud.CPlatform.Runtime;
using Surging.Cloud.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Surging.Cloud.ProxyGenerator.Implementation
{
    public class ServiceProxyProvider : IServiceProxyProvider
    {
        private readonly IServiceRouteProvider _serviceRouteProvider;
        private readonly CPlatformContainer _serviceProvider;
        public ServiceProxyProvider( IServiceRouteProvider serviceRouteProvider
            , CPlatformContainer serviceProvider)
        {
            _serviceRouteProvider = serviceRouteProvider;
            _serviceProvider = serviceProvider;
        }

        public async Task<T> Invoke<T>(IDictionary<string, object> parameters, string routePath, HttpMethod httpMethod)
        {
            var serviceRoute = await LocationServiceRoute(routePath, httpMethod.ToString());
            T result = default(T);
            if (parameters.ContainsKey("serviceKey"))
            {
                var serviceKey = parameters["serviceKey"].ToString();
                var proxy = ServiceResolver.Current.GetService<RemoteServiceProxy>(serviceKey);
                if (proxy == null)
                {
                    proxy = new RemoteServiceProxy(serviceKey.ToString(), _serviceProvider);
                    ServiceResolver.Current.Register(serviceKey.ToString(), proxy);
                }

                result = await proxy.Invoke<T>(parameters, serviceRoute.ServiceDescriptor.Id);
            }
            else
            {
                var proxy = ServiceResolver.Current.GetService<RemoteServiceProxy>();
                if (proxy == null)
                {
                    proxy = new RemoteServiceProxy(null, _serviceProvider);
                    ServiceResolver.Current.Register(null, proxy);
                }
                result = await proxy.Invoke<T>(parameters, serviceRoute.ServiceDescriptor.Id);
            }
            return result;
        }



        public async Task<T> Invoke<T>(IDictionary<string, object> parameters, string routePath, HttpMethod httpMethod, string serviceKey)
        {
            var serviceRoute = await LocationServiceRoute(routePath, httpMethod.ToString());
            T result = default(T);
            if (!string.IsNullOrEmpty(serviceKey))
            {
                var proxy = ServiceResolver.Current.GetService<RemoteServiceProxy>(serviceKey);
                if (proxy == null)
                {
                    proxy = new RemoteServiceProxy(serviceKey, _serviceProvider);
                    ServiceResolver.Current.Register(serviceKey, proxy);
                }
                
                result = await proxy.Invoke<T>(parameters, serviceRoute.ServiceDescriptor.Id);
            }
            else
            {
                var proxy = ServiceResolver.Current.GetService<RemoteServiceProxy>();
                if (proxy == null)
                {
                    proxy = new RemoteServiceProxy(null, _serviceProvider);
                    ServiceResolver.Current.Register(null, proxy);
                }
                result = await proxy.Invoke<T>(parameters, serviceRoute.ServiceDescriptor.Id);
            }
            return result;
        }

        

        private async Task<ServiceRoute> LocationServiceRoute(string routePath,string httpMethod)
        {
            var serviceRoute = await _serviceRouteProvider.GetRouteByPathOrRegexPath(routePath.ToLower(), httpMethod);
          
            if (serviceRoute == null)
            {
                throw new CPlatformException($"不存在api为{routePath}的路由信息");
            }

            return serviceRoute;
        }
    }
}
