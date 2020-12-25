
using Surging.Cloud.KestrelHttpServer.Filters.Implementation;
using System.Threading.Tasks;

namespace Surging.Cloud.KestrelHttpServer.Filters
{
    public interface IAuthorizationFilter : IFilter
    {
        Task OnAuthorization(AuthorizationFilterContext serviceRouteContext);

        int Order { get; }
    }
}
