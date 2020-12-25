using Microsoft.Extensions.Configuration;
using Surging.Cloud.DNS.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.DNS
{
    public static  class AppConfig
    {
        public static IConfigurationRoot Configuration { get; set; }

        public static DnsOption DnsOption { get; set; }
    }
}
