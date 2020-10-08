using org.apache.zookeeper;
using Rabbit.Zookeeper;
using Surging.Core.CPlatform.Exceptions;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static org.apache.zookeeper.Watcher;

namespace Surging.Core.Zookeeper
{
    public class ZookeeperLocker : IDisposable
    {
        private readonly IZookeeperClient _zookeeperClient;
        private readonly ManualResetEvent _event = new ManualResetEvent(false);
        private readonly string _lockName;
        private string _currentNode;

        private readonly int _lockTimeout;
        public ZookeeperLocker(IZookeeperClient zookeeperClient, string lockerName, int lockTimeout)
        {
            _zookeeperClient = zookeeperClient;
            _lockName = lockerName;
            _lockTimeout = lockTimeout;
            CreateLockNode();
        }

        public ZookeeperLocker(IZookeeperClient zookeeperClient, string lockerName) : this(zookeeperClient, lockerName, 5000)
        {

        }

        public ZookeeperLocker(IZookeeperClient zookeeperClient) : this(zookeeperClient, "defaultLock")
        {

        }

        /// <summary>
        /// lock
        /// </summary>
        public async Task<ZookeeperLocker> Lock()
        {
            _currentNode = await _zookeeperClient.CreateAsync($"/locks/{_lockName}/node", new byte[0],
                ZooDefs.Ids.OPEN_ACL_UNSAFE,
                CreateMode.EPHEMERAL_SEQUENTIAL);


            var result = WaitHandle.WaitAny(new WaitHandle[] { _event }, TimeSpan.FromMilliseconds(_lockTimeout));
            if (result == WaitHandle.WaitTimeout || result == 1)
            {
                await UnLock();
                throw new LockerTimeoutException($"分布式锁{_lockName}超时");
            }
            return this;

            //if (await ExistPreNodeExecuted())
            //{
            //    //var result = WaitHandle.WaitAny(new WaitHandle[] { _event }, TimeSpan.FromMilliseconds(_lockTimeout));
            //    if (!_event.WaitOne(_lockTimeout))
            //    {
            //        await UnLock();
            //        throw new LockerTimeoutException($"获取分布式锁{_lockName}超时");
            //    }
            //}
            
        }

        private async Task UnLock()
        {
            await _zookeeperClient.DeleteAsync(_currentNode);
            _event.Dispose();
        }

        private async Task<bool> ExistPreNodeExecuted()
        {
            var children = (await _zookeeperClient.GetChildrenAsync($"/locks/{_lockName}")).OrderBy(item => int.Parse(Regex.Replace(item, @"[a-zA-Z|_]", "0"))).ToList();

            var currentIndex = children.IndexOf(_currentNode.Replace($"/locks/{_lockName}/", ""));
            if (currentIndex <= 0)
            {
                return false;
            }
            var preNode = await _zookeeperClient.ExistsAsync($"/locks/{_lockName}/" + children[currentIndex - 1]);
            if (!preNode)
            {
                return await ExistPreNodeExecuted();
            }

            return true;
        }

        public void Dispose()
        {
            UnLock().Wait();
        }


        private void CreateLockNode()
        {
            if (!_zookeeperClient.ExistsAsync("/locks").Result)
            {
                _zookeeperClient.CreateAsync("/locks", new byte[0], ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT).Wait();
            }
            if (!_zookeeperClient.ExistsAsync("/locks/" + _lockName).Result)
            {
                _zookeeperClient.CreateAsync("/locks/" + _lockName, new byte[0], ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT).Wait();
            }
            _zookeeperClient.SubscribeChildrenChange("/locks/" + _lockName, async (client, args) =>
            {
                if (args.Type == Event.EventType.NodeDeleted)
                {
                    if (!await ExistPreNodeExecuted())
                    {
                        if (!_event.SafeWaitHandle.IsClosed)
                        {
                            _event.Set();
                        }
                    }
                }

            });
        }
    }
}
