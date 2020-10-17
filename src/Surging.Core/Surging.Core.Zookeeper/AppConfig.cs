using Microsoft.Extensions.Configuration;
using Surging.Core.Zookeeper.Configurations;

namespace Surging.Core.Zookeeper
{
   public class AppConfig
    {
        public static IConfigurationRoot Configuration { get; set; }

        public static ConfigInfo Config { get; internal set; }
    }
}
