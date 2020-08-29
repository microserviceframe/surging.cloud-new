

namespace Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes
{
    public class HttpPatchAttribute : HttpMethodAttribute
    {
        private static readonly string _supportedMethod = HttpMethod.PATCH.ToString();

        public HttpPatchAttribute()
            : base(_supportedMethod)
        {
        }

       
    }
}
