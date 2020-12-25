using Surging.Cloud.ApiGateWay.ServiceDiscovery.Implementation;
using Surging.Cloud.CPlatform;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Cloud.ApiGateWay.ServiceDiscovery
{
    public interface IServiceSubscribeProvider
    {
        Task<IEnumerable<ServiceAddressModel>> GetAddressAsync(string condition = null);

        Task<IEnumerable<ServiceDescriptor>> GetServiceDescriptorAsync(string address, string condition = null);
    }
}
