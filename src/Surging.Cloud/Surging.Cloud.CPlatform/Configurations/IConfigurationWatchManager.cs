using Surging.Cloud.CPlatform.Configurations.Watch;

namespace Surging.Cloud.CPlatform.Configurations
{
    public  interface IConfigurationWatchManager
    {
        void Register(ConfigurationWatch watch);
    }
}
