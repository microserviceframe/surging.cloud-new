using Surging.Cloud.CPlatform;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.Codec.ProtoBuffer
{
   public static class ContainerBuilderExtensions
    {
        public static IServiceBuilder UseProtoBufferCodec(this IServiceBuilder builder)
        {
            return builder.UseCodec<ProtoBufferTransportMessageCodecFactory>();
        }
    }
}
