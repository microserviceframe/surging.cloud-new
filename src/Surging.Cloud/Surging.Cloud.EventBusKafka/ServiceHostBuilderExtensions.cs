using Autofac;
using Surging.Cloud.CPlatform.EventBus;
using Surging.Cloud.ServiceHosting.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.EventBusKafka
{
   public static  class ServiceHostBuilderExtensions
    {
        public static IServiceHostBuilder SubscribeAt(this IServiceHostBuilder hostBuilder)
        {
            return hostBuilder.MapServices(mapper =>
            {
                mapper.Resolve<ISubscriptionAdapt>().SubscribeAt();
            });
        }
    }
}
