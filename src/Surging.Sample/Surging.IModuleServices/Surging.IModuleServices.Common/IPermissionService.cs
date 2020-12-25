using Surging.Cloud.CPlatform.Ioc;
using Surging.Cloud.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using System.Threading.Tasks;

namespace Surging.IModuleServices.Common
{
    [ServiceBundle("Api/{Service}")]
    public interface IPermissionService : IServiceKey
    {
        Task<bool> Check(string serviceId);
    }
}
