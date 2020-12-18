using DotNetty.Buffers;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using Surging.Core.CPlatform.Transport.Codec;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.DotNetty.Adapter
{
    class TransportMessageChannelHandlerAdapter : ChannelHandlerAdapter
    {
        private readonly ITransportMessageDecoder _transportMessageDecoder;

        public TransportMessageChannelHandlerAdapter(ITransportMessageDecoder transportMessageDecoder)
        {
            _transportMessageDecoder = transportMessageDecoder;
        }

        #region Overrides of ChannelHandlerAdapter

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var buffer = (IByteBuffer)message;
            var data = new byte[buffer.ReadableBytes];
            buffer.ReadBytes(data);
            if (!DataEquals(data, Encoding.UTF8.GetBytes(DotNettyConstants.HeartBeatPacket))) 
            {
                var transportMessage = _transportMessageDecoder.Decode(data);
                context.FireChannelRead(transportMessage);
            }           
            ReferenceCountUtil.Release(buffer);
            //ReferenceCountUtil.Release(buffer);

        }

        #endregion Overrides of ChannelHandlerAdapter

        private static bool DataEquals(byte[] data1, byte[] data2)
        {
            if (data1.Length != data2.Length)
                return false;
            for (var i = 0; i < data1.Length; i++)
            {
                var b1 = data1[i];
                var b2 = data2[i];
                if (b1 != b2)
                    return false;
            }
            return true;
        }
    }
}