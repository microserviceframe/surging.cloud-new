using Surging.Core.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Zookeeper.Internal.Cluster.Implementation.Selectors
{
    public interface IZookeeperAddressSelector: IAddressSelector
    {
        Task<string> SelectConnectionAsync(AddressSelectContext context);
    }
}
