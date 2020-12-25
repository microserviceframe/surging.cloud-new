using Surging.Cloud.CPlatform.Messages;

namespace Surging.Cloud.CPlatform.Transport.Codec
{
    public interface ITransportMessageEncoder
    {
        byte[] Encode(TransportMessage message);
    }
}