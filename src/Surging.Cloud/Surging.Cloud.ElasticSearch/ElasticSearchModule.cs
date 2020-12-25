using Autofac;
using Microsoft.Extensions.Configuration;
using Surging.Cloud.CPlatform;
using Surging.Cloud.CPlatform.Exceptions;
using Surging.Cloud.CPlatform.Module;
using Surging.Cloud.CPlatform.Utilities;
using Surging.Cloud.ElasticSearch.Provider;

namespace Surging.Cloud.ElasticSearch
{
    public class ElasticSearchModule : EnginePartModule
    {
        protected override void RegisterBuilder(ContainerBuilderWrapper builder) 
        {
            var esSettingSection = AppConfig.GetSection("ElasticSearch");
            if (!esSettingSection.Exists())
            {
                throw new DataAccessException("未对ElasticSearch服务进行配置");
            }

            var esSetting = AppConfig.Configuration.GetSection("ElasticSearch").Get<ElasticSearchSetting>();
            ElasticSearchSetting.Instance = esSetting;

            builder.RegisterType<ElasticSearchProvider>().As<IElasticSearchProvider>().AsSelf().InstancePerDependency();
        }
    }
}
