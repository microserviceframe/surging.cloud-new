using RedLockNet;
using Surging.Core.CPlatform.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.Lock
{
    public static class IRedLockExtensions
    {
        public static async Task Lock(this IRedLock redLock, Func<Task> callback) 
        {
            var lockerTime = DateTime.Now;
            while (true) 
            {
                if (redLock.IsAcquired) 
                {
                    await callback();
                    break;
                }
                Thread.Sleep(AppConfig.LockOption.RetryTimeSpan);
                if (DateTime.Now - lockerTime > AppConfig.LockOption.WaitTimeSpan) 
                {
                    throw new CPlatformException($"获取分布式锁资源{redLock.Resource}超时");
                }
            }
        }

        public static async Task<T> Lock<T>(this IRedLock redLock, Func<Task<T>> callback)
        {
            var lockerTime = DateTime.Now;
            while (true)
            {
                if (redLock.IsAcquired)
                {
                    return await callback();
                }
                Thread.Sleep(AppConfig.LockOption.RetryTimeSpan);
                if (DateTime.Now - lockerTime > AppConfig.LockOption.WaitTimeSpan)
                {
                    throw new CPlatformException($"获取分布式锁资源{redLock.Resource}超时");
                }
            }
        }
    }
}
