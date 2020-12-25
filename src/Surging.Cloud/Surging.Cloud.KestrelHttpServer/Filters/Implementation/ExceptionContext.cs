using Microsoft.AspNetCore.Http;
using Surging.Cloud.CPlatform.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.KestrelHttpServer.Filters.Implementation
{
   public class ExceptionContext
    {
        public string RoutePath { get; internal set; }

        public Exception Exception { get; internal set; }

        public HttpResultMessage<object> Result { get; set; }

        public HttpContext Context { get; internal set; }
    }
}
