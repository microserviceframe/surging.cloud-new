using Surging.Cloud.CPlatform.Address;
using Surging.Cloud.CPlatform.HashAlgorithms;
using Surging.Cloud.CPlatform.Ioc;
using Surging.Cloud.CPlatform.Routing;
using Surging.Cloud.CPlatform.Runtime;
using Surging.Cloud.CPlatform.Runtime.Client;
using Surging.Cloud.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors;
using Surging.Cloud.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation;
using Surging.Cloud.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using System.Threading.Tasks;

namespace Surging.Cloud.CPlatform.Module
{
    public class EchoService : ServiceBase, IEchoService
    {
        private readonly IHashAlgorithm _hashAlgorithm;
        private readonly IAddressSelector _addressSelector;
        private readonly IServiceRouteProvider _serviceRouteProvider;
        private readonly IServiceHeartbeatManager _serviceHeartbeatManager;

        public EchoService(IHashAlgorithm hashAlgorithm, IServiceRouteProvider serviceRouteProvider,
            CPlatformContainer container, IServiceHeartbeatManager serviceHeartbeatManager)
        {
            _hashAlgorithm = hashAlgorithm;
            _addressSelector =container.GetInstances<IAddressSelector>(AddressSelectorMode.HashAlgorithm.ToString());
            _serviceRouteProvider = serviceRouteProvider;

            _serviceHeartbeatManager = serviceHeartbeatManager;
        }

        public async Task<IpAddressModel> Locate(string key,string routePath, HttpMethod httpMethod)
        {
            var route= await _serviceRouteProvider.SearchRoute(routePath, httpMethod.ToString());
            AddressModel result = new IpAddressModel();
            if (route != null)
            {
                 result = await _addressSelector.SelectAsync(new AddressSelectContext()
                {
                    Address = route.Address,
                    Descriptor = route.ServiceDescriptor,
                    Item = key,
                });
                _serviceHeartbeatManager.AddWhitelist(route.ServiceDescriptor.Id);
            } 
            var ipAddress = result as IpAddressModel;
            return ipAddress;
        }
    }
}
