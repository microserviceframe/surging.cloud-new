using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ApiGateWay.OAuth
{
    public interface IAuthorizationServerProvider
    {
        Task<string> IssueToken(Dictionary<string, object> parameters);

        string RefreshToken(string token);

        IDictionary<string, object> GetPayload(string token);

        ValidateResult ValidateClientAuthentication(string token);
    }
}
