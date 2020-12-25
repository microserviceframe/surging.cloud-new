using Microsoft.AspNetCore.Http;
using Surging.Cloud.CPlatform.Messages;
using Surging.Cloud.CPlatform.Routing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.KestrelHttpServer.Filters.Implementation
{
   public  class ActionExecutingContext
    {
        public HttpMessage Message { get; internal set; }

        public ServiceRoute Route { get; internal set; }

        public HttpResultMessage<object> Result { get; set; }
          
        public HttpContext Context { get; internal set; }
    }
}
