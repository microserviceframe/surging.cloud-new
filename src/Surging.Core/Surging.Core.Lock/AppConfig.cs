using Microsoft.Extensions.Configuration;
using Surging.Core.Lock.Configurations;

namespace Surging.Core.Lock
{
    public class AppConfig
    {
        public static string Path { get; internal set; }
        public static IConfigurationRoot Configuration { get; internal set; }

        public static LockOptions LockOption { get; set; } = new LockOptions();


    }
}
