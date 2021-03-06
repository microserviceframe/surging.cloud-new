﻿using DotNetty.Buffers;
using DotNetty.Common.Utilities;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using Surging.Cloud.CPlatform;
using Surging.Cloud.CPlatform.Address;
using Surging.Cloud.CPlatform.Runtime.Client.HealthChecks;
using System;
using System.Net;
using System.Text;

namespace Surging.Cloud.DotNetty.Adapter
{
    public class HeartBeatHandler : ChannelHandlerAdapter
    {
        private IHealthCheckService _healthCheckService;
        private DotNettyTransportClientFactory _transportClientFactory;

        public HeartBeatHandler(IHealthCheckService healthCheckService, DotNettyTransportClientFactory transportClientFactory)
        {
            _healthCheckService = healthCheckService;
            _transportClientFactory = transportClientFactory;
        }

        public async override void UserEventTriggered(IChannelHandlerContext context, object evt)
        {
            if (evt is IdleStateEvent @event)
            {
                if (@event.State == IdleState.WriterIdle)
                {
                    await context.Channel.WriteAndFlushAsync(
                        Unpooled.WrappedBuffer(Encoding.UTF8.GetBytes(DotNettyConstants.HeartBeatPacket)));
                    
                }
            }
            else 
            {
                base.UserEventTriggered(context, evt);
            }
                
        }
   
    }
}
