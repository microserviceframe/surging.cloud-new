using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Surging.Core.Lock.Provider
{
    public class LockerProvider : ILockerProvider
    {

        private readonly RedLockFactory _redLockFactory;
        public LockerProvider() 
        {
            _redLockFactory = RedLockFactory.Create(GetRedLockMultiplexers());
        }

        public Task<IRedLock> CreateLockAsync(string resource, TimeSpan expiry,TimeSpan wait, TimeSpan retry)
        {
            return _redLockFactory.CreateLockAsync(resource, expiry, wait, retry);

        }

        public Task<IRedLock> CreateLockAsync(string resource, TimeSpan expiry) 
        {
            return CreateLockAsync(resource, expiry, AppConfig.LockOption.WaitTimeSpan, AppConfig.LockOption.RetryTimeSpan);

        }

        public Task<IRedLock> CreateLockAsync(string resource)
        {
            return CreateLockAsync(resource, AppConfig.LockOption.DefaultExpiryTimeSpan, AppConfig.LockOption.WaitTimeSpan, AppConfig.LockOption.RetryTimeSpan);
        }

        public Task<IRedLock> CreateLockAsync()
        {                    
            return CreateLockAsync(AppConfig.LockOption.DefaultResource);
        }

        void IDisposable.Dispose()
        {
            _redLockFactory.Dispose();
        }

        private IList<RedLockMultiplexer> GetRedLockMultiplexers() 
        {
            if (AppConfig.LockOption == null) 
            {
                throw new ArgumentNullException("没有设置分布式锁服务");
            }
            var multiplexers = new List<RedLockMultiplexer>();
            var existingConnectionMultiplexer = ConnectionMultiplexer.Connect(AppConfig.LockOption.LockRedisConnection);
            multiplexers.Add(existingConnectionMultiplexer);
            return multiplexers;
        }
    }
}
