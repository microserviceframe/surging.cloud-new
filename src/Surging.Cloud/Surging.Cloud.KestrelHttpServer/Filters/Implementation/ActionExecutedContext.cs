using Microsoft.AspNetCore.Http;
using Surging.Cloud.CPlatform.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.KestrelHttpServer.Filters.Implementation
{
   public class ActionExecutedContext
    {
        public HttpMessage Message { get; internal set; }
        public HttpContext Context { get; internal set; }
    }
}
