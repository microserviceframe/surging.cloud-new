using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Utilities;

namespace Surging.Core.Consul.Internal.Cluster.HealthChecks.Implementation
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
                monitorEntry = new MonitorEntry(ipAddress);
                await Check(monitorEntry.Address, _timeout);
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
            var ipAddress = address as IpAddressModel;
            return SocketCheck.TestConnection(ipAddress.Ip, ipAddress.Port, timeout);
        }

        private static async Task Check(IEnumerable<MonitorEntry> entrys, int timeout)
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
                    entry.UnhealthyTimes++;
                    entry.Health = false;
                }
            }
        }

        #endregion Private Method

        #region Help Class

        protected class MonitorEntry
        {
            public MonitorEntry(AddressModel addressModel, bool health = true)
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
