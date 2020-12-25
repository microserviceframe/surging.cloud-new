using Surging.Cloud.CPlatform.Messages;

namespace Surging.Cloud.CPlatform.Transport.Codec
{
    public interface ITransportMessageDecoder
    {
        TransportMessage Decode(byte[] data);
    }
}