using Microsoft.Extensions.Configuration;

namespace Surging.Cloud.CPlatform.Configurations
{
    public class CPlatformConfigurationSource : FileConfigurationSource
    {
        public string ConfigurationKeyPrefix { get; set; }

        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            FileProvider = FileProvider ?? builder.GetFileProvider();
            return new CPlatformConfigurationProvider(this);
        }
    }
}