namespace Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes
{
    public class HttpPutAttribute : HttpMethodAttribute
    {
        private static readonly string _supportedMethod = HttpMethod.PUT.ToString();

        public HttpPutAttribute()
            : base(_supportedMethod)
        {
        }

        
    }
}