using Surging.Cloud.CPlatform.Ioc;
using Surging.Cloud.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation;
using Surging.Cloud.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.Cloud.CPlatform.Support.Attributes;
using Surging.Cloud.Protocol.WS;
using Surging.Cloud.Protocol.WS.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.IModuleServices.Common
{
    [ServiceBundle("Api/{Service}")]
    [BehaviorContract(IgnoreExtensions =true)]
    public  interface IChatService: IServiceKey
    {
        [Command( ShuntStrategy=AddressSelectorMode.HashAlgorithm)]
        Task SendMessage(string name,string data);
    }
}
