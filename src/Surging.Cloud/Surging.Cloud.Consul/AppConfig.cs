using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.Consul
{
   public class AppConfig
    {
        public static IConfigurationRoot Configuration { get; set; }
    }
}
