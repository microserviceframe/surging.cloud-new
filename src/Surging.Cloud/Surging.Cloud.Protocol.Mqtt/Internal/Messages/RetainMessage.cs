using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.Protocol.Mqtt.Internal.Messages
{
   public  class RetainMessage
    {
        public byte[] ByteBuf { get; set; }

        public int QoS { get; set; }
        public new string ToString => Encoding.UTF8.GetString(ByteBuf);
    }
}
