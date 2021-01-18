﻿using Microsoft.Extensions.Logging;
using org.apache.zookeeper;
using Rabbit.Zookeeper;
using Surging.Cloud.CPlatform.Routing;
using Surging.Cloud.CPlatform.Routing.Implementation;
using Surging.Cloud.CPlatform.Runtime.Server;
using Surging.Cloud.CPlatform.Serialization;
using Surging.Cloud.CPlatform.Support;
using Surging.Cloud.CPlatform.Support.Implementation;
using Surging.Cloud.CPlatform.Utilities;
using Surging.Cloud.Zookeeper.Configurations;
using Surging.Cloud.Zookeeper.Internal;
using Surging.Cloud.Zookeeper.WatcherProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static org.apache.zookeeper.KeeperException;

namespace Surging.Cloud.Zookeeper
{
    public class ZookeeperServiceCommandManager : ServiceCommandManagerBase, IDisposable
    {
        private readonly ConfigInfo _configInfo;
        private readonly ISerializer<byte[]> _serializer;
        private readonly ILogger<ZookeeperServiceCommandManager> _logger;
        private ServiceCommandDescriptor[] _serviceCommands;
        private readonly IServiceRouteManager _serviceRouteManager;
        private readonly IZookeeperClientProvider _zookeeperClientProvider;
        private IDictionary<string, NodeMonitorWatcher> nodeWatchers = new Dictionary<string, NodeMonitorWatcher>();
        private ChildrenMonitorWatcher watcher = null;

        public ZookeeperServiceCommandManager(ConfigInfo configInfo, ISerializer<byte[]> serializer,
            ISerializer<string> stringSerializer, IServiceRouteManager serviceRouteManager, IServiceEntryManager serviceEntryManager,
            ILogger<ZookeeperServiceCommandManager> logger, IZookeeperClientProvider zookeeperClientProvider) : base(stringSerializer, serviceEntryManager)
        {
            _configInfo = configInfo;
            _serializer = serializer;
            _serviceRouteManager = serviceRouteManager;
            _logger = logger;
            _zookeeperClientProvider = zookeeperClientProvider;
            EnterServiceCommands().GetAwaiter().GetResult();
            _serviceRouteManager.Removed += ServiceRouteManager_Removed;
            
        }


        /// <summary>
        /// 获取所有可用的服务命令信息。
        /// </summary>
        /// <returns>服务命令集合。</returns>
        public override async Task<IEnumerable<ServiceCommandDescriptor>> GetServiceCommandsAsync()
        {
            await EnterServiceCommands();
            return _serviceCommands;
        }

