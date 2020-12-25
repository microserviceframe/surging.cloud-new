

namespace Surging.Cloud.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes
{
   public class HttpHeadAttribute : HttpMethodAttribute
    {
        private static readonly string _supportedMethod = HttpMethod.HEAD.ToString();

        public HttpHeadAttribute()
            : base(_supportedMethod)
        {
        }

        
    }
}
