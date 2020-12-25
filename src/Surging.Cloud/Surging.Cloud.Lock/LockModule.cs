using Microsoft.Extensions.Configuration;
using Surging.Cloud.CPlatform.Exceptions;
using Surging.Cloud.CPlatform.Module;
using Surging.Cloud.Lock.Configurations;
using Surging.Cloud.Lock.Provider;
using SurgingAppConfig = Surging.Cloud.CPlatform.AppConfig;
namespace Surging.Cloud.Lock
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
