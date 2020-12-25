using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using org.apache.zookeeper;
using Rabbit.Zookeeper;
using Rabbit.Zookeeper.Implementation;
using Surging.Cloud.CPlatform;
using Surging.Cloud.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors;
using Surging.Cloud.Zookeeper.Configurations;
using Surging.Cloud.Zookeeper.Internal.Cluster.HealthChecks;
using Surging.Cloud.Zookeeper.Internal.Cluster.Implementation.Selectors;
using Level = Microsoft.Extensions.Logging.LogLevel;

namespace Surging.Cloud.Zookeeper.Internal.Implementation
{
    public class DefaultZookeeperClientProvider : IZookeeperClientProvider
    {
        private ConfigInfo _config;
        private readonly IHealthCheckService _healthCheckService;
        private readonly IZookeeperAddressSelector _zookeeperAddressSelector;
        private readonly ILogger<DefaultZookeeperClientProvider> _logger;
        private readonly ConcurrentDictionary<string, IAddressSelector> _addressSelectors = new ConcurrentDictionary<string, IAddressSelector>();
        private readonly ConcurrentDictionary<string, IZookeeperClient> _zookeeperClients = new ConcurrentDictionary<string, IZookeeperClient>();


        public DefaultZookeeperClientProvider(ConfigInfo config, IHealthCheckService healthCheckService, IZookeeperAddressSelector zookeeperAddressSelector,
          ILogger<DefaultZookeeperClientProvider> logger)
        {
            _config = config;
            _healthCheckService = healthCheckService;
            _zookeeperAddressSelector = zookeeperAddressSelector;
            _logger = logger;
        }


        public async Task<IZookeeperClient> GetZooKeeperClient()
        {
            var conns = new List<string>();
            foreach (var conn in _config.Addresses) 
            {
                await _healthCheckService.Monitor(conn);
                if (!await _healthCheckService.IsHealth(conn)) 
                {
                    continue;
                }
                conns.Add(conn);
            }
            if (!conns.Any())
            {
                _logger.LogWarning($"找不到可用的注册中心地址。");
                return default;
            }
            var addr = await _zookeeperAddressSelector.SelectConnectionAsync(new AddressSelectContext
            {
                Descriptor = new ServiceDescriptor { Id = nameof(DefaultZookeeperClientProvider) },
                Connections = conns
            });
            return CreateZooKeeper(addr);
        }

        protected IZookeeperClient CreateZooKeeper(string conn)
        {
            if (_zookeeperClients.TryGetValue(conn, out IZookeeperClient zookeeperClient)
                    && zookeeperClient.WaitForKeeperState(Watcher.Event.KeeperState.SyncConnected, zookeeperClient.Options.OperatingTimeout))
            {
                return zookeeperClient;
            }
            else
            {
                var options = new ZookeeperClientOptions(conn)
                {
                    ConnectionTimeout = _config.ConnectionTimeout,
                    SessionTimeout = _config.SessionTimeout,
                    OperatingTimeout = _config.OperatingTimeout
                };
                zookeeperClient = new ZookeeperClient(options);
                _zookeeperClients.AddOrUpdate(conn, zookeeperClient, (k, v) => zookeeperClient);
                return zookeeperClient;
            }

        }

        public async Task<IEnumerable<IZookeeperClient>> GetZooKeeperClients()
        {
            var result = new List<IZookeeperClient>();
            foreach (var address in _config.Addresses)
            {
                if (await _healthCheckService.IsHealth(address)) 
                {
                    result.Add(CreateZooKeeper(address));
                }
            }
            return result;
        }

        public void Dispose()
        {
            if (_zookeeperClients.Any())
            {
                foreach (var client in _zookeeperClients)
                {
                    client.Value.Dispose();
                }
            }
        }
    }
}
