using Surging.Cloud.CPlatform.Runtime.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.Protocol.Mqtt.Internal.Runtime
{
    public class MqttRemoteInvokeContext: RemoteInvokeContext
    {
         public string topic { get; set; }
    }
}
 