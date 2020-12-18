using DotNetty.Buffers;
using DotNetty.Common.Utilities;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Address;
using Surging.Core.CPlatform.Runtime.Client.HealthChecks;
using System;
using System.Net;
using System.Text;

namespace Surging.Core.DotNetty.Adapter
{
    public class HeartBeatHandler : ChannelHandlerAdapter
    {
        private IHealthCheckService _healthCheckService;

        public HeartBeatHandler(IHealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        public async override void UserEventTriggered(IChannelHandlerContext context, object evt)
        {
            if (evt is IdleStateEvent)
            {
                var @event = (IdleStateEvent)evt;
                if (@event.State == IdleState.ReaderIdle)
                {

                    var providerServerEndpoint = context.Channel.RemoteAddress as IPEndPoint;
                    var providerServerAddress = new IpAddressModel(providerServerEndpoint.Address.MapToIPv4().ToString(), providerServerEndpoint.Port);
                    var unHealthTimes = await _healthCheckService.MarkFailure(providerServerAddress);
                    if (unHealthTimes > AppConfig.ServerOptions.AllowServerUnhealthyTimes)
                    {
                        await context.Channel.CloseAsync();
                    }
                }

            }
            else 
            {
                base.UserEventTriggered(context, evt);
            }
                
        }
   
    }
}
