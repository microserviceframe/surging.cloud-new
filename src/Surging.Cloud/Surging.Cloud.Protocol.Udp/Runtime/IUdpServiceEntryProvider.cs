using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.Protocol.Udp.Runtime
{
    public interface IUdpServiceEntryProvider
    {
        UdpServiceEntry GetEntry();
    }
}
