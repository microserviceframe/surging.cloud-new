using Microsoft.Extensions.Logging;
using Surging.Cloud.CPlatform;
using Surging.Cloud.CPlatform.Module;
using Surging.Cloud.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using Autofac;

namespace Surging.Cloud.Nlog
{
    public class NLogModule : EnginePartModule
    {
        private string nlogConfigFile = "${LogPath}|NLog.config";
        private bool isProvider = false;
        public override void Initialize(AppModuleContext context)
        {
            if (!isProvider)
            {
                var serviceProvider = context.ServiceProvoider;
                base.Initialize(context);
                var section = AppConfig.GetSection("Logging");
                nlogConfigFile = EnvironmentHelper.GetEnvironmentVariable(nlogConfigFile);
                NLog.LogManager.LoadConfiguration(nlogConfigFile);
                serviceProvider.Resolve<ILoggerFactory>().AddProvider(new NLogProvider());
                isProvider = true;
            }
           
        }

        /// <summary>
        /// Inject dependent third-party components
        /// </summary>
        /// <param name="builder"></param>
        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            base.RegisterBuilder(builder);
           
        }
    }
}
