using Surging.Cloud.CPlatform.Messages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Surging.Cloud.CPlatform.Support
{
    public interface IBreakeRemoteInvokeService
    {
        Task<RemoteInvokeResultMessage> InvokeAsync(IDictionary<string, object> parameters, string serviceId, string _serviceKey, bool decodeJOject, bool isFailoverCall = false);
    }
}
