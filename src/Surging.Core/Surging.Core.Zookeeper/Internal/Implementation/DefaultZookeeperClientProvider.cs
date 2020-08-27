using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using org.apache.zookeeper;
using Rabbit.Zookeeper;
using Rabbit.Zookeeper.Implementation;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Exceptions;
using Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors;
using Surging.Core.Zookeeper.Configurations;
using Surging.Core.Zookeeper.Internal.Cluster.HealthChecks;
using Surging.Core.Zookeeper.Internal.Cluster.Implementation.Selectors;
using Level = Microsoft.Extensions.Logging.LogLevel;

namespace Surging.Core.Zookeeper.Internal.Implementation
{
    public class DefaultZookeeperClientProvider : IZookeeperClientProvider
    {
        private ConfigInfo _config;
        private readonly IHealthCheckService _healthCheckService;
        private readonly IZookeeperAddressSelector _zookeeperAddressSelector;
        private readonly ILogger<DefaultZookeeperClientProvider> _logger;
        private readonly ConcurrentDictionary<string, IAddressSelector> _addressSelectors = new ConcurrentDictionary<string, IAddressSelector>();
        private readonly ConcurrentDictionary<AddressModel, IZookeeperClient> _zookeeperClients = new ConcurrentDictionary<AddressModel, IZookeeperClient>();


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
            var address = new List<AddressModel>();
            foreach (var addressModel in _config.Addresses)
            {
                //await _healthCheckService.Monitor(addressModel);
                //var isHealth = await _healthCheckService.IsHealth(addressModel);
                //if (!isHealth)
                //{
                //    _logger.LogWarning($"服务注册中心地址{addressModel.ToString()}不健康。");
                //    continue;
                //}
                address.Add(addressModel);
            }
            if (!address.Any())
            {
                if (_logger.IsEnabled(Level.Warning))
                    _logger.LogWarning($"找不到可用的注册中心地址。");
                throw new CPlatformException("找不到可用的Zookeeper注册中心地址");
            }

            var addr = await _zookeeperAddressSelector.SelectAsync(new AddressSelectContext
            {
                Descriptor = new ServiceDescriptor { Id = nameof(DefaultZookeeperClientProvider) },
                Address = address
            });
            if (addr != null)
            {
                var ipAddress = addr as IpAddressModel;
                return CreateZooKeeper(ipAddress);
            }
            throw new CPlatformException("找不到可用的Zookeeper注册中心地址");
        }

        protected IZookeeperClient CreateZooKeeper(IpAddressModel ipAddress)
        {
            if (!_zookeeperClients.TryGetValue(ipAddress, out IZookeeperClient zookeeperClient))
            {
                var options = new ZookeeperClientOptions(ipAddress.ToString()) { ConnectionTimeout = _config.SessionTimeout, SessionTimeout = _config.SessionTimeout };
                zookeeperClient = new ZookeeperClient(options);

                _zookeeperClients.AddOrUpdate(ipAddress, zookeeperClient, (k,v)=> zookeeperClient);
            }
            return zookeeperClient;
        }

        public async Task<IEnumerable<IZookeeperClient>> GetZooKeeperClients()
        {
            var result = new List<IZookeeperClient>();
            foreach (var address in _config.Addresses)
            {
                var ipAddress = address as IpAddressModel;
                //if (await _healthCheckService.IsHealth(address))
                //{
                //    result.Add(CreateZooKeeper(ipAddress));

                //}
                result.Add(CreateZooKeeper(ipAddress));
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
