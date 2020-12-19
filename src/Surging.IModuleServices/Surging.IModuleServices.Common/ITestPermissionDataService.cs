using System.Threading.Tasks;
using Surging.Core.CPlatform.Ioc;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.Core.Domain.PagedAndSorted;
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