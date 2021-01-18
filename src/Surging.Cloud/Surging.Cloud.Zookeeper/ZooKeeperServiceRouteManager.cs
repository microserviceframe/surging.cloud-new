﻿using Microsoft.Extensions.Logging;
using org.apache.zookeeper;
using Rabbit.Zookeeper;
using Surging.Cloud.CPlatform;
using Surging.Cloud.CPlatform.Address;
using Surging.Cloud.CPlatform.Routing;
using Surging.Cloud.CPlatform.Routing.Implementation;
using Surging.Cloud.CPlatform.Serialization;
using Surging.Cloud.CPlatform.Utilities;
using Surging.Cloud.Zookeeper.Configurations;
using Surging.Cloud.Zookeeper.Internal;
using Surging.Cloud.Zookeeper.WatcherProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static org.apache.zookeeper.KeeperException;
namespace Surging.Cloud.Zookeeper
{
     public class ZooKeeperServiceRouteManager : ServiceRouteManagerBase
    {
        private readonly ConfigInfo _configInfo;
        private readonly ISerializer<byte[]> _serializer;
        private readonly ISerializer<string> _stringSerializer;
        private readonly IServiceRouteFactory _serviceRouteFactory;
        private readonly ILogger<ZooKeeperServiceRouteManager> _logger;
        private ServiceRoute[] _routes;
        private readonly IZookeeperClientProvider _zookeeperClientProvider;
        private IDictionary<string, NodeMonitorWatcher> nodeWatchers = new Dictionary<string, NodeMonitorWatcher>();
        private ChildrenMonitorWatcher watcher = null;

        public ZooKeeperServiceRouteManager(ConfigInfo configInfo, ISerializer<byte[]> serializer,
            ISerializer<string> stringSerializer, IServiceRouteFactory serviceRouteFactory,
            ILogger<ZooKeeperServiceRouteManager> logger, IZookeeperClientProvider zookeeperClientProvider)
            : base(stringSerializer)
        {
            _configInfo = configInfo;
            _serializer = serializer;
            _stringSerializer = stringSerializer;
            _serviceRouteFactory = serviceRouteFactory;
            _logger = logger;
            _zookeeperClientProvider = zookeeperClientProvider;
            EnterRoutes().GetAwaiter().GetResult();            
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

        /// <summary>
        /// 清空所有的服务路由。
        /// </summary>
        /// <returns>一个任务。</returns>
        public override async Task ClearAsync()
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("准备清空所有路由配置。");
            var zooKeeperClients = await _zookeeperClientProvider.GetZooKeeperClients();
            foreach (var zooKeeperClient in zooKeeperClients)
            {
                var path = _configInfo.RoutePath;
                var childrens = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                var index = 0;
                while (childrens.Count() > 1)
                {
                    var nodePath = "/" + string.Join("/", childrens);

                    if (await zooKeeperClient.ExistsAsync(nodePath))
                    {
                        var children = (await zooKeeperClient.GetChildrenAsync(nodePath)).ToArray();
                        if (children != null && childrens.Any())
                        {
                            foreach (var child in children)
                            {
                                var childPath = $"{nodePath}/{child}";
                                if (_logger.IsEnabled(LogLevel.Debug))
                                    _logger.LogDebug($"准备删除：{childPath}。");
                                if (await zooKeeperClient.ExistsAsync(childPath))
                                {
                                    await zooKeeperClient.DeleteAsync(childPath);
                                }

                            }
                        }
                        if (_logger.IsEnabled(LogLevel.Debug))
                            _logger.LogDebug($"准备删除：{nodePath}。");
                        await zooKeeperClient.DeleteAsync(nodePath);
                    }
                    index++;
                    childrens = childrens.Take(childrens.Length - index).ToArray();
                }
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("路由配置清空完成。");

            }
        }

        /// <summary>
        /// 设置服务路由。
        /// </summary>
        /// <param name="routes">服务路由集合。</param>
        /// <returns>一个任务。</returns>
        protected override async Task SetRoutesAsync(IEnumerable<ServiceRouteDescriptor> routes)
        {
            var zooKeeperClients = await _zookeeperClientProvider.GetZooKeeperClients();
            foreach (var zooKeeperClient in zooKeeperClients)
            { 
                _logger.LogInformation($"准备向服务注册中心{zooKeeperClient.Options.ConnectionString}注册路由信息");
                await CreateSubdirectory(zooKeeperClient, _configInfo.RoutePath);

                var path = _configInfo.RoutePath;
                if (!path.EndsWith("/"))
                    path += "/";

                routes = routes.ToArray();
                var registerCount = 0;
                foreach (var serviceRoute in routes)
                {
                    if (await SetRouteAsync(serviceRoute, zooKeeperClient))
                    {
                        registerCount++;
                    }
                }
                _logger.LogInformation($"成功向服务注册中心注册{registerCount}个服务路由");
            }
          
        }

