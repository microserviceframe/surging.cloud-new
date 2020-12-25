using Surging.Cloud.CPlatform.Exceptions;
using Surging.Cloud.CPlatform.Filters.Implementation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.KestrelHttpServer.Filters.Implementation
{
    public class ServiceExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(RpcActionExecutedContext context)
        {
            if (context.Exception is CPlatformCommunicationException)
                throw new Exception(context.Exception.Message, context.Exception);
        }
    }
}
