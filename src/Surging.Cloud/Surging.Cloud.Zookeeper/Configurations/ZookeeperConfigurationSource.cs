using Microsoft.Extensions.Configuration;
 

namespace Surging.Cloud.Zookeeper.Configurations
{
    public class ZookeeperConfigurationSource : FileConfigurationSource
    {
        public string ConfigurationKeyPrefix { get; set; }

        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            FileProvider = FileProvider ?? builder.GetFileProvider();
            return new ZookeeperConfigurationProvider(this);
        }
    }
}