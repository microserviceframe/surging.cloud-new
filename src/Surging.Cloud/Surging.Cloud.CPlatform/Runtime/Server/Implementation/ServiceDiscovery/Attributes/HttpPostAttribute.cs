
namespace Surging.Cloud.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes
{
    public class HttpPostAttribute : HttpMethodAttribute
    {
        private static readonly string _supportedMethod = HttpMethod.POST.ToString();

        public HttpPostAttribute()
            : base(_supportedMethod)
        {
        }
         
        
    }
}