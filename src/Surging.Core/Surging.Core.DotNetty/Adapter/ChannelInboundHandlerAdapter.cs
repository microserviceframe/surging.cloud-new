using DotNetty.Buffers;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using System;
using System.Text;

namespace Surging.Core.DotNetty.Adapter
{
    public class ChannelInboundHandlerAdapter : ChannelHandlerAdapter
    {
        public ChannelInboundHandlerAdapter() 
        {
        
        }
        public async override void UserEventTriggered(IChannelHandlerContext context, object evt) 
        {
            if (evt is IdleStateEvent)
            {
                var @event = (IdleStateEvent)evt;
                if (@event.State == IdleState.WriterIdle)
                {
                    await context.Channel.WriteAndFlushAsync(Unpooled.WrappedBuffer(Encoding.UTF8.GetBytes(DotNettyConstants.HeartBeatPacket)));
                }

            }
            else
            {
                base.UserEventTriggered(context, evt);
            }
        }

        
    }
}
