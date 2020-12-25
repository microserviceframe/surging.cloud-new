using Surging.Cloud.Codec.ProtoBuffer.Messages;
using Surging.Cloud.Codec.ProtoBuffer.Utilities;
using Surging.Cloud.CPlatform.Messages;
using Surging.Cloud.CPlatform.Transport.Codec;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.Codec.ProtoBuffer
{
   public sealed class ProtoBufferTransportMessageDecoder : ITransportMessageDecoder
    {
        #region Implementation of ITransportMessageDecoder

        public TransportMessage Decode(byte[] data)
        {
            var message = SerializerUtilitys.Deserialize<ProtoBufferTransportMessage>(data);
            return message.GetTransportMessage();
        }

        #endregion Implementation of ITransportMessageDecoder
    }
} 