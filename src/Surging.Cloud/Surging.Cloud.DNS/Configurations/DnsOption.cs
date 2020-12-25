using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.DNS.Configurations
{
    public class DnsOption
    {
        public string RootDnsAddress { get; set; }

        public int QueryTimeout { get; set; } = 1000;
    }
}
