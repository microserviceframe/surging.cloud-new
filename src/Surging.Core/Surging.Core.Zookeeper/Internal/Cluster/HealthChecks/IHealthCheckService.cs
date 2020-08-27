using Surging.Core.CPlatform.Address;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Zookeeper.Internal.Cluster.HealthChecks
{
   public interface IHealthCheckService
    {
        Task Monitor(AddressModel address);

        Task<bool> IsHealth(AddressModel address);
    }
}
