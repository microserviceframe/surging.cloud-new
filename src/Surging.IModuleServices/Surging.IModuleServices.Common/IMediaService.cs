using Surging.Cloud.CPlatform.Ioc;
using Surging.Cloud.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.Cloud.Protocol.WS.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.IModuleServices.Common
{
    [ServiceBundle("Api/{Service}")]
    [BehaviorContract(IgnoreExtensions =true,Protocol = "media")]
    public interface IMediaService : IServiceKey
    { 
        Task Push(IEnumerable<byte> data);
    }

}
