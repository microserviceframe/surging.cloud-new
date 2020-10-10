using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Lock
{
    public class LockerProvider : ILockerProvider
    {
        public Task<IRedLock> CreateLockAsync(string resource, TimeSpan expiry)
        {
            var redlockFactory = RedLockFactory.Create(GetRedLockMultiplexers());
            return redlockFactory.CreateLockAsync(resource, expiry);

        }

        public Task<IRedLock> CreateLockAsync(string resource)
        {
            var redlockFactory = RedLockFactory.Create(GetRedLockMultiplexers());
            var defaultExpiry = TimeSpan.FromSeconds(AppConfig.LockOptions.DefaultExpiry);
            return redlockFactory.CreateLockAsync(resource, defaultExpiry);
        }

        public Task<IRedLock> CreateLockAsync()
        {
            var redlockFactory = RedLockFactory.Create(GetRedLockMultiplexers());
            var defaultResource = AppConfig.LockOptions.DefaultResource;
            var defaultExpiry = TimeSpan.FromSeconds(AppConfig.LockOptions.DefaultExpiry);
            return redlockFactory.CreateLockAsync(defaultResource, defaultExpiry);
        }

        private IList<RedLockMultiplexer> GetRedLockMultiplexers()
        {
            if (AppConfig.LockOptions == null || !AppConfig.LockOptions.LockConnections.Any())
            {
                throw new ArgumentNullException("没有设置分布式锁服务");
            }
            var multiplexers = new List<RedLockMultiplexer>();
            foreach (var redisConnection in AppConfig.LockOptions.LockConnections)
            {
                var existingConnectionMultiplexer = ConnectionMultiplexer.Connect(redisConnection);
                multiplexers.Add(existingConnectionMultiplexer);
            }
            return multiplexers;
        }
    }
}
