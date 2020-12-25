using Surging.Cloud.CPlatform.EventBus;
using Surging.Cloud.ServiceHosting.Internal;
using Autofac;
using Surging.Cloud.CPlatform.Engines;
using Surging.Cloud.CPlatform;
using Surging.Cloud.CPlatform.Routing;
using Surging.Cloud.CPlatform.EventBus.Implementation;

namespace Surging.Cloud.EventBusRabbitMQ
{
    public static class ServiceHostBuilderExtensions
    {
        public static IServiceHostBuilder SubscribeAt(this IServiceHostBuilder hostBuilder)
        {
            return hostBuilder.MapServices(mapper =>
            {
                mapper.Resolve<IServiceEngineLifetime>().ServiceEngineStarted.Register(() =>
                {
                      mapper.Resolve<ISubscriptionAdapt>().SubscribeAt();
                    new ServiceRouteWatch(mapper.Resolve<CPlatformContainer>(), () =>
                    {
                        var subscriptionAdapt = mapper.Resolve<ISubscriptionAdapt>();
                        mapper.Resolve<IEventBus>().OnShutdown += (sender, args) =>
                        {
                            subscriptionAdapt.Unsubscribe();
                        };
                        mapper.Resolve<ISubscriptionAdapt>().SubscribeAt();
                    });
                });
            });
        }
    }
}
