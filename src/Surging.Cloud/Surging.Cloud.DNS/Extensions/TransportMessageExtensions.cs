using Surging.Cloud.CPlatform.Messages;
using System.Runtime.CompilerServices;


namespace Surging.Cloud.DNS.Extensions
{
    public static class TransportMessageExtensions
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDnsResultMessage(this TransportMessage message)
        {
            return message.ContentType == typeof(DnsTransportMessage).FullName;
        }
    }
}
