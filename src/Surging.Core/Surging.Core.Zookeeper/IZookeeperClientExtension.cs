using Microsoft.Extensions.Configuration;
using Rabbit.Zookeeper;
using Surging.Core.CPlatform.Utilities;
using System;
using System.Threading.Tasks;
using static org.apache.zookeeper.KeeperException;

namespace Surging.Core.Zookeeper
{
    public static class IZookeeperClientExtension
    {
        public static async Task<bool> StrictExistsAsync(this IZookeeperClient zookeeperClient,string path) 
        {
            try
            {
                return await zookeeperClient.ExistsAsync(path);
            }
            catch (Exception ex) 
            {
                return false;
            }
            
        }

        public static async Task<ZookeeperLocker> Lock(this IZookeeperClient zookeeperClient, string lockerName) 
        {
            var lockTimeoutSection = AppConfig.Configuration.GetSection("LockTimeout");
            ZookeeperLocker locker = null;
            if (lockTimeoutSection.Exists())
            {
                var lockTimeout = lockTimeoutSection.Value.To<int>() * 1000;
                locker = new ZookeeperLocker(zookeeperClient, lockerName, lockTimeout);

            }
            else 
            {
                locker = new ZookeeperLocker(zookeeperClient, lockerName);
            }
            return await locker.Lock();
        }
    }
}
