using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Configurations;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Runtime.Client.HealthChecks.Implementation
{
    /// <summary>
    /// 默认健康检查服务(每10秒会检查一次服务状态，在构造函数中添加服务管理事件) 
    /// </summary>
    public class DefaultHealthCheckService : IHealthCheckService, IDisposable
    {
        private readonly ConcurrentDictionary<Tuple<string, int>, MonitorEntry> _dictionaries = new ConcurrentDictionary<Tuple<string, int>, MonitorEntry>();


        private readonly IServiceRouteManager _serviceRouteManager;
        private readonly int _timeout = AppConfig.ServerOptions.ConnectTimeout;
        private readonly ILogger<DefaultHealthCheckService> _logger;
        public event EventHandler<HealthCheckEventArgs> Removed;
        public event EventHandler<HealthCheckEventArgs> Changed;

        /// <summary>
        /// 默认心跳检查服务(每10秒会检查一次服务状态，在构造函数中添加服务管理事件) 
        /// </summary>
        /// <param name="serviceRouteManager"></param>
        public DefaultHealthCheckService(IServiceRouteManager serviceRouteManager)
        {
            _serviceRouteManager = serviceRouteManager;
            _logger = ServiceLocator.GetService<ILogger<DefaultHealthCheckService>>();

            //去除监控。
            _serviceRouteManager.Removed += (s, e) =>
            {
                Remove(e.Route.Address);
            };
            //重新监控。
            _serviceRouteManager.Created += async (s, e) =>
            {
                Remove(e.Route.Address);

            };
            //重新监控。
            _serviceRouteManager.Changed += async (s, e) =>
            {
                Remove(e.Route.Address);
            };

        }
        
        #region Implementation of IHealthCheckService


        /// <summary>
        /// 判断一个地址是否健康。
        /// </summary>
        /// <param name="address">地址模型。</param>
        /// <returns>健康返回true，否则返回false。</returns>
        public async Task<bool> IsHealth(AddressModel address)
        {
            var ipAddress = address as IpAddressModel;
            MonitorEntry entry;
            if (!_dictionaries.TryGetValue(new Tuple<string, int>(ipAddress.Ip, ipAddress.Port), out entry))
            {
                entry = new MonitorEntry(address, true);
                _dictionaries.TryAdd(new Tuple<string, int>(ipAddress.Ip, ipAddress.Port), entry);
            }
            OnChanged(new HealthCheckEventArgs(address, entry.Health));
            return entry.Health;

        }


        /// <summary>
        /// 标记一个地址为失败的。
        /// </summary>
        /// <param name="address">地址模型。</param>
        /// <returns>一个任务。</returns>
        public async Task<int> MarkFailure(AddressModel address)
        {
            var ipAddress = address as IpAddressModel;
            var entry = _dictionaries.GetOrAdd(new Tuple<string, int>(ipAddress.Ip, ipAddress.Port), k => new MonitorEntry(address,false));
            entry.Health = false;
            entry.UnhealthyTimes += 1;
            if (entry.UnhealthyTimes > AppConfig.ServerOptions.AllowServerUnhealthyTimes)
            {
                await RemoveUnhealthyAddress(entry);
            }
            return entry.UnhealthyTimes;
        }


        protected void OnRemoved(params HealthCheckEventArgs[] args)
        {
            if (Removed == null)
                return;

            foreach (var arg in args)
                Removed(this, arg);
        }

        protected void OnChanged(params HealthCheckEventArgs[] args)
        {
            if (Changed == null)
                return;

            foreach (var arg in args)
                Changed(this, arg);
        }

        #endregion Implementation of IHealthCheckService

        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
        }

        #endregion Implementation of IDisposable

        #region Private Method

        private void Remove(IEnumerable<AddressModel> addressModels)
        {
            foreach (var addressModel in addressModels)
            {
                MonitorEntry value;
                var ipAddress = addressModel as IpAddressModel;
                _dictionaries.TryRemove(new Tuple<string, int>(ipAddress.Ip, ipAddress.Port), out value);
            }
        }


        private async Task RemoveUnhealthyAddress(MonitorEntry monitorEntry)
        {
            var ipAddress = monitorEntry.Address as IpAddressModel;
            await _serviceRouteManager.RemveAddressAsync(new List<AddressModel>() { ipAddress });
            _dictionaries.TryRemove(new Tuple<string, int>(ipAddress.Ip, ipAddress.Port), out MonitorEntry value);
            OnRemoved(new HealthCheckEventArgs(ipAddress));
        }



        #endregion Private Method

        #region Help Class

        protected class MonitorEntry
        {
            public MonitorEntry(AddressModel addressModel, bool health)
            {
                Address = addressModel;
                Health = health;
                UnhealthyTimes = 0;

            }

            public int UnhealthyTimes { get; set; }

            public AddressModel Address { get; set; }

            public bool Health { get; set; }

        }

        #endregion Help Class
    }
}