        private async Task<bool> SetRouteAsync(ServiceRouteDescriptor route, IZookeeperClient zooKeeperClient)
        {
            try
            {
                bool isSetRoute = false;
                _logger.LogDebug($"准备添加{route.ServiceDescriptor.Id}服务路由。");
                var zooKeeperClients = await _zookeeperClientProvider.GetZooKeeperClients();
                await CreateSubdirectory(zooKeeperClient, _configInfo.RoutePath);
                var path = _configInfo.RoutePath;
                if (!path.EndsWith("/"))
                    path += "/";

                var nodePath = $"{path}{route.ServiceDescriptor.Id}";
                var nodeData = _serializer.Serialize(route);
                _logger.LogDebug($"服务路由内容为：{Encoding.UTF8.GetString(nodeData)}。");
                if (!nodeWatchers.ContainsKey(nodePath))
                {   var watcher = nodeWatchers.GetOrAdd(nodePath, f => new NodeMonitorWatcher(path, async (oldData, newData) => await NodeChange(oldData, newData)));
                    await zooKeeperClient.SubscribeDataChange(nodePath, watcher.HandleNodeDataChange);
                }
                if (!await zooKeeperClient.ExistsAsync(nodePath))
                {
                    _logger.LogDebug($"节点：{nodePath}不存在将进行创建。");
                    await zooKeeperClient.CreateAsync(nodePath, nodeData, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
                }
                else
                {
                    var onlineData = (await zooKeeperClient.GetDataAsync(nodePath)).ToArray();
                    if (!DataEquals(nodeData, onlineData))
                    {
                        await zooKeeperClient.SetDataAsync(nodePath, nodeData);
                        _logger.LogDebug($"{nodePath}节点的缓存的服务路由与服务注册中心不一致,路由数据已被更新。");
                        isSetRoute = true;
                    }
                }

                return isSetRoute;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{route.ServiceDescriptor.Id}服务的路由注册失败,原因:{ex.Message}" );
                return false;

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
            await RemveAddressAsync(address, serviceRoute);

        }

        protected override async Task RemveAddressAsync(IEnumerable<AddressModel> address, ServiceRoute serviceRoute)
        {
            serviceRoute.Address = serviceRoute.Address.Except(address).ToList();
            var zookeeperClients = await _zookeeperClientProvider.GetZooKeeperClients();
            foreach (var zookeeperClient in zookeeperClients)
            {
                await SetRouteAsync(CreateServiceRouteDescriptor(serviceRoute), zookeeperClient);
            }

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

        private ServiceRoute GetRouteByRegexPathAsync(string path, string httpMethod)
        {
            var pattern = "/{.*?}";
            var route = _routes.FirstOrDefault(i =>
            {
                var routePath = Regex.Replace(i.ServiceDescriptor.RoutePath, pattern, "");
                var newPath = path.Replace(routePath, "");
                return (newPath.StartsWith("/") || newPath.Length == 0) && i.ServiceDescriptor.HttpMethod().Contains(httpMethod) && i.ServiceDescriptor.RoutePath.Split("/").Length == path.Split("/").Length && !i.ServiceDescriptor.GetMetadata<bool>("IsOverload")
                ;
            });

            if (route == null)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning($"根据服务路由：{path}-{httpMethod}，找不到相关服务信息。");
            }
            return route;
        }


        public override async Task<ServiceRoute> GetRouteByServiceIdAsync(string serviceId, bool isCache = true)
        {
            var route = _routes.FirstOrDefault(p => p.ServiceDescriptor.Id == serviceId);
            if (route != null && route.Address != null && route.Address.Any() && isCache)
            {
                return route;
            }

            var nodePath = GetServiceRouteNodePath(serviceId);

            var newRoute = await GetRoute(nodePath);
            if (newRoute != null && !newRoute.Equals(route))
            {
                //删除旧路由，并添加上新的路由。
                _routes =
                    _routes
                        .Where(i => i.ServiceDescriptor.Id != serviceId)
                        .Concat(new[] { newRoute }).ToArray();
                OnChanged(new ServiceRouteChangedEventArgs(newRoute, route));
            }

            return newRoute;

        }

        public override async Task SetRoutesAsync(IEnumerable<ServiceRoute> routes)
        {
            var hostAddr = NetUtils.GetHostAddress();
            var serviceRoutes = await GetRoutes(routes.Select(p => p.ServiceDescriptor.Id));
            await RemoveExceptRoutesAsync(routes, hostAddr);
            if (serviceRoutes.Any())
            {
                foreach (var route in routes)
                {
                    var serviceRoute = serviceRoutes.FirstOrDefault(p => p.ServiceDescriptor.Id == route.ServiceDescriptor.Id);
                    if (serviceRoute != null)
                    {
                        var newAddresses = new List<AddressModel>();
                        var addresses = serviceRoute.Address.Concat(route.Address);
                        foreach (var address in addresses)
                        {
                            if (!newAddresses.Any(a => a.Equals(address)))
                            {
                                newAddresses.Add(address);
                            }

                        }
                        route.Address = newAddresses;
                    }

                }
            }

            await base.SetRoutesAsync(routes);
        }

        private async Task RemoveExceptRoutesAsync(IEnumerable<ServiceRoute> routes, AddressModel hostAddr)
        {
            var path = _configInfo.RoutePath;
            if (!path.EndsWith("/"))
                path += "/";
            var zooKeepers = await _zookeeperClientProvider.GetZooKeeperClients();
            foreach (var zooKeeper in zooKeepers)
            {
                if (_routes != null)
                {
                    var oldRouteIds = _routes.Select(i => i.ServiceDescriptor.Id).ToArray();
                    var newRouteIds = routes.Select(i => i.ServiceDescriptor.Id).ToArray();
                    var removeRouteIds = oldRouteIds.Except(newRouteIds).ToArray();
                    foreach (var removeRouteId in removeRouteIds)
                    {
                        var removeRoute = _routes.FirstOrDefault(p => p.ServiceDescriptor.Id == removeRouteId);
                        if (removeRoute != null && removeRoute.Address != null && removeRoute.Address.Any(p => p.Equals(hostAddr)))
                        {
                            try
                            {
                                removeRoute.Address = removeRoute.Address.Where(p => !p.Equals(hostAddr)).ToList();
                                await SetRouteAsync(CreateServiceRouteDescriptor(removeRoute),zooKeeper);
                            }
                            catch (NoNodeException ex)
                            {
                                _logger.LogWarning(ex.Message);
                            }
                        }

                    }
                }

            }
        }

        private async Task CreateSubdirectory(IZookeeperClient zooKeeperClient, string path)
        {

            if (await zooKeeperClient.ExistsAsync(path))
                return;

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation($"节点{path}不存在，将进行创建。");

            var childrens = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var nodePath = "/";

            foreach (var children in childrens)
            {
                nodePath += children;
                if (!await zooKeeperClient.ExistsAsync(nodePath))
                {
                    await zooKeeperClient.CreateAsync(nodePath, null, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
                }
                nodePath += "/";
            }
        }

        private async Task<ServiceRoute> GetRoute(byte[] data)
        {
            _logger.LogDebug($"准备转换服务路由，配置内容：{Encoding.UTF8.GetString(data)}。");

            if (data == null || data.Length <= 0)
                return null;

            var descriptor = _serializer.Deserialize<byte[], ServiceRouteDescriptor>(data);
            return (await _serviceRouteFactory.CreateServiceRoutesAsync(new[] { descriptor })).First();
        }

        private async Task<ServiceRoute> GetRoute(string path)
        {
            ServiceRoute result = null;
            var zooKeeperClient = await _zookeeperClientProvider.GetZooKeeperClient();
            if (zooKeeperClient == null)
            {
                return null;
            }
            _logger.LogDebug($"准备从节点：{path}中获取路由信息。");
            if (await zooKeeperClient.ExistsAsync(path))
            {
                var data = (await zooKeeperClient.GetDataAsync(path)).ToArray();
                if (!nodeWatchers.ContainsKey(path))
                {
                    var watcher = nodeWatchers.GetOrAdd(path, f => new NodeMonitorWatcher(path, async (oldData, newData) => await NodeChange(oldData, newData)));
                    await zooKeeperClient.SubscribeDataChange(path, watcher.HandleNodeDataChange);
                }
                var route = await GetRoute(data);
                if (route != null)
                {
                    _logger.LogDebug($"从服务注册中心获取到服务{GetServiceIdByNodePath(path)}的路由数据为:{_stringSerializer.Serialize(result)}");
                }
                else
                {
                    _logger.LogWarning($"服务注册中心不存在{GetServiceIdByNodePath(path)}服务的路由数据");
                }
                return route;

            }
            else
            {
                _logger.LogWarning($"服务注册中心不存在{GetServiceIdByNodePath(path)}服务的路由数据");
            }

            return result;

        }

        private async Task<ServiceRoute[]> GetRoutes(IEnumerable<string> childrens)
        {
            var rootPath = _configInfo.RoutePath;
            if (!rootPath.EndsWith("/"))
                rootPath += "/";
            var routes = new List<ServiceRoute>();

            foreach (var children in childrens)
            {
               
                var nodePath = $"{rootPath}{children}";
                var route = await GetRoute(nodePath);
                if (route != null)
                {
                    routes.Add(route);                  
                }
            }

            return routes.ToArray();
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

        private async Task EnterRoutes(bool needUpdateFromServiceCenter = false)
        {
            if (_routes != null && _routes.Length > 0 && !needUpdateFromServiceCenter)
                return;
            var zooKeeperClient = await _zookeeperClientProvider.GetZooKeeperClient();
            if (zooKeeperClient == null)
            {
                _logger.LogWarning("没有找到可用的服务注册中心");
                return;
            }
            try
            {
                if (watcher == null) 
                {
                    watcher = new ChildrenMonitorWatcher(_configInfo.RoutePath,
                                               async (oldChildrens, newChildrens) => await ChildrenChange(oldChildrens, newChildrens));
                }
               
                await zooKeeperClient.SubscribeChildrenChange(_configInfo.RoutePath, watcher.HandleChildrenChange);

                if (await zooKeeperClient.ExistsAsync(_configInfo.RoutePath))
                {
                    var childrens = (await zooKeeperClient.GetChildrenAsync(_configInfo.RoutePath)).ToArray();
                    if (watcher != null)
                    {
                        watcher.SetCurrentData(childrens);
                    }
                    _routes = await GetRoutes(childrens);
                }
                else
                {
                    if (_logger.IsEnabled(LogLevel.Warning))
                        _logger.LogWarning($"无法获取路由信息，因为节点：{_configInfo.RoutePath}，不存在。");
                    _routes = new ServiceRoute[0];
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"从服务注册中心更新路由缓存错误:原因：{ex.Message}");
                return;
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

        public async Task NodeChange(byte[] oldData, byte[] newData)
        {
            if (DataEquals(oldData, newData))
                return;

            var newRoute = await GetRoute(newData);
            _logger.LogDebug($"接收到服务注册中心推送到的路由信息为:{_stringSerializer.Serialize(newRoute)}");
            if (_routes != null && _routes.Any() && newRoute != null)
            {  //得到旧的路由。
                var oldRoute = _routes.FirstOrDefault(i => i.ServiceDescriptor.Id == newRoute.ServiceDescriptor.Id);
                if (newRoute.Address != null && newRoute.Address.Any())
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
                        _routes = _routes.Where(i => i.ServiceDescriptor.Id != newRoute.ServiceDescriptor.Id).ToArray();
                    }
                    OnRemoved(new ServiceRouteEventArgs(newRoute));
                }

            }

        }

        public async Task ChildrenChange(string[] oldChildrens, string[] newChildrens)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"最新的节点信息：{string.Join(",", newChildrens)}");

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"旧的节点信息：{string.Join(",", oldChildrens)}");

            //计算出已被删除的节点。
            var deletedChildrens = oldChildrens.Except(newChildrens).ToArray();
            //计算出新增的节点。
            var createdChildrens = newChildrens.Except(oldChildrens).ToArray();

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"需要被删除的路由节点：{string.Join(",", deletedChildrens)}");
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"需要被添加的路由节点：{string.Join(",", createdChildrens)}");

            //获取新增的路由信息。
            var newRoutes = (await GetRoutes(createdChildrens)).ToArray();
            if (_routes != null && _routes.Any())
            {
                var routes = _routes.ToArray();
                lock (_routes)
                {
                    _routes = _routes
                        //删除无效的节点路由。
                        .Where(i => !deletedChildrens.Contains(i.ServiceDescriptor.Id))
                        //连接上新的路由。
                        .Concat(newRoutes)
                        .ToArray();
                }
                //需要删除的路由集合。
                var deletedRoutes = routes.Where(i => deletedChildrens.Contains(i.ServiceDescriptor.Id)).ToArray();
                //触发删除事件。
                OnRemoved(deletedRoutes.Select(route => new ServiceRouteEventArgs(route)).ToArray());
            }


            //触发路由被创建事件。
            OnCreated(newRoutes.Select(route => new ServiceRouteEventArgs(route)).ToArray());

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("路由数据更新成功。");
        }
        

    }
}