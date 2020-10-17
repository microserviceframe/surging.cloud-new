using System;

namespace Surging.Core.Lock.Configurations
{
    public class LockOptions
    {
        public LockOptions() 
        {
            DefaultExpiry = 120;
            Wait = 30;
            Retry = 1;
            DefaultResource = "default_lock";
        }

        public string LockRedisConnection { get; set; }
      
        public int DefaultExpiry { get; set; }

        public int Wait { get; set; }

        public int Retry { get; set; }

        public string DefaultResource { get; set; }

        public TimeSpan DefaultExpiryTimeSpan => TimeSpan.FromSeconds(DefaultExpiry);

        public TimeSpan WaitTimeSpan => TimeSpan.FromSeconds(Wait);

        public TimeSpan RetryTimeSpan => TimeSpan.FromSeconds(Retry);
    }
}
