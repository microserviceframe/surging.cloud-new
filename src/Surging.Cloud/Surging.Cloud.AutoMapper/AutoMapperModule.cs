using Autofac;
using Microsoft.Extensions.Configuration;
using Surging.Cloud.CPlatform.Module;
using CPlatformAppConfig = Surging.Cloud.CPlatform.AppConfig;

namespace Surging.Cloud.AutoMapper
{
    public class AutoMapperModule : EnginePartModule
    {

        public override void Initialize(AppModuleContext context)
        {
            base.Initialize(context);
            context.ServiceProvoider.Resolve<IAutoMapperBootstrap>().Initialize();
        }

        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            var configAssembliesStr = CPlatformAppConfig.GetSection("Automapper:Assemblies").Get<string>();
            if (!string.IsNullOrEmpty(configAssembliesStr))
            {
                AppConfig.AssembliesStrings = configAssembliesStr.Split(";");
            }
            builder.RegisterType<AutoMapperBootstrap>().As<IAutoMapperBootstrap>();
        }


    }
}
