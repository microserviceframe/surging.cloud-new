using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Transport.Implementation;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Routing.Implementation
{
    public class DefaultServiceRouteProvider : IServiceRouteProvider
    {
        private readonly ConcurrentDictionary<string, ServiceRoute> _concurrent = new ConcurrentDictionary<string, ServiceRoute>();
        private readonly ConcurrentDictionary<Tuple<string, string>, ServiceRoute> _serviceRoutes = new ConcurrentDictionary<Tuple<string, string>, ServiceRoute>();
        private readonly IServiceEntryManager _serviceEntryManager;
        private readonly ILogger<DefaultServiceRouteProvider> _logger;
        private readonly IServiceRouteManager _serviceRouteManager;
        private readonly IServiceTokenGenerator _serviceTokenGenerator;
        public DefaultServiceRouteProvider(IServiceRouteManager serviceRouteManager, ILogger<DefaultServiceRouteProvider> logger,
            IServiceEntryManager serviceEntryManager, IServiceTokenGenerator serviceTokenGenerator)
        {
            _serviceRouteManager = serviceRouteManager;
            serviceRouteManager.Changed += ServiceRouteManager_Removed;
            serviceRouteManager.Removed += ServiceRouteManager_Removed;
            serviceRouteManager.Created += ServiceRouteManager_Add;
            _serviceEntryManager = serviceEntryManager;
            _serviceTokenGenerator = serviceTokenGenerator;
            _logger = logger;
        }

        public async Task<ServiceRoute> Locate(string serviceId, bool fromCache = true)
        {
            if (fromCache && _concurrent.TryGetValue(serviceId, out ServiceRoute route) && route.Address.Any())
            {
                return route;
            }

            route = await _serviceRouteManager.GetRouteByServiceIdAsync(serviceId, fromCache);
            if (route == null)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning($"根据服务id：{serviceId}，找不到相关服务信息。");
            }
            if (route != null)
            {
                _concurrent.AddOrUpdate(serviceId, route, (k, v) => route);
                foreach (var httpMethod in route.ServiceDescriptor.HttpMethod())
                {
                    _serviceRoutes.AddOrUpdate(new Tuple<string, string>(route.ServiceDescriptor.RoutePath, httpMethod), route, (k, v) => route);
                }
            }

            return route;
        }


        public async Task<ServiceRoute> GetRouteByPath(string path, string httpMethod)
        {
            _serviceRoutes.TryGetValue(new Tuple<string, string>(path, httpMethod), out ServiceRoute route);
            if (route == null)
            {
                return await GetRouteByPathAsync(path, httpMethod);
            }
            return route;
        }

        public async Task<ServiceRoute> GetRouteByPathOrRegexPath(string path, string httpMethod)
        {
            var route = await GetRouteByPath(path, httpMethod);
            return route;
        }


        public async Task<ServiceRoute> SearchRoute(string path, string httpMethod)
        {
            return await SearchRouteAsync(path, httpMethod);
        }

        public async Task RegisterRoutes(decimal processorTime)
        {
            var addess = NetUtils.GetHostAddress();
            addess.ProcessorTime = processorTime;
            RpcContext.GetContext().SetAttachment("Host", addess);
            var addressDescriptors = _serviceEntryManager.GetEntries().Select(i =>
            {
                i.Descriptor.Token = _serviceTokenGenerator.GetToken();
                return new ServiceRoute
                {
                    Address = new[] { addess },
                    ServiceDescriptor = i.Descriptor
                };
            }).ToList();
            await _serviceRouteManager.SetRoutesAsync(addressDescriptors);
        }

        public async Task RemoveHostAddress(string serviceId)
        {
            var hostAddr = NetUtils.GetHostAddress();
            var serviceRoute = await Locate(serviceId);
            _concurrent.TryRemove(serviceId, out ServiceRoute serviceRoute1);
            foreach (var httpMethod in serviceRoute.ServiceDescriptor.HttpMethod())
            {
                _serviceRoutes.TryRemove(new Tuple<string, string>(serviceRoute.ServiceDescriptor.RoutePath, httpMethod), out ServiceRoute serviceRoute2);

            }
            await _serviceRouteManager.RemveAddressAsync(new List<AddressModel>() { hostAddr }, serviceId);
        }

        #region 私有方法
        private static string GetCacheKey(ServiceDescriptor descriptor)
        {
            return descriptor.Id;
        }

        private void ServiceRouteManager_Removed(object sender, ServiceRouteEventArgs e)
        {
            var key = GetCacheKey(e.Route.ServiceDescriptor);
            _concurrent.TryRemove(key, out ServiceRoute route1);
            var httpMethods = e.Route.ServiceDescriptor.HttpMethod();
            foreach (var httpMethod in httpMethods)
            {
                _serviceRoutes.TryRemove(new Tuple<string, string>(e.Route.ServiceDescriptor.RoutePath, httpMethod), out ServiceRoute route2);
            }

        }

        private void ServiceRouteManager_Add(object sender, ServiceRouteEventArgs e)
        {
            var key = GetCacheKey(e.Route.ServiceDescriptor);
            _concurrent.AddOrUpdate(key, e.Route, (k, v) => e.Route);
            var httpMethods = e.Route.ServiceDescriptor.HttpMethod();
            foreach (var httpMethod in httpMethods)
            {
                _serviceRoutes.AddOrUpdate(new Tuple<string, string>(e.Route.ServiceDescriptor.RoutePath, httpMethod), e.Route, (k, v) => e.Route);
            }

        }

        private async Task<ServiceRoute> SearchRouteAsync(string path, string httpMethod)
        {
            var route = await _serviceRouteManager.GetRouteByPathAsync(path, httpMethod);
            if (route == null)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning($"根据服务路由路径：{path}- {httpMethod}，找不到相关服务信息。");
                return null;
            }
            else
            {
                _serviceRoutes.GetOrAdd(new Tuple<string, string>(path, httpMethod), route);
            }
            return route;
        }

        private async Task<ServiceRoute> GetRouteByPathAsync(string path, string httpMethod)
        {
            var route = await _serviceRouteManager.GetRouteByPathAsync(path, httpMethod);
            if (route == null)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning($"根据服务路由路径：{path}-{httpMethod}，找不到相关服务信息。");
                return null;
            }

            else
            {
                _serviceRoutes.GetOrAdd(new Tuple<string, string>(path, httpMethod), route);
            }
            return route;
        }

        private ServiceRoute GetRouteByRegexPath(IEnumerable<ServiceRoute> routes, string path, string httpMethod)
        {
            var pattern = "/{.*?}";

            var route = routes.FirstOrDefault(i =>
            {
                var routePath = Regex.Replace(i.ServiceDescriptor.RoutePath, pattern, "");
                var newPath = path.Replace(routePath, "");
                return ((newPath.StartsWith("/") || newPath.Length == 0) && i.ServiceDescriptor.HttpMethod().Contains(httpMethod) && i.ServiceDescriptor.RoutePath.Split("/").Length == path.Split("/").Length && !i.ServiceDescriptor.GetMetadata<bool>("IsOverload"))
                ;
            });


            if (route == null)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning($"根据服务路由路径：{path} - {httpMethod}，找不到相关服务信息。");
            }
            else
            {
                if (Regex.IsMatch(route.ServiceDescriptor.RoutePath, pattern))
                {
                    _serviceRoutes.GetOrAdd(new Tuple<string, string>(path, httpMethod), route);
                }
            }

            return route;
        }

        public async Task RegisterRoutes(IEnumerable<ServiceEntry> serviceEntries)
        {
            var addess = NetUtils.GetHostAddress();
            addess.ProcessorTime = Math.Round(Convert.ToDecimal(Process.GetCurrentProcess().TotalProcessorTime.TotalSeconds), 2, MidpointRounding.AwayFromZero);
            RpcContext.GetContext().SetAttachment("Host", addess);
            var addressDescriptors = serviceEntries.Select(i =>
            {
                i.Descriptor.Token = _serviceTokenGenerator.GetToken();
                return new ServiceRoute
                {
                    Address = new[] { addess },
                    ServiceDescriptor = i.Descriptor
                };
            }).ToList();
            await _serviceRouteManager.SetRoutesAsync(addressDescriptors);
        }

        public void UpdateServiceRouteCache(ServiceRoute serviceRoute)
        {
            var key = GetCacheKey(serviceRoute.ServiceDescriptor);
            _concurrent.AddOrUpdate(key, serviceRoute, (k, v) => serviceRoute);

            var httpMethods = serviceRoute.ServiceDescriptor.HttpMethod();
            foreach (var httpMethod in httpMethods)
            {
                _serviceRoutes.AddOrUpdate(new Tuple<string, string>(serviceRoute.ServiceDescriptor.RoutePath, httpMethod), serviceRoute, (k, v) => serviceRoute);
            }
        }

        #endregion
    }
}