using Surging.Cloud.ApiGateWay.ServiceDiscovery.Implementation;
using Surging.Cloud.CPlatform;
using Surging.Cloud.CPlatform.Address;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Cloud.ApiGateWay.ServiceDiscovery
{
   public interface IServiceDiscoveryProvider
    {
        Task<IEnumerable<ServiceAddressModel>> GetAddressAsync(string condition = null);

        Task<IEnumerable<ServiceDescriptor>> GetServiceDescriptorAsync(string address, string condition = null);
        
        Task EditServiceToken(AddressModel address);
    }
}
