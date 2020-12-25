using Surging.Cloud.CPlatform.Ioc;
using Surging.Cloud.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation;
using Surging.Cloud.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.Cloud.CPlatform.Support.Attributes;
using Surging.Cloud.Protocol.Mqtt.Internal.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Cloud.Protocol.Mqtt.Internal.Runtime
{
    [ServiceBundle("Device")]
    public interface IMqttRomtePublishService : IServiceKey
    {
        [Command(ShuntStrategy = AddressSelectorMode.HashAlgorithm)]
        Task Publish(string deviceId, MqttWillMessage message);
    }
} 
