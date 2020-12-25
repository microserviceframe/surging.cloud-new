using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Surging.Cloud.CPlatform.Exceptions;
using Surging.Cloud.CPlatform.Messages;
using Surging.Cloud.CPlatform.Transport;
using Surging.Cloud.CPlatform.Transport.Codec;
using Surging.Cloud.CPlatform.Transport.Implementation;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Surging.Cloud.Protocol.Udp
{
   public abstract class DotNettyUdpMessageSender
    {
        private readonly ITransportMessageEncoder _transportMessageEncoder;

        protected DotNettyUdpMessageSender(ITransportMessageEncoder transportMessageEncoder)
        {
            _transportMessageEncoder = transportMessageEncoder;
        }

        protected IByteBuffer GetByteBuffer(TransportMessage message)
        {
            var data =  message.GetContent<byte[]>(); 
            return Unpooled.WrappedBuffer(data);
        }
    }

    /// <summary>
    /// 基于DotNetty服务端的消息发送者。
    /// </summary>
    public class DotNettyUdpServerMessageSender : DotNettyUdpMessageSender, IMessageSender
    {
        private readonly IChannelHandlerContext _context;

        public event EventHandler<EndPoint> OnChannelUnActived;

        public DotNettyUdpServerMessageSender(ITransportMessageEncoder transportMessageEncoder, IChannelHandlerContext context) : base(transportMessageEncoder)
        {
            _context = context;
        }

        #region Implementation of IMessageSender

        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAsync(TransportMessage message)
        {
            if (!_context.Channel.Active)
            {
                if (OnChannelUnActived != null)
                {
                    OnChannelUnActived(this, _context.Channel.RemoteAddress);
                }
                throw new CommunicationException($"{_context.Channel.RemoteAddress}服务提供者不健康,无法发送消息");
            }
            var buffer = GetByteBuffer(message);
            await _context.WriteAsync(buffer);
        }

        /// <summary>
        /// 发送消息并清空缓冲区。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAndFlushAsync(TransportMessage message)
        {
            if (!_context.Channel.Active)
            {
                if (OnChannelUnActived != null)
                {
                    OnChannelUnActived(this, _context.Channel.RemoteAddress);
                }
                throw new CommunicationException($"{_context.Channel.RemoteAddress}服务提供者不健康,无法发送消息");
            }

            var buffer = GetByteBuffer(message);
            if( _context.Channel.RemoteAddress !=null)
            await _context.WriteAndFlushAsync(buffer);
            //RpcContext.GetContext().ClearAttachment();
        }

        #endregion Implementation of IMessageSender
    }
}
