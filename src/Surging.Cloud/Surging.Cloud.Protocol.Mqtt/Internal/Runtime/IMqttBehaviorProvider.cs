using Surging.Cloud.Protocol.Mqtt.Internal.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.Protocol.Mqtt.Internal.Runtime
{
   public interface IMqttBehaviorProvider
    {
        MqttBehavior GetMqttBehavior();
    }
}
