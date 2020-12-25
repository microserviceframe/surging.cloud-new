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

namespace Surging.Cloud.DotNetty
{
    /// <summary>
    /// 基于DotNetty的消息发送者基类。
    /// </summary>
    public abstract class DotNettyMessageSender
    {
        private readonly ITransportMessageEncoder _transportMessageEncoder;

        protected DotNettyMessageSender(ITransportMessageEncoder transportMessageEncoder)
        {
            _transportMessageEncoder = transportMessageEncoder;
        }

        protected IByteBuffer GetByteBuffer(TransportMessage message)
        {
            var data = _transportMessageEncoder.Encode(message);
            //var buffer = PooledByteBufferAllocator.Default.Buffer();
            return Unpooled.WrappedBuffer(data);
        }
    }

    /// <summary>
    /// 基于DotNetty客户端的消息发送者。
    /// </summary>
    public class DotNettyMessageClientSender : DotNettyMessageSender, IMessageSender, IDisposable
    {
        private readonly IChannel _channel;

        public event EventHandler<EndPoint> OnChannelUnActived;

        public DotNettyMessageClientSender(ITransportMessageEncoder transportMessageEncoder, IChannel channel) : base(transportMessageEncoder)
        {
            _channel = channel;
        }

        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Task.Run(async () =>
            {
                await _channel.DisconnectAsync();
            }).Wait();
        }

        #endregion Implementation of IDisposable

        #region Implementation of IMessageSender

        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAsync(TransportMessage message)
        {
            if(!_channel.Active)
            {
                if (OnChannelUnActived != null)
                {
                    OnChannelUnActived(this, _channel.RemoteAddress);
                }
                throw new CommunicationException($"{_channel.RemoteAddress}服务提供者不健康,无法发送消息");
            }
            var buffer = GetByteBuffer(message);
            await _channel.WriteAndFlushAsync(buffer);
        }

        /// <summary>
        /// 发送消息并清空缓冲区。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAndFlushAsync(TransportMessage message)
        {
            if (!_channel.Active)
            {
                if (OnChannelUnActived != null)
                {
                    OnChannelUnActived(this, _channel.RemoteAddress);
                }
                throw new CommunicationException($"{_channel.RemoteAddress}服务提供者不健康,无法发送消息");
            }
            var buffer = GetByteBuffer(message);
            await _channel.WriteAndFlushAsync(buffer);

            //RpcContext.GetContext().ClearAttachment();
        }

        #endregion Implementation of IMessageSender
    }

    /// <summary>
    /// 基于DotNetty服务端的消息发送者。
    /// </summary>
    public class DotNettyServerMessageSender : DotNettyMessageSender, IMessageSender
    {
        private readonly IChannelHandlerContext _context;

        public event EventHandler<EndPoint> OnChannelUnActived;

        public DotNettyServerMessageSender(ITransportMessageEncoder transportMessageEncoder, IChannelHandlerContext context) : base(transportMessageEncoder)
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
            await _context.Channel.WriteAndFlushAsync(buffer);
            //RpcContext.GetContext().ClearAttachment();
        }

        #endregion Implementation of IMessageSender
    }
}