        /// <summary>
        /// 清空所有的服务命令。
        /// </summary>
        /// <returns>一个任务。</returns>
        public override async Task ClearAsync()
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("准备清空所有命令配置。");
            var zooKeeperClients = await _zookeeperClientProvider.GetZooKeeperClients();
            foreach (var zooKeeperClient in zooKeeperClients)
            {

                var path = _configInfo.CommandPath;
                var childrens = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                var index = 0;
                while (childrens.Count() > 1)
                {
                    var nodePath = "/" + string.Join("/", childrens);

                    if (await zooKeeperClient.ExistsAsync(nodePath))
                    {
                        var children = await zooKeeperClient.GetChildrenAsync(nodePath);
                        if (children != null && children.Any())
                        {
                            foreach (var child in children)
                            {
                                var childPath = $"{nodePath}/{child}";
                                if (_logger.IsEnabled(LogLevel.Debug))
                                    _logger.LogDebug($"准备删除：{childPath}。");
                                await zooKeeperClient.DeleteAsync(childPath);
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
                    _logger.LogInformation("服务命令配置清空完成。");

            }
        }

        /// <summary>
        /// 设置服务命令。
        /// </summary>
        /// <param name="routes">服务命令集合。</param>
        /// <returns>一个任务。</returns>
        public override async Task SetServiceCommandsAsync(IEnumerable<ServiceCommandDescriptor> serviceCommands)
        {
          
            var zooKeeperClients = await _zookeeperClientProvider.GetZooKeeperClients();
            foreach (var zooKeeperClient in zooKeeperClients)
            {
                _logger.LogInformation($"准备向服务注册中心{zooKeeperClient.Options.ConnectionString}添加服务命令。");
                await CreateSubdirectory(zooKeeperClient, _configInfo.CommandPath);

                var path = _configInfo.CommandPath;
                if (!path.EndsWith("/"))
                    path += "/";

                serviceCommands = serviceCommands.ToArray();
                var addCount = 0;
                var exsitCount = 0;
                foreach (var command in serviceCommands)
                {

                    var nodePath = $"{path}{command.ServiceId}";
                    var nodeData = _serializer.Serialize(command);
                    if (!nodeWatchers.ContainsKey(nodePath))
                    {
                        var watcher = nodeWatchers.GetOrAdd(nodePath, f => new NodeMonitorWatcher(path, async (oldData, newData) => await NodeChange(oldData, newData)));
                        await zooKeeperClient.SubscribeDataChange(nodePath, watcher.HandleNodeDataChange);
                    }
                   
                    if (!await zooKeeperClient.ExistsAsync(nodePath))
                    {
                        _logger.LogDebug($"节点：{nodePath}不存在将进行创建。");
                        await zooKeeperClient.CreateAsync(nodePath, nodeData, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
                        addCount++;
                    }
                    else
                    {
                        _logger.LogDebug($"将更新节点：{nodePath}的数据。");
                        var onlineData = (await zooKeeperClient.GetDataAsync(nodePath)).ToArray();
                        if (!DataEquals(nodeData, onlineData))
                        {
                            await zooKeeperClient.SetDataAsync(nodePath, nodeData);
                            addCount++;
                        }
                        else
                        {
                            exsitCount++;
                        }
                    }
                    NodeChange(command);
                }
                _logger.LogInformation($"共发现{serviceCommands.Count()}个服务命令,其中,成功向服务注册中心添加{addCount}个,服务注册中心已经存在{exsitCount}个");
                   

            }
        }

        protected override async Task InitServiceCommandsAsync(IEnumerable<ServiceCommandDescriptor> serviceCommands)
        {
            var commands = await GetServiceCommands(serviceCommands.Select(p => p.ServiceId));
            if (!commands.Any() || _configInfo.ReloadOnChange)
            {
                await SetServiceCommandsAsync(serviceCommands);
            }
        }

        private void ServiceRouteManager_Removed(object sender, ServiceRouteEventArgs e)
        {
            var path = _configInfo.CommandPath;
            if (!path.EndsWith("/"))
                path += "/";
            var zooKeeperClients = _zookeeperClientProvider.GetZooKeeperClients().Result;

            try
            {

                foreach (var zooKeeperClient in zooKeeperClients)
                {
                    var nodePath = $"{path}{e.Route.ServiceDescriptor.Id}";
                    if (zooKeeperClient.StrictExistsAsync(nodePath).Result)
                    {
                        zooKeeperClient.DeleteAsync(nodePath).Wait(_configInfo.ConnectionTimeout);
                    }

                }
            }
            catch (Exception ex)
            {
                if (ex is NoNodeException || ex.InnerException is NoNodeException)
                {
                    _logger.LogDebug(ex.Message, ex);
                }
                else
                {
                    throw ex;
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

        private ServiceCommandDescriptor GetServiceCommand(byte[] data)
        {
            _logger.LogDebug($"准备转换服务命令，配置内容：{Encoding.UTF8.GetString(data)}。");
            if (data != null)
            {
                var descriptor = _serializer.Deserialize<byte[], ServiceCommandDescriptor>(data);
                return descriptor;
                
            }
            return null;

        }

        private async Task<ServiceCommandDescriptor> GetServiceCommand(string path)
        {
            ServiceCommandDescriptor result = null;
            var zooKeeperClient = await _zookeeperClientProvider.GetZooKeeperClient();
            if (zooKeeperClient == null) 
            {
                return result;
            }
            if (await zooKeeperClient.ExistsAsync(path))
            {
                var data = (await zooKeeperClient.GetDataAsync(path)).ToArray();

                if (!nodeWatchers.ContainsKey(path))
                {
                    var watcher = nodeWatchers.GetOrAdd(path, f => new NodeMonitorWatcher(path, async (oldData, newData) => await NodeChange(oldData, newData)));
                    await zooKeeperClient.SubscribeDataChange(path, watcher.HandleNodeDataChange);
                }
            
                result = GetServiceCommand(data);
            }

            return result;

        }

        private async Task<ServiceCommandDescriptor[]> GetServiceCommands(IEnumerable<string> childrens)
        {
            var rootPath = _configInfo.CommandPath;
            if (!rootPath.EndsWith("/"))
                rootPath += "/";

            childrens = childrens.ToArray();
            var serviceCommands = new List<ServiceCommandDescriptor>(childrens.Count());

            foreach (var children in childrens)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"准备从节点：{children}中获取服务命令信息。");

                var nodePath = $"{rootPath}{children}";
                var serviceCommand = await GetServiceCommand(nodePath);
                if (serviceCommand != null)
                    serviceCommands.Add(serviceCommand);
            }
            return serviceCommands.ToArray();
        }

        private async Task EnterServiceCommands()
        {
            if (_serviceCommands != null && _serviceCommands.Any())
                return;
            var zooKeeperClient = await _zookeeperClientProvider.GetZooKeeperClient();
            if (zooKeeperClient == null) 
            {
                return;
            }
            if (watcher == null) 
            {
                watcher = new ChildrenMonitorWatcher(_configInfo.CommandPath,
                                async (oldChildrens, newChildrens) => await ChildrenChange(oldChildrens, newChildrens));
               
            }
            
            await zooKeeperClient.SubscribeChildrenChange(_configInfo.CommandPath, watcher.HandleChildrenChange);

            if (await zooKeeperClient.ExistsAsync(_configInfo.CommandPath))
            {
                var childrens = (await zooKeeperClient.GetChildrenAsync(_configInfo.CommandPath)).ToArray();
                
                if (watcher != null) 
                {
                    watcher.SetCurrentData(childrens);
                }
                _serviceCommands = await GetServiceCommands(childrens);
            }
            else
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning($"无法获取服务命令信息，因为节点：{_configInfo.CommandPath}，不存在。");
                _serviceCommands = new ServiceCommandDescriptor[0];
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

        public void NodeChange(ServiceCommandDescriptor newCommand)
        {
            if (_serviceCommands != null && _serviceCommands.Any())
            {
                //得到旧的服务命令。
                var oldCommand = _serviceCommands.FirstOrDefault(i => i.ServiceId == newCommand.ServiceId);

                lock (_serviceCommands)
                {
                    //删除旧服务命令，并添加上新的服务命令。
                    _serviceCommands =
                        _serviceCommands
                            .Where(i => i.ServiceId != newCommand.ServiceId)
                            .Concat(new[] { newCommand }).ToArray();
                }

                //触发服务命令变更事件。
                OnChanged(new ServiceCommandChangedEventArgs(newCommand, oldCommand));
            }

        }

        public async Task NodeChange(byte[] oldData, byte[] newData)
        {
            if (DataEquals(oldData, newData))
                return;

            var newCommand = GetServiceCommand(newData);
            if (_serviceCommands != null && _serviceCommands.Any())
            {
                //得到旧的服务命令。
                var oldCommand = _serviceCommands.FirstOrDefault(i => i.ServiceId == newCommand.ServiceId);

                lock (_serviceCommands)
                {
                    //删除旧服务命令，并添加上新的服务命令。
                    _serviceCommands =
                        _serviceCommands
                            .Where(i => i.ServiceId != newCommand.ServiceId)
                            .Concat(new[] { newCommand }).ToArray();
                }
                //触发服务命令变更事件。
                OnChanged(new ServiceCommandChangedEventArgs(newCommand, oldCommand));
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
                _logger.LogDebug($"需要被删除的服务命令节点：{string.Join(",", deletedChildrens)}");
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"需要被添加的服务命令节点：{string.Join(",", createdChildrens)}");

            //获取新增的服务命令信息。
            var newCommands = (await GetServiceCommands(createdChildrens)).ToArray();
            if (_serviceCommands != null && _serviceCommands.Any())
            {
                var routes = _serviceCommands.ToArray();
                lock (_serviceCommands)
                {
                    _serviceCommands = _serviceCommands
                        //删除无效的节点服务命令。
                        .Where(i => !deletedChildrens.Contains(i.ServiceId))
                        //连接上新的服务命令。
                        .Concat(newCommands)
                        .ToArray();
                }
                //需要删除的服务命令集合。
                var deletedRoutes = routes.Where(i => deletedChildrens.Contains(i.ServiceId)).ToArray();
                //触发删除事件。
                OnRemoved(deletedRoutes.Select(command => new ServiceCommandEventArgs(command)).ToArray());

            }

            //触发服务命令被创建事件。
            OnCreated(newCommands.Select(command => new ServiceCommandEventArgs(command)).ToArray());

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("服务命令数据更新成功。");
        }




        public void Dispose()
        {
        }


    }
}