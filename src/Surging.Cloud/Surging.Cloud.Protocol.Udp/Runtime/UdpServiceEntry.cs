using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.Protocol.Udp.Runtime
{
   public class UdpServiceEntry
    {
        public string Path { get; set; }

        public Type Type { get; set; }

        public UdpBehavior Behavior { get; set; }
    }

}
