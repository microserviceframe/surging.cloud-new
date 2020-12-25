using Surging.Cloud.Codec.MessagePack.Messages;
using Surging.Cloud.Codec.MessagePack.Utilities;
using Surging.Cloud.CPlatform.Messages;
using Surging.Cloud.CPlatform.Transport.Codec;
using System.Runtime.CompilerServices;

namespace Surging.Cloud.Codec.MessagePack
{
   public sealed class MessagePackTransportMessageEncoder:ITransportMessageEncoder
    {
        #region Implementation of ITransportMessageEncoder

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] Encode(TransportMessage message)
        {
            var transportMessage = new MessagePackTransportMessage(message)
            {
                Id = message.Id,
                ContentType = message.ContentType,
            };
            return SerializerUtilitys.Serialize(transportMessage);
        }
        #endregion Implementation of ITransportMessageEncoder
    }
}
