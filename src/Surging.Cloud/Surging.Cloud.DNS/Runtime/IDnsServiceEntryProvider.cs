using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.DNS.Runtime
{
    public interface IDnsServiceEntryProvider
    {
        DnsServiceEntry GetEntry();
    }
}
