using Rabbit.Zookeeper;
using System;
using System.Threading.Tasks;

namespace Surging.Cloud.Zookeeper
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

    }
}
