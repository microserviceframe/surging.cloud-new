using System;
using System.Collections.Generic;


namespace Surging.Cloud.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public abstract class HttpMethodAttribute : Attribute
    { 


        public HttpMethodAttribute(params string [] httpMethods)
        {
            if (httpMethods == null)
            {
                throw new ArgumentNullException(nameof(httpMethods));
            }

            HttpMethods = httpMethods;
            //IsRegisterMetadata = isRegisterMetadata;
        } 
        public IEnumerable<string> HttpMethods { get; }
        //public bool IsRegisterMetadata { get; }

    }
}
