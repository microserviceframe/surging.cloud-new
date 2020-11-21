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
        private readonly IServiceEntryManager _serviceEntryManager;
        private readonly IServiceRouteProvider _serviceRouteProvider;
        private readonly int _timeout = AppConfig.ServerOptions.HealthCheckTimeout;
        private readonly Timer _serviceHealthCheckTimer;
        private readonly Timer _synchServiceRoutesTimer;
        private readonly ILogger<DefaultHealthCheckService> _logger;
        public event EventHandler<HealthCheckEventArgs> Removed;
        public event EventHandler<HealthCheckEventArgs> Changed;

        /// <summary>
        /// 默认心跳检查服务(每10秒会检查一次服务状态，在构造函数中添加服务管理事件) 
        /// </summary>
        /// <param name="serviceRouteManager"></param>
        public DefaultHealthCheckService(IServiceRouteManager serviceRouteManager, IServiceEntryManager serviceEntryManager, IServiceRouteProvider serviceRouteProvider)
        {
            _serviceRouteManager = serviceRouteManager;
            _serviceEntryManager = serviceEntryManager;
            _serviceRouteProvider = serviceRouteProvider;
            _logger = ServiceLocator.GetService<ILogger<DefaultHealthCheckService>>();
            var timeSpan = TimeSpan.FromSeconds(AppConfig.ServerOptions.HealthCheckWatchIntervalInSeconds);

            //建立计时器
            _serviceHealthCheckTimer = new Timer(async s =>
            {
                //检查服务是否可用
                await Check(_dictionaries.ToArray().Select(i => i.Value), _timeout);

            }, null, timeSpan, timeSpan);

            //if (AppConfig.ServerOptions.CheckServiceRegister) 
            //{
            //    var synchServiceRoutesTimeSpan = GetSynchServiceRoutesTimeSpan();
            //    _synchServiceRoutesTimer = new Timer(async s =>
            //    {
            //        await CheckServiceRegister();

            //    }, null, synchServiceRoutesTimeSpan, synchServiceRoutesTimeSpan);

            //}

            //去除监控。
            _serviceRouteManager.Removed += (s, e) =>
            {
                Remove(e.Route.Address);
            };
            //重新监控。
            _serviceRouteManager.Created += async (s, e) =>
            {
                var keys = e.Route.Address.Select(address =>
                {
                    var ipAddress = address as IpAddressModel;
                    return new Tuple<string, int>(ipAddress.Ip, ipAddress.Port);
                });
                await Check(_dictionaries.Where(i => keys.Contains(i.Key)).Select(i => i.Value), _timeout);

            };
            //重新监控。
            _serviceRouteManager.Changed += async (s, e) =>
            {
                var keys = e.Route.Address.Select(address =>
                {
                    var ipAddress = address as IpAddressModel;
                    return new Tuple<string, int>(ipAddress.Ip, ipAddress.Port);
                });
                await Check(_dictionaries.Where(i => keys.Contains(i.Key)).Select(i => i.Value), _timeout);

            };

        }

        private TimeSpan GetSynchServiceRoutesTimeSpan()
        {
            var random = new Random();
            var seed = random.Next(1, 60);
            return TimeSpan.FromSeconds(AppConfig.ServerOptions.CheckServiceRegisterIntervalInSeconds + seed);
        }

        //private async Task CheckServiceRegister()
        //{
        //    var serviceRoutes = await _serviceRouteManager.GetRoutesAsync();
        //    if (serviceRoutes == null)
        //    {
        //        _logger.LogWarning("从服务注册中心获取路由失败");
        //        return;
        //    }
        //    var localServiceEntries = _serviceEntryManager.GetEntries();
        //    var localServiceRoutes = serviceRoutes.Where(p => localServiceEntries.Any(q => q.Descriptor.Id == p.ServiceDescriptor.Id));
        //    var addess = NetUtils.GetHostAddress();
        //    var registerServiceEntries = localServiceEntries.Where(e => localServiceRoutes.Any(p => !p.Address.Any(q => q.Equals(addess)) && p.ServiceDescriptor.Id == e.Descriptor.Id));
        //    if (registerServiceEntries.Any())
        //    {
        //        _logger.LogWarning($"服务路由未注册成功,重新注册服务路由,服务条目数量为:{registerServiceEntries.Count()}");
        //        try
        //        {
        //            await _serviceRouteProvider.RegisterRoutes(registerServiceEntries);
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError($"服务路由注册失败,原因:{ex.Message}");
        //        }

        //    }


        //}


        #region Implementation of IHealthCheckService

        /// <summary>
        /// 监控一个地址。
        /// </summary>
        /// <param name="address">地址模型。</param>
        /// <returns>一个任务。</returns>
        public async Task Monitor(AddressModel address)
        {
            var ipAddress = address as IpAddressModel;
            if (!_dictionaries.TryGetValue(new Tuple<string, int>(ipAddress.Ip, ipAddress.Port), out MonitorEntry monitorEntry))
            {
                monitorEntry = new MonitorEntry(ipAddress, Check(ipAddress, _timeout));
                _dictionaries.TryAdd(new Tuple<string, int>(ipAddress.Ip, ipAddress.Port), monitorEntry);
            }
            OnChanged(new HealthCheckEventArgs(address, monitorEntry.Health));
        }


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
                entry = new MonitorEntry(address, Check(ipAddress, _timeout));
                _dictionaries.TryAdd(new Tuple<string, int>(ipAddress.Ip, ipAddress.Port), entry);
            }

            if (entry.UnhealthyTimes >= AppConfig.ServerOptions.AllowServerUnhealthyTimes)
            {
                await RemoveUnhealthyAddress(entry);
            }
            OnChanged(new HealthCheckEventArgs(address, entry.Health));
            return entry.Health;

        }


        /// <summary>
        /// 标记一个地址为失败的。
        /// </summary>
        /// <param name="address">地址模型。</param>
        /// <returns>一个任务。</returns>
        public Task MarkFailure(AddressModel address)
        {
            return Task.Run(() =>
            {
                var ipAddress = address as IpAddressModel;
                var entry = _dictionaries.GetOrAdd(new Tuple<string, int>(ipAddress.Ip, ipAddress.Port), k => new MonitorEntry(address, Check(ipAddress, _timeout)));
                entry.Health = false;
                entry.UnhealthyTimes += 1;
            });
        }
        public Task MarkSuccess(AddressModel address, string serviceId)
        {
            return Task.Run(() =>
            {
                var ipAddress = address as IpAddressModel;
               
            });
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
            _serviceHealthCheckTimer.Dispose();
            if (_synchServiceRoutesTimer != null)
            {
                _synchServiceRoutesTimer.Dispose();
            }

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

        private bool Check(IpAddressModel ipAddress, int timeout)
        {
            return SocketCheck.TestConnection(ipAddress.Ip, ipAddress.Port, timeout);
        }

        private async Task Check(IEnumerable<MonitorEntry> entrys, int timeout)
        {
            foreach (var entry in entrys)
            {
                var ipAddress = entry.Address as IpAddressModel;
                if (SocketCheck.TestConnection(ipAddress.Ip, ipAddress.Port, timeout))
                {
                    entry.UnhealthyTimes = 0;
                    entry.Health = true;
                }
                else
                {
                    if (entry.UnhealthyTimes >= AppConfig.ServerOptions.AllowServerUnhealthyTimes)
                    {
                        _logger.LogWarning($"服务地址{entry.Address}不健康,UnhealthyTimes={entry.UnhealthyTimes},服务将会被移除");
                        await RemoveUnhealthyAddress(entry);
                    }
                    else
                    {
                        entry.UnhealthyTimes++;
                        entry.Health = false;
                        _logger.LogWarning($"服务地址{entry.Address}不健康,UnhealthyTimes={entry.UnhealthyTimes}");
                    }
                }

            }
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

            public int TimeOutTimes { get; set; }

            public AddressModel Address { get; set; }

            public bool Health { get; set; }
        }

        #endregion Help Class
    }
}