using Surging.Cloud.Codec.MessagePack.Messages;
using Surging.Cloud.Codec.MessagePack.Utilities;
using Surging.Cloud.CPlatform.Messages;
using Surging.Cloud.CPlatform.Transport.Codec;
using System.Runtime.CompilerServices;

namespace Surging.Cloud.Codec.MessagePack
{
    public sealed class MessagePackTransportMessageDecoder : ITransportMessageDecoder
    {
        #region Implementation of ITransportMessageDecoder

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TransportMessage Decode(byte[] data)
        {
            var message = SerializerUtilitys.Deserialize<MessagePackTransportMessage>(data);
            return message.GetTransportMessage();
        }

        #endregion Implementation of ITransportMessageDecoder
    }
}
