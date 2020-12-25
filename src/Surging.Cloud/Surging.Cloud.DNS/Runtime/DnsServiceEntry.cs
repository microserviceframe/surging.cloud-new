using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.DNS.Runtime
{
   public class DnsServiceEntry
    {
        public string Path { get; set; }

        public Type Type { get; set; }

        public DnsBehavior Behavior { get; set; }

    }
}
