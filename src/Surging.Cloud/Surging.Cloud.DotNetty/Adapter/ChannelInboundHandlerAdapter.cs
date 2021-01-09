using System.Net;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using Surging.Cloud.CPlatform.Address;
using Surging.Cloud.CPlatform.Runtime.Client.HealthChecks;
using Surging.Cloud.CPlatform.Utilities;

namespace Surging.Cloud.DotNetty.Adapter
{
    public class ChannelInboundHandlerAdapter : ChannelHandlerAdapter
    {

        private readonly IHealthCheckService _healthCheckService;

        public ChannelInboundHandlerAdapter()
        {
            _healthCheckService = ServiceLocator.GetService<IHealthCheckService>();
        }

        public async override void UserEventTriggered(IChannelHandlerContext context, object evt) 
        {
            if (evt is IdleStateEvent @event)
            {
                if (@event.State == IdleState.ReaderIdle)
                {
                    var iPEndPoint = context.Channel.RemoteAddress as IPEndPoint;
                    var ipAddressModel = new IpAddressModel(iPEndPoint.Address.MapToIPv4().ToString(),
                        iPEndPoint.Port);
                    await _healthCheckService.MarkHealth(ipAddressModel);
                    if (context.Channel.Active)
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
