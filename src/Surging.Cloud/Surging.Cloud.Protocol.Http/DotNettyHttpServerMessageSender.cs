using DotNetty.Buffers;
using DotNetty.Codecs.Http;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using Surging.Cloud.CPlatform.Exceptions;
using Surging.Cloud.CPlatform.Messages;
using Surging.Cloud.CPlatform.Serialization;
using Surging.Cloud.CPlatform.Transport;
using Surging.Cloud.CPlatform.Transport.Codec;
using Surging.Cloud.CPlatform.Transport.Implementation;
using Surging.Cloud.DotNetty;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Cloud.Protocol.Http
{
    public class DotNettyHttpServerMessageSender: DotNettyMessageSender, IMessageSender
    {
        private readonly IChannelHandlerContext _context;
        private readonly ISerializer<string> _serializer;
        private readonly AsciiString ContentTypeEntity = HttpHeaderNames.ContentType;
        private readonly AsciiString ServerEntity = HttpHeaderNames.Server;
        private readonly AsciiString ContentLengthEntity = HttpHeaderNames.ContentLength;
        private readonly AsciiString TypeJson = AsciiString.Cached("application/json");

        public event EventHandler<System.Net.EndPoint> OnChannelUnActived;

        public DotNettyHttpServerMessageSender(ITransportMessageEncoder transportMessageEncoder, IChannelHandlerContext context, ISerializer<string> serializer) : base(transportMessageEncoder)
        {
            _context = context;
            _serializer = serializer;
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
            var buffer = GetByteBuffer(message, out int contentLength);
            var response = WriteResponse(_context, buffer, TypeJson, AsciiString.Cached($"{contentLength}"));
            await _context.WriteAsync(response);
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

            var buffer = GetByteBuffer(message, out int contentLength);
            var response = WriteResponse(_context, buffer, TypeJson, AsciiString.Cached($"{ contentLength}"));

            await _context.Channel.WriteAndFlushAsync(response);
            await _context.Channel.CloseAsync();
            //RpcContext.GetContext().ClearAttachment();
        }

        private IByteBuffer GetByteBuffer(TransportMessage message, out int contentLength)
        {
            contentLength = 0;
            if (!message.IsHttpResultMessage())
                return null;

            var httpResultMessage = message.GetContent<HttpResultMessage>();
            var data = Encoding.UTF8.GetBytes(_serializer.Serialize(httpResultMessage));
            contentLength = data.Length;
            return Unpooled.WrappedBuffer(data);
        }

        private DefaultFullHttpResponse WriteResponse(IChannelHandlerContext ctx, IByteBuffer buf, ICharSequence contentType, ICharSequence contentLength)
        {
            var response = new DefaultFullHttpResponse(HttpVersion.Http11, HttpResponseStatus.OK, buf, false);
            HttpHeaders headers = response.Headers;
            headers.Set(ContentTypeEntity, contentType);
            headers.Set(ContentLengthEntity, contentLength);
            return response;
        }
        #endregion Implementation of IMessageSender
    }
}

