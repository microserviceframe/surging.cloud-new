using Microsoft.Extensions.Configuration;
using Surging.Cloud.CPlatform.Configurations.Remote;
using System.IO;

namespace Surging.Cloud.CPlatform.Configurations
{
    public class CPlatformConfigurationProvider : FileConfigurationProvider
    {

        public CPlatformConfigurationProvider(CPlatformConfigurationSource source) : base(source) { }

        public override void Load(Stream stream)
        {
            var parser = new JsonConfigurationParser();
            this.Data = parser.Parse(stream, null);
        }
    }
}
