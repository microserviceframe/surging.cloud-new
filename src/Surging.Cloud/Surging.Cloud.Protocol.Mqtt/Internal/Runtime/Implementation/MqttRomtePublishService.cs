using Surging.Cloud.CPlatform.Ioc;
using Surging.Cloud.CPlatform.Utilities;
using Surging.Cloud.Protocol.Mqtt.Internal.Messages;
using Surging.Cloud.Protocol.Mqtt.Internal.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Cloud.Protocol.Mqtt.Internal.Runtime.Implementation
{
    public class MqttRomtePublishService : ServiceBase, IMqttRomtePublishService
    {
       public async Task Publish(string deviceId, MqttWillMessage message)
        {
            await ServiceLocator.GetService<IChannelService>().Publish(deviceId, message);
        }
    }
}
