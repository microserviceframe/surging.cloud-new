using Microsoft.Extensions.Configuration;
using Surging.Cloud.Zookeeper.Configurations;

namespace Surging.Cloud.Zookeeper
{
   public class AppConfig
    {
        public static IConfigurationRoot Configuration { get; set; }

        public static ConfigInfo Config { get; internal set; }
    }
}
