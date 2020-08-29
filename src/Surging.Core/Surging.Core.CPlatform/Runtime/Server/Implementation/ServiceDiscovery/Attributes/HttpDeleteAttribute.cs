
namespace Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes
{
   public class HttpDeleteAttribute : HttpMethodAttribute
    {
        private static readonly string _supportedMethod =  HttpMethod.DELETE.ToString();

        public HttpDeleteAttribute()
            : base(_supportedMethod)
        {
        }

        
    }
}
