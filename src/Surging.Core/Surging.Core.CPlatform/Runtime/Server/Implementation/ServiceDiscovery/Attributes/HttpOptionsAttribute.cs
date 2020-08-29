

namespace Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes
{
    public class HttpOptionsAttribute : HttpMethodAttribute
    {
        private static readonly string _supportedMethod = HttpMethod.OPTION.ToString();

        public HttpOptionsAttribute()
            : base(_supportedMethod)
        {
        }

       
    }
}
