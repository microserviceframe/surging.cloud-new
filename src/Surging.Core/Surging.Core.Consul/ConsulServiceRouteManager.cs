using Consul;
using Microsoft.Extensions.Logging;
using Surging.Core.Consul.Configurations;
using Surging.Core.Consul.Internal;
using Surging.Core.Consul.Utilitys;
using Surging.Core.Consul.WatcherProvider;
using Surging.Core.Consul.WatcherProvider.Implementation;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Routing.Implementation;
using Surging.Core.CPlatform.Runtime.Client;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Surging.Core.Consul
{
    /// <summary>
    /// consul服务路由管理器 
    /// </summary>
    public class ConsulServiceRouteManager : ServiceRouteManagerBase, IDisposable
    {
        private readonly ConfigInfo _configInfo;
        private readonly ISerializer<byte[]> _serializer;
        private readonly IServiceRouteFactory _serviceRouteFactory;
        private readonly ILogger<ConsulServiceRouteManager> _logger;
        private readonly ISerializer<string> _stringSerializer;
        private readonly IClientWatchManager _manager;
        private ServiceRoute[] _routes;
        private readonly IConsulClientProvider _consulClientProvider;
        private readonly IServiceHeartbeatManager _serviceHeartbeatManager;

        public ConsulServiceRouteManager(ConfigInfo configInfo, ISerializer<byte[]> serializer,
        ISerializer<string> stringSerializer, IClientWatchManager manager, IServiceRouteFactory serviceRouteFactory,
        ILogger<ConsulServiceRouteManager> logger,
        IServiceHeartbeatManager serviceHeartbeatManager,
        IConsulClientProvider consulClientProvider) : base(stringSerializer)
        {
            _configInfo = configInfo;
            _serializer = serializer;
            _stringSerializer = stringSerializer;
            _serviceRouteFactory = serviceRouteFactory;
            _logger = logger;
            _consulClientProvider = consulClientProvider;
            _manager = manager;
            _serviceHeartbeatManager = serviceHeartbeatManager;
            EnterRoutes().Wait();
           
        }

        /// <summary>
        /// 清空服务路由
        /// </summary>
        /// <returns></returns>
        public override async Task ClearAsync()
        {
            var clients = await _consulClientProvider.GetClients();
            foreach (var client in clients)
            {
                //根据前缀获取consul结果
                var queryResult = await client.KV.List(_configInfo.RoutePath);
                var response = queryResult.Response;
                if (response != null)
                {
                    //删除操作
                    foreach (var result in response)
                    {
                        await client.KV.DeleteCAS(result);
                    }
                }
            }
        }

        public void Dispose()
        {
            
        }

        /// <summary>
        /// 获取所有可用的服务路由信息。
        /// </summary>
        /// <returns>服务路由集合。</returns>
        public override async Task<IEnumerable<ServiceRoute>> GetRoutesAsync(bool needUpdateFromServiceCenter = false)
        {
            await EnterRoutes(needUpdateFromServiceCenter);
            return _routes;
        }

        public override async Task SetRoutesAsync(IEnumerable<ServiceRoute> routes)
        {
            var hostAddr = NetUtils.GetHostAddress();
            await RemoveExceptRoutesAsync(routes, hostAddr);
            var serviceRoutes = await GetRoutes(routes.Select(p => $"{ _configInfo.RoutePath}{p.ServiceDescriptor.Id}"));
            foreach (var route in routes)
            {
                var serviceRoute = serviceRoutes.FirstOrDefault(p => p.ServiceDescriptor.Id == route.ServiceDescriptor.Id);
                if (serviceRoute != null)
                {
                    var addresses = serviceRoute.Address.Concat(route.Address).Distinct();
                    route.Address = addresses.ToList();
                }
            }

            await base.SetRoutesAsync(routes);           
                        
        }

        public override async Task<ServiceRoute> GetRouteByPathAsync(string path, string httpMethod)
        {
            var route = GetRouteByPathFormRoutes(path, httpMethod);
            if (route == null) 
            {
                await EnterRoutes(true);
                return GetRouteByPathFormRoutes(path, httpMethod);
            }
            return route;
        }

        private ServiceRoute GetRouteByPathFormRoutes(string path, string httpMethod)
        {
            if (_routes != null && _routes.Any(p => p.ServiceDescriptor.RoutePath == path && p.ServiceDescriptor.HttpMethod().Contains(httpMethod)))
            {
                return _routes.First(p => p.ServiceDescriptor.RoutePath == path && p.ServiceDescriptor.HttpMethod().Contains(httpMethod));
            }
            return GetRouteByRegexPathAsync(path, httpMethod);

        }

        private ServiceRoute GetRouteByRegexPathAsync(string path, string httpMthod)
        {
            var pattern = "/{.*?}";
            if (_routes != null)
            {
                var route = _routes.FirstOrDefault(i =>
                {
                    var routePath = Regex.Replace(i.ServiceDescriptor.RoutePath, pattern, "");
                    var newPath = path.Replace(routePath, "");
                    return (newPath.StartsWith("/") || newPath.Length == 0) && i.ServiceDescriptor.HttpMethod().Contains(httpMthod) && i.ServiceDescriptor.RoutePath.Split("/").Length == path.Split("/").Length;

                });


                if (route == null)
                {
                    if (_logger.IsEnabled(LogLevel.Warning))
                        _logger.LogWarning($"根据服务路由：{path}-{httpMthod}，找不到相关服务信息。");
                }
                return route;
            }
            return null;


        }
        public override async Task<ServiceRoute> GetRouteByServiceIdAsync(string serviceId, bool isCache = true)
        {
            var route = _routes.FirstOrDefault(p => p.ServiceDescriptor.Id == serviceId);
            if (route != null && route.Address.Any() && isCache)
            {
                return route;
            }
            var nodePath = GetServiceRouteNodePath(serviceId);

            var newRoute = await GetRoute(nodePath);
            //删除旧路由，并添加上新的路由。
            _routes =
                _routes
                    .Where(i => i.ServiceDescriptor.Id != route.ServiceDescriptor.Id)
                    .Concat(new[] { newRoute }).ToArray();
            return newRoute;

        }

 

        private string GetServiceRouteNodePath(string serviceId)
        {
            var rootPath = _configInfo.RoutePath;
            if (!rootPath.EndsWith("/"))
                rootPath += "/";
            var nodePath = $"{rootPath}{serviceId}";
            return nodePath;
        }

        private string GetServiceIdByNodePath(string nodePath)
        {
            return nodePath.Split("/").Last();
        }

        protected override async Task SetRoutesAsync(IEnumerable<ServiceRouteDescriptor> routes)
        {
            var clients = await _consulClientProvider.GetClients();
            foreach (var client in clients)
            {
                foreach (var serviceRoute in routes)
                {
                    await SetRouteAsync(serviceRoute);
                }
            }
        }

        protected override async Task SetRouteAsync(ServiceRouteDescriptor route)
        {
            var clients = await _consulClientProvider.GetClients();
            foreach (var client in clients)
            {
                var nodeData = _serializer.Serialize(route);
                if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                    _logger.LogDebug($"准备设置服务路由信息：{Encoding.UTF8.GetString(nodeData)}。");
                var keyValuePair = new KVPair($"{_configInfo.RoutePath}{route.ServiceDescriptor.Id}") { Value = nodeData };
                await client.KV.Put(keyValuePair);
            }
        }
        public override async Task RemveAddressAsync(IEnumerable<AddressModel> address)
        {
            await EnterRoutes(true);
            var routes = _routes.Where(route => route.Address.Any(p => address.Any(q => q.Equals(p))));
            foreach (var route in routes)
            {
                await RemveAddressAsync(address, route);
            }
        }

        public override async Task RemveAddressAsync(IEnumerable<AddressModel> address, string serviceId)
        {
            var serviceRoute = await GetRouteByServiceIdAsync(serviceId, false);
            serviceRoute.Address = serviceRoute.Address.Except(address).ToList();
            await base.SetRouteAsync(serviceRoute);

        }

        protected override async Task RemveAddressAsync(IEnumerable<AddressModel> address, ServiceRoute serviceRoute)
        {
            serviceRoute.Address = serviceRoute.Address.Except(address).ToList();
            await base.SetRouteAsync(serviceRoute);
        }

        #region 私有方法

        private async Task RemoveExceptRoutesAsync(IEnumerable<ServiceRoute> routes, AddressModel hostAddr)
        {
            var clients = await _consulClientProvider.GetClients();
            foreach (var client in clients)
            {
                if (_routes != null)
                {
                    var oldRouteIds = _routes.Select(i => i.ServiceDescriptor.Id).ToArray();
                    var newRouteIds = routes.Select(i => i.ServiceDescriptor.Id).ToArray();
                    var removeRouteIds = oldRouteIds.Except(newRouteIds).ToArray();
                    foreach (var removeRouteId in removeRouteIds)
                    {
                        var removeRoute = _routes.FirstOrDefault(p => p.ServiceDescriptor.Id == removeRouteId);
                        removeRoute.Address = removeRoute.Address.Where(p => !p.Equals(hostAddr)).ToList();
                        if (removeRoute != null && removeRoute.Address != null && removeRoute.Address.Any(p => p.Equals(hostAddr)))
                        {
                            var nodePath = $"{_configInfo.RoutePath}{removeRouteId}";
                            await SetRouteAsync(removeRoute);
                        }

                    }
                }
            }
        }

        private async Task<ServiceRoute> GetRoute(byte[] data)
        {
            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                _logger.LogDebug($"准备转换服务路由，配置内容：{Encoding.UTF8.GetString(data)}。");

            if (data == null || data.Length <= 0)
                return null;

            var descriptor = _serializer.Deserialize<byte[], ServiceRouteDescriptor>(data);
            return (await _serviceRouteFactory.CreateServiceRoutesAsync(new[] { descriptor })).First();
        }

        private async Task<ServiceRoute[]> GetRouteDatas(string[] routes)
        {
            List<ServiceRoute> serviceRoutes = new List<ServiceRoute>();
            foreach (var route in routes)
            {
                var serviceRoute = await GetRouteData(route);
                serviceRoutes.Add(serviceRoute);
            }
            return serviceRoutes.ToArray();
        }

        private async Task<ServiceRoute> GetRouteData(string data)
        {
            if (data == null)
                return null;

            var descriptor = _stringSerializer.Deserialize(data, typeof(ServiceRouteDescriptor)) as ServiceRouteDescriptor;
            return (await _serviceRouteFactory.CreateServiceRoutesAsync(new[] { descriptor })).First();
        }

        private async Task<ServiceRoute[]> GetRoutes(IEnumerable<string> childrens)
        {

            childrens = childrens.ToArray();
            var routes = new List<ServiceRoute>(childrens.Count());

            foreach (var children in childrens)
            {
                if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                    _logger.LogDebug($"准备从节点：{children}中获取路由信息。");

                var route = await GetRoute(children);
                if (route != null)
                    routes.Add(route);
            }

            return routes.ToArray();
        }

        private async Task<ServiceRoute> GetRoute(string path)
        {
            ServiceRoute result = null;
            var client = await GetConsulClient();
            if (client != null)
            {
                var watcher = new NodeMonitorWatcher(GetConsulClient, _manager, path,
                        async (oldData, newData) => await NodeChange(oldData, newData), null);

                var queryResult = await client.KV.Keys(path);
                if (queryResult.Response != null)
                {
                    var data = (await client.GetDataAsync(path));
                    if (data != null)
                    {
                        watcher.SetCurrentData(data);
                        result = await GetRoute(data);
                    }
                }
                return result;

            }

            return result;
        }

        private async ValueTask<ConsulClient> GetConsulClient()
        {
            var client = await _consulClientProvider.GetClient();
            return client;
        }

        private async Task EnterRoutes(bool needUpdateFromServiceCenter = false)
        {
            if (_routes != null && _routes.Length > 0 && !needUpdateFromServiceCenter)
                return;
            Action<string[]> action = null;
            var client = await GetConsulClient();
            if (client != null)
            {
                //判断是否启用子监视器
                if (_configInfo.EnableChildrenMonitor)
                {
                    //创建子监控类
                    var watcher = new ChildrenMonitorWatcher(GetConsulClient, _manager, _configInfo.RoutePath,
                    async (oldChildrens, newChildrens) => await ChildrenChange(oldChildrens, newChildrens),
                   (result) => ConvertPaths(result).Result);
                    //对委托绑定方法
                    action = currentData => watcher.SetCurrentData(currentData);
                }
                if (client.KV.Keys(_configInfo.RoutePath).Result.Response?.Count() > 0)
                {
                    var result = await client.GetChildrenAsync(_configInfo.RoutePath);
                    var keys = await client.KV.Keys(_configInfo.RoutePath);
                    var childrens = result;
                    //传参数到方法中
                    action?.Invoke(ConvertPaths(childrens).Result.Select(key => $"{_configInfo.RoutePath}{key}").ToArray());
                    //重新赋值到routes中
                    _routes = await GetRoutes(keys.Response);
                }
                else
                {
                    if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Warning))
                        _logger.LogWarning($"无法获取路由信息，因为节点：{_configInfo.RoutePath}，不存在。");
                    _routes = new ServiceRoute[0];
                }
            }

        }

        private static bool DataEquals(IReadOnlyList<byte> data1, IReadOnlyList<byte> data2)
        {
            if (data1.Count != data2.Count)
                return false;
            for (var i = 0; i < data1.Count; i++)
            {
                var b1 = data1[i];
                var b2 = data2[i];
                if (b1 != b2)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 转化路径集合
        /// </summary>
        /// <param name="datas">信息数据集合</param>
        /// <returns>返回路径集合</returns>
        private async Task<string[]> ConvertPaths(string[] datas)
        {
            List<string> paths = new List<string>();
            foreach (var data in datas)
            {
                var result = await GetRouteData(data);
                var serviceId = result?.ServiceDescriptor.Id;
                if (!string.IsNullOrEmpty(serviceId))
                    paths.Add(serviceId);
            }
            return paths.ToArray();
        }

        private async Task NodeChange(byte[] oldData, byte[] newData)
        {
            if (DataEquals(oldData, newData))
                return;

            var newRoute = await GetRoute(newData);
            if (_routes != null && _routes.Any() && newRoute != null)
            {
                //得到旧的路由。
                var oldRoute = _routes.FirstOrDefault(i => i.ServiceDescriptor.Id == newRoute.ServiceDescriptor.Id);
                if (newRoute.Address.Any())
                {
                    lock (_routes)
                    {
                        //删除旧路由，并添加上新的路由。
                        _routes =
                            _routes
                                .Where(i => i.ServiceDescriptor.Id != newRoute.ServiceDescriptor.Id)
                                .Concat(new[] { newRoute }).ToArray();
                    }

                    //触发路由变更事件。
                    OnChanged(new ServiceRouteChangedEventArgs(newRoute, oldRoute));

                }
                else
                {
                    lock (_routes)
                    {
                        //删除旧路由，并添加上新的路由。
                        _routes = _routes.Where(i => i.ServiceDescriptor.Id != newRoute.ServiceDescriptor.Id).ToArray();
                    }
                    OnRemoved(new ServiceRouteEventArgs(newRoute));

                }

            }

        }

        /// <summary>
        /// 数据更新
        /// </summary>
        /// <param name="oldChildrens">旧的节点信息</param>
        /// <param name="newChildrens">最新的节点信息</param>
        /// <returns></returns>
        private async Task ChildrenChange(string[] oldChildrens, string[] newChildrens)
        {
            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                _logger.LogDebug($"最新的节点信息：{string.Join(",", newChildrens)}");

            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                _logger.LogDebug($"旧的节点信息：{string.Join(",", oldChildrens)}");

            //计算出已被删除的节点。
            var deletedChildrens = oldChildrens.Except(newChildrens).ToArray();
            //计算出新增的节点。
            var createdChildrens = newChildrens.Except(oldChildrens).ToArray();

            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                _logger.LogDebug($"需要被删除的路由节点：{string.Join(",", deletedChildrens)}");
            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                _logger.LogDebug($"需要被添加的路由节点：{string.Join(",", createdChildrens)}");

            //获取新增的路由信息。
            var newRoutes = (await GetRoutes(createdChildrens)).ToArray();

            var routes = _routes.ToArray();
            lock (_routes)
            {
                #region 节点变更操作
                _routes = _routes
                    //删除无效的节点路由。
                    .Where(i => !deletedChildrens.Contains($"{_configInfo.RoutePath}{i.ServiceDescriptor.Id}"))
                    //连接上新的路由。
                    .Concat(newRoutes)
                    .ToArray();
                #endregion
            }
            //需要删除的路由集合。
            var deletedRoutes = routes.Where(i => deletedChildrens.Contains($"{_configInfo.RoutePath}{i.ServiceDescriptor.Id}")).ToArray();
            //触发删除事件。
            OnRemoved(deletedRoutes.Select(route => new ServiceRouteEventArgs(route)).ToArray());

            //触发路由被创建事件。
            OnCreated(newRoutes.Select(route => new ServiceRouteEventArgs(route)).ToArray());

            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Information))
                _logger.LogInformation("路由数据更新成功。");
        }
        #endregion
    }
}