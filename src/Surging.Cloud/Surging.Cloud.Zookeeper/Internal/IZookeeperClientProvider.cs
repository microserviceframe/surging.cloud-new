using Rabbit.Zookeeper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Surging.Cloud.Zookeeper.Internal
{
    public interface IZookeeperClientProvider : IDisposable
    {
        Task<IZookeeperClient> GetZooKeeperClient();

        Task<IEnumerable<IZookeeperClient>> GetZooKeeperClients();

    }
}
