using System.Threading.Tasks;
using Surging.Cloud.CPlatform.Ioc;
using Surging.Cloud.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.Cloud.KestrelHttpServer;

namespace Surging.IModuleServices.Common
{
    [ServiceBundle("{Service}")]
    public interface IFileService : IServiceKey
    {
        [Service(EnableAuthorization = false)]
        Task<IActionResult> Preview(string fileId);
    }
}