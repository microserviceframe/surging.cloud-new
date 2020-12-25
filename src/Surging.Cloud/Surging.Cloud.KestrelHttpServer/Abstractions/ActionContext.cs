using Microsoft.AspNetCore.Http;
using Surging.Cloud.CPlatform.Messages;
using Surging.Cloud.CPlatform.Runtime.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.KestrelHttpServer
{
    public  class ActionContext
    {
        public ActionContext()
        {

        }

        public HttpContext HttpContext { get; set; }

        public TransportMessage Message { get; set; }
         
    }
}
