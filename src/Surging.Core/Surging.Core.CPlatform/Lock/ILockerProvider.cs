
using RedLockNet;
using System;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Lock
{
    public interface ILockerProvider
    {
        Task<IRedLock> CreateLockAsync(string resource, TimeSpan expiry);

        Task<IRedLock> CreateLockAsync(string resource);

        Task<IRedLock> CreateLockAsync();
    }
}
