using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;

namespace Surging.Core.DotNetty.Adapter
{
    public class ChannelInboundHandlerAdapter : ChannelHandlerAdapter
    {
        public ChannelInboundHandlerAdapter() 
        {
        
        }
        public async override void UserEventTriggered(IChannelHandlerContext context, object evt) 
        {
            if (evt is IdleStateEvent @event)
            {
                if (@event.State == IdleState.ReaderIdle)
                {
                    await context.Channel.CloseAsync();
                }

            }
            else
            {
                base.UserEventTriggered(context, evt);
            }
        }

        
    }
}
