using System.Threading.Tasks;
using Surging.Cloud.CPlatform.Ioc;
using Surging.Cloud.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.Cloud.Domain.PagedAndSorted;
using Surging.IModuleServices.Common.Models;

namespace Surging.IModuleServices.Common
{
    [ServiceBundle("api/{DataService}")]
    public interface ITestPermissionDataService : IServiceKey
    {
        [ServiceRoute("search")]
        [HttpPost]
        Task<IPagedResult<PermissionData>> Search(QueryPermissionData query);
    }
}