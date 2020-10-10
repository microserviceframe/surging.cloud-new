using Surging.Core.CPlatform.Utilities;
using System.Collections.Generic;

namespace Surging.Core.CPlatform.Configurations
{
    public class LockOptions
    {
        public LockOptions()
        {
            DefaultExpiry = 30;
            DefaultResource = "default_lock";
        }

        public string LockRedisConnections { get; set; }
        public ICollection<string> LockConnections
        {
            get
            {
                if (LockRedisConnections.IsNullOrEmpty())
                {
                    return new List<string>();
                }
                return LockRedisConnections.Split(",");
            }
        }
        public int DefaultExpiry { get; set; }
        public string DefaultResource { get; set; }
    }
}
