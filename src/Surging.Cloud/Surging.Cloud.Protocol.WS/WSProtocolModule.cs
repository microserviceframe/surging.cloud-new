﻿using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Surging.Cloud.CPlatform;
using Surging.Cloud.CPlatform.Module;
using Surging.Cloud.CPlatform.Runtime.Server;
using Surging.Cloud.CPlatform.Runtime.Server.Implementation;
using Surging.Cloud.CPlatform.Serialization;
using Surging.Cloud.CPlatform.Transport.Codec;
using Surging.Cloud.Protocol.WS.Configurations;
using Surging.Cloud.Protocol.WS.Runtime;
using Surging.Cloud.Protocol.WS.Runtime.Implementation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.Protocol.WS
{
    public class WSProtocolModule : EnginePartModule
    {
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
            var options = new WebSocketOptions();
            var section = AppConfig.GetSection("WebSocket");
            if (section.Exists())
                options = section.Get<WebSocketOptions>();
            base.RegisterBuilder(builder);
            builder.Register(provider =>
            {
                return new DefaultWSServiceEntryProvider(
                       provider.Resolve<IServiceEntryProvider>(),
                    provider.Resolve<ILogger<DefaultWSServiceEntryProvider>>(),
                      provider.Resolve<CPlatformContainer>(),
                      options
                      );
            }).As(typeof(IWSServiceEntryProvider)).SingleInstance();
            if (AppConfig.ServerOptions.Protocol == CommunicationProtocol.WS)
            {
                RegisterDefaultProtocol(builder, options);
            }
            else if (AppConfig.ServerOptions.Protocol == CommunicationProtocol.None)
            {
                RegisterWSProtocol(builder, options);
            }
        }

        private static void RegisterDefaultProtocol(ContainerBuilderWrapper builder, WebSocketOptions options)
        {

            builder.Register(provider =>
            {
                return new DefaultWSServerMessageListener(
                    provider.Resolve<ILogger<DefaultWSServerMessageListener>>(),
                      provider.Resolve<IWSServiceEntryProvider>(),
                      options
                      );
            }).SingleInstance();
            builder.Register(provider =>
            {
                var messageListener = provider.Resolve<DefaultWSServerMessageListener>();
                return new DefaultServiceHost(async endPoint =>
                {
                    await messageListener.StartAsync(endPoint);
                    return messageListener;
                }, null);

            }).As<IServiceHost>();
        }

        private static void RegisterWSProtocol(ContainerBuilderWrapper builder, WebSocketOptions options)
        {
            builder.Register(provider =>
            {
                return new DefaultWSServerMessageListener(provider.Resolve<ILogger<DefaultWSServerMessageListener>>(),
                      provider.Resolve<IWSServiceEntryProvider>(),
                      options
                      );
            }).SingleInstance();
            builder.Register(provider =>
            {
                var messageListener = provider.Resolve<DefaultWSServerMessageListener>();
                return new WSServiceHost(async endPoint =>
                {
                    await messageListener.StartAsync(endPoint);
                    return messageListener;
                });

            }).As<IServiceHost>();
        }
    }
}
