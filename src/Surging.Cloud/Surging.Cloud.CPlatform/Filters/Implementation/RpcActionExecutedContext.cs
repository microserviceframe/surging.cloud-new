using Surging.Cloud.CPlatform.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.CPlatform.Filters.Implementation
{
    public class RpcActionExecutedContext
    {

        public RemoteInvokeMessage InvokeMessage { get; set; }
         
        public Exception Exception { get; set; }
    }
}
