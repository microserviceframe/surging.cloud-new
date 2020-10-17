using Microsoft.Extensions.Configuration;
using Surging.Core.CPlatform.Exceptions;
using Surging.Core.CPlatform.Module;
using Surging.Core.Lock.Configurations;
using Surging.Core.Lock.Provider;
using SurgingAppConfig = Surging.Core.CPlatform.AppConfig;
namespace Surging.Core.Lock
{
    public class LockModule : EnginePartModule
    {

        public override void Initialize(AppModuleContext context)
        {
            base.Initialize(context);
            LockOptions lockOption = null;

            if (AppConfig.Configuration != null)
            {
                lockOption = AppConfig.Configuration.Get<LockOptions>();
            }
            else 
            {
                var lockSection = SurgingAppConfig.Configuration.GetSection("Lock");
                if (lockSection.Exists()) 
                {
                    lockOption = lockSection.Get<LockOptions>();
                }
            }
            if (lockOption == null) 
            {
                throw new CPlatformException("请添加分布式锁服务设置");
            }
            AppConfig.LockOption = lockOption;
            
        }

        protected override void RegisterBuilder(ContainerBuilderWrapper builder) 
        {
            base.RegisterBuilder(builder);
            builder.RegisterType(typeof(LockerProvider)).As(typeof(ILockerProvider)).SingleInstance();
        }
    }
}
