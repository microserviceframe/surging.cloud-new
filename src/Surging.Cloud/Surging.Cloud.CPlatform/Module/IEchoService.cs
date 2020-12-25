using Surging.Cloud.CPlatform.Address;
using Surging.Cloud.CPlatform.Ioc;
using Surging.Cloud.CPlatform.Runtime;
using Surging.Cloud.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation;
using Surging.Cloud.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.Cloud.CPlatform.Support.Attributes;
using System.Threading.Tasks;

namespace Surging.Cloud.CPlatform.Module
{
    [ServiceBundle("")]
    public interface IEchoService: IServiceKey
    {
        [Command(ShuntStrategy = AddressSelectorMode.HashAlgorithm)]
        [HttpGet]
        Task<IpAddressModel> Locate(string key,string routePath, HttpMethod httpMethod);
    }
}
