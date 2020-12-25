using Surging.Cloud.CPlatform;
using Surging.Cloud.CPlatform.Module;
using Surging.Cloud.CPlatform.Utilities;
using Surging.Cloud.System.MongoProvider.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.System.MongoProvider
{
    public class MongoModule : SystemModule
    {
        /// <summary>
        ///  Function module initialization,trigger when the module starts loading
        /// </summary>
        public override void Initialize(AppModuleContext context)
        {
            base.Initialize(context);
        }

        /// <summary>
        /// Inject dependent third-party components
        /// </summary>
        /// <param name="builder"></param>
        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            base.RegisterBuilder(builder);
            builder.RegisterGeneric(typeof(MongoRepository<>)).As(typeof(IMongoRepository<>)).SingleInstance();
        }
    }
}
