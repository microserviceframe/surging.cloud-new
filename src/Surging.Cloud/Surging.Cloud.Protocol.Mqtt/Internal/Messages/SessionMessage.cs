using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.Protocol.Mqtt.Internal.Messages
{
   public class SessionMessage
    {
        public byte[] Message { get; set; } 

        public int QoS { get; set; }

        public string Topic { get; set; }

    }
}
