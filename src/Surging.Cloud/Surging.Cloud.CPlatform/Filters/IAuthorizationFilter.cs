using Surging.Cloud.CPlatform.Routing;
using Surging.Cloud.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Cloud.CPlatform.Filters
{
    public interface IAuthorizationFilter: IFilter
    {
        void ExecuteAuthorizationFilterAsync(ServiceRouteContext serviceRouteContext,CancellationToken cancellationToken);
    }
}
