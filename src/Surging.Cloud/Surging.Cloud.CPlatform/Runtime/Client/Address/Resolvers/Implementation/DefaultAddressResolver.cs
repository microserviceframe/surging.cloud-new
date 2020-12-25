using Autofac.Core;
using Microsoft.Extensions.Logging;
using Surging.Cloud.CPlatform.Address;
using Surging.Cloud.CPlatform.Diagnostics;
using Surging.Cloud.CPlatform.Exceptions;
using Surging.Cloud.CPlatform.Routing;
using Surging.Cloud.CPlatform.Routing.Implementation;
using Surging.Cloud.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors;
using Surging.Cloud.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation;
using Surging.Cloud.CPlatform.Runtime.Client.HealthChecks;
using Surging.Cloud.CPlatform.Support;
using Surging.Cloud.CPlatform.Transport.Implementation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surging.Cloud.CPlatform.Runtime.Client.Address.Resolvers.Implementation
{
    /// <summary>
    /// 默认的服务地址解析器。
    /// </summary>
    public class DefaultAddressResolver : IAddressResolver
    {
        #region Field

        private readonly IServiceRouteProvider _serviceRouteProvider;
        private readonly ILogger<DefaultAddressResolver> _logger;
        private readonly IHealthCheckService _healthCheckService;
        private readonly CPlatformContainer _container;
        private readonly ConcurrentDictionary<string, IAddressSelector> _addressSelectors = new
            ConcurrentDictionary<string, IAddressSelector>();
        private readonly IServiceCommandProvider _commandProvider;
        private readonly IServiceHeartbeatManager _serviceHeartbeatManager;

        #endregion Field

        #region Constructor

        public DefaultAddressResolver(IServiceCommandProvider commandProvider,
            ILogger<DefaultAddressResolver> logger,
            CPlatformContainer container,
            IHealthCheckService healthCheckService,
            IServiceHeartbeatManager serviceHeartbeatManager,
            IServiceRouteProvider serviceRouteProvider)
        {
            _container = container;
            _logger = logger;
            LoadAddressSelectors();
            _commandProvider = commandProvider;
            _healthCheckService = healthCheckService;
            _serviceHeartbeatManager = serviceHeartbeatManager;
            _serviceRouteProvider = serviceRouteProvider;
        }

        #endregion Constructor

        #region Implementation of IAddressResolver

        /// <summary>
        /// 解析服务地址。
        /// </summary>
        /// <param name="serviceId">服务Id。</param>
        /// <returns>服务地址模型。</returns>
        /// 1.从字典中拿到serviceroute对象
        /// 2.从字典中拿到服务描述符集合
        /// 3.获取或添加serviceroute
        /// 4.添加服务id到白名单
        /// 5.根据服务描述符得到地址并判断地址是否是可用的（地址应该是多个）
        /// 6.添加到集合中
        /// 7.拿到服务命今
        /// 8.根据负载分流策略拿到一个选择器
        /// 9.返回addressmodel
        public async Task<AddressModel> Resolver(string serviceId, string item)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"准备为服务id：{serviceId}，解析可用地址。");

            var isFailoverInvoke = IsFailoverInvoke();
            var serviceRoute = await _serviceRouteProvider.Locate(serviceId, !isFailoverInvoke);
            if (serviceRoute == null) 
            {
                throw new CPlatformException($"根据服务id：{serviceId}，找不到相关服务信息。【fromCache:{!isFailoverInvoke}】");
            }
            var address = await GetHealthAddress(serviceRoute);
            if (!address.Any()) 
            {
                throw new CPlatformException($"根据服务id：{serviceId},找不到可用的服务提供者的地址");
            }

            _serviceHeartbeatManager.AddWhitelist(serviceId);
           
            var command = await _commandProvider.GetCommand(serviceId);
            var addressSelector = _addressSelectors[command.ShuntStrategy.ToString()];

            var selectAddress = await addressSelector.SelectAsync(new AddressSelectContext
            {
                Descriptor = serviceRoute.ServiceDescriptor,
                Address = address,
                Item = item
            });
            return selectAddress;
        }

        private bool IsFailoverInvoke()
        {
            var isFailoverAttachmentValue = RpcContext.GetContext().GetAttachment("isFailoverInvoke");
            if (isFailoverAttachmentValue == null) 
            {
                return false;
            }
            return Convert.ToBoolean(isFailoverAttachmentValue);
        }

        private async Task<IEnumerable<AddressModel>> GetHealthAddress(ServiceRoute serviceRoute)
        {
            _logger.LogDebug($"根据服务id：{serviceRoute.ServiceDescriptor.Id},找到如下所有地址：{string.Join(",", serviceRoute.Address.Select(i => i.ToString()))}。");
            var address = new List<AddressModel>();
            foreach (var addressModel in serviceRoute.Address)
            {
                var isHealth = await _healthCheckService.IsHealth(addressModel);
                if (!isHealth)
                {
                    continue;
                }
                address.Add(addressModel);
            }
            if (!address.Any())
            {
                _logger.LogWarning($"根据服务id：{serviceRoute.ServiceDescriptor.Id}，找不到可用的地址。");
                return address;
            }
            _logger.LogInformation($"根据服务id：{serviceRoute.ServiceDescriptor.Id}，找到以下可用地址：{string.Join(",", address.Select(i => i.ToString()))}。");
            return address;
        }

        private void LoadAddressSelectors()
        {
            foreach (AddressSelectorMode item in Enum.GetValues(typeof(AddressSelectorMode)))
            {
                _addressSelectors.TryAdd(item.ToString(), _container.GetInstances<IAddressSelector>(item.ToString()));
            }
        }

        #endregion Implementation of IAddressResolver
    }
}