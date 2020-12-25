using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Consul;
using Surging.Cloud.CPlatform.Address;
using Surging.Cloud.CPlatform.Utilities;

namespace Surging.Cloud.Consul.Internal.Cluster.HealthChecks.Implementation
{
    public class DefaultHealthCheckService : IHealthCheckService,IDisposable
    {
        private readonly int _timeout = 3000;
        private readonly Timer _timer;
        private readonly ConcurrentDictionary<Tuple<string, int>, MonitorEntry> _dictionary =
        new ConcurrentDictionary<Tuple<string, int>, MonitorEntry>();

        #region Implementation of IHealthCheckService
        public DefaultHealthCheckService()
        {
            var timeSpan = TimeSpan.FromSeconds(60);

            _timer = new Timer(async s =>
            {
                await Check(_dictionary.ToArray().Select(i => i.Value), _timeout);
            }, null, timeSpan, timeSpan);
        }

        public async Task<bool> IsHealth(AddressModel address)
        {
            var ipAddress = address as IpAddressModel;
            MonitorEntry entry;
            var isHealth = !_dictionary.TryGetValue(new Tuple<string, int>(ipAddress.Ip, ipAddress.Port), out entry) ? await Check(address, _timeout) : entry.Health;
            return isHealth;
        }

        public async Task Monitor(AddressModel address)
        {
            var ipAddress = address as IpAddressModel;
            if (!_dictionary.TryGetValue(new Tuple<string, int>(ipAddress.Ip, ipAddress.Port), out MonitorEntry monitorEntry))
            {
                monitorEntry = new MonitorEntry(ipAddress, await Check(ipAddress, _timeout));
                _dictionary.TryAdd(new Tuple<string, int>(ipAddress.Ip, ipAddress.Port), monitorEntry);
            }
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            _timer.Dispose();
        }
        #endregion

        #endregion Implementation of IDisposable

        #region Private Method

        private static async Task<bool> Check(AddressModel address, int timeout)
        {
            try 
            {
                var ipAddress = address as IpAddressModel;
                //return SocketCheck.TestConnection(ipAddress.Ip, ipAddress.Port, timeout);

                var consul = new ConsulClient(config =>
                {
                    config.Address = new Uri($"http://{ipAddress.Ip}:{ipAddress.Port}");
                }, null, h => { h.UseProxy = false; h.Proxy = null; });
                await consul.Status.Leader();
                return true;
            } catch (Exception) 
            {
                return false;
            }
            

        }

        private static async Task Check(IEnumerable<MonitorEntry> entrys, int timeout)
        {
            foreach (var entry in entrys)
            {
                try 
                {
                    var ipAddress = entry.Address as IpAddressModel;
                    var consul = new ConsulClient(config =>
                    {
                        config.Address = new Uri($"http://{ipAddress.Ip}:{ipAddress.Port}");
                    }, null, h => { h.UseProxy = false; h.Proxy = null; });
                    await consul.Status.Leader();
                    entry.Health = true;
                } catch (Exception) 
                {
                    entry.Health = false;
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

            }

            public int UnhealthyTimes { get; set; }

            public AddressModel Address { get; set; }
            public bool Health { get; set; }
        }

        #endregion Help Class
    }
}
