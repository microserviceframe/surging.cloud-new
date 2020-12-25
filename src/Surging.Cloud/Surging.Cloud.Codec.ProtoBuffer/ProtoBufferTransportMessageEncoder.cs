using Surging.Cloud.Codec.ProtoBuffer.Messages;
using Surging.Cloud.Codec.ProtoBuffer.Utilities;
using Surging.Cloud.CPlatform.Messages;
using Surging.Cloud.CPlatform.Transport.Codec;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.Codec.ProtoBuffer
{
    public sealed class ProtoBufferTransportMessageEncoder : ITransportMessageEncoder
    {
        #region Implementation of ITransportMessageEncoder

        public byte[] Encode(TransportMessage message)
        {
            var transportMessage = new ProtoBufferTransportMessage(message)
            {
                Id = message.Id,
                ContentType = message.ContentType,
            };

            return SerializerUtilitys.Serialize(transportMessage);
        }

        #endregion Implementation of ITransportMessageEncoder
    }
}