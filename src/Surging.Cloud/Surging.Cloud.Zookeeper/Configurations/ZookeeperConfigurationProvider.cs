using Microsoft.Extensions.Configuration;
using Surging.Cloud.CPlatform.Configurations.Remote;
using System.IO;

namespace Surging.Cloud.Zookeeper.Configurations
{
    public class ZookeeperConfigurationProvider : FileConfigurationProvider
    {
        public ZookeeperConfigurationProvider(ZookeeperConfigurationSource source) : base(source) { }

        public override void Load(Stream stream)
        {
            var parser = new JsonConfigurationParser();
            this.Data = parser.Parse(stream, null);
        }
    }
}
