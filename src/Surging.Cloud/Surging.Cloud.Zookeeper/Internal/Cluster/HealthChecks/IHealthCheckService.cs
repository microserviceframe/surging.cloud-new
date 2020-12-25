using Surging.Cloud.CPlatform.Address;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Cloud.Zookeeper.Internal.Cluster.HealthChecks
{
   public interface IHealthCheckService
    {
        Task Monitor(string conn);

        Task<bool> IsHealth(string conn);
    }
}
