using Autofac;
using Surging.Cloud.CPlatform;
using Surging.Cloud.CPlatform.Module;
using Surging.Cloud.CPlatform.Utilities;
using Microsoft.Extensions.Logging;

namespace Surging.Cloud.Log4net
{
   public class Log4netModule : EnginePartModule
    {
        private string log4NetConfigFile = "${LogPath}|log4net.config";
        private bool isAddProvider = false;

        public override void Initialize(AppModuleContext context)
        {
            if (!isAddProvider)
            {
                var serviceProvider = context.ServiceProvoider;
                base.Initialize(context);
                var section = CPlatform.AppConfig.GetSection("Logging");
                log4NetConfigFile = EnvironmentHelper.GetEnvironmentVariable(log4NetConfigFile);
                serviceProvider.Resolve<ILoggerFactory>().AddProvider(new Log4NetProvider(log4NetConfigFile));
                isAddProvider = true;
            }
        }
    }
}
