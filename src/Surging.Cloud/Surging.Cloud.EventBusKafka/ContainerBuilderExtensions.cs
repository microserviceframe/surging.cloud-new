using Autofac;
using Microsoft.Extensions.Logging;
using Surging.Cloud.CPlatform;
using Surging.Cloud.CPlatform.EventBus;
using Surging.Cloud.CPlatform.EventBus.Implementation;
using Surging.Cloud.EventBusKafka.Implementation;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Surging.Cloud.CPlatform.EventBus.Events;
using Microsoft.Extensions.Options;
using Surging.Cloud.EventBusKafka.Configurations;
using Microsoft.Extensions.Configuration;

namespace Surging.Cloud.EventBusKafka
{
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// 使用KafkaMQ进行传输。
        /// </summary>
        /// <param name="builder">服务构建者。</param>
        /// <param name="options"></param>
        /// <returns>服务构建者。</returns>
        public static IServiceBuilder UseKafkaMQTransport(this IServiceBuilder builder,Action<KafkaOptions> options)
        {
            AppConfig.Options = new KafkaOptions();
            var section = CPlatform.AppConfig.GetSection("Kafka");
            if (section.Exists())
                AppConfig.Options = section.Get<KafkaOptions>();
            else if (AppConfig.Configuration != null)
                AppConfig.Options = AppConfig.Configuration.Get<KafkaOptions>();
            options.Invoke(AppConfig.Options);
            AppConfig.KafkaConsumerConfig = AppConfig.Options.GetConsumerConfig();
            var services = builder.Services;
            builder.Services.RegisterType(typeof(Implementation.EventBusKafka)).As(typeof(IEventBus)).SingleInstance();
            builder.Services.RegisterType(typeof(DefaultConsumeConfigurator)).As(typeof(IConsumeConfigurator)).SingleInstance();
            builder.Services.RegisterType(typeof(InMemoryEventBusSubscriptionsManager)).As(typeof(IEventBusSubscriptionsManager)).SingleInstance();
            builder.Services.RegisterType(typeof(KafkaProducerPersistentConnection))
           .Named(KafkaConnectionType.Producer.ToString(), typeof(IKafkaPersisterConnection)).SingleInstance();
            builder.Services.RegisterType(typeof(KafkaConsumerPersistentConnection))
            .Named(KafkaConnectionType.Consumer.ToString(), typeof(IKafkaPersisterConnection)).SingleInstance();
            return builder;
        }

        public static IServiceBuilder UseKafkaMQEventAdapt(this IServiceBuilder builder, Func<IServiceProvider, ISubscriptionAdapt> adapt)
        {
            var services = builder.Services;
            services.RegisterAdapter(adapt);
            return builder;
        }

        public static IServiceBuilder AddKafkaMQAdapt(this IServiceBuilder builder)
        {
            return builder.UseKafkaMQEventAdapt(provider =>
             new KafkaSubscriptionAdapt(
                 provider.GetService<IConsumeConfigurator>(),
                 provider.GetService<IEnumerable<IIntegrationEventHandler>>()
                 )
            );
        }
    }
}

