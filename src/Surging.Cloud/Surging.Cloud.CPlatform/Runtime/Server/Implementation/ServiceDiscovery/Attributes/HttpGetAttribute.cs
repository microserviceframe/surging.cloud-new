
namespace Surging.Cloud.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes
{
   public class HttpGetAttribute : HttpMethodAttribute
    {
        private static readonly string _supportedMethod = HttpMethod.GET.ToString();

        public HttpGetAttribute()
            : base(_supportedMethod)
        {
        }

        
    }
}
