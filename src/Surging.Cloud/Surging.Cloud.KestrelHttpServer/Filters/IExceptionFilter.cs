using Surging.Cloud.KestrelHttpServer.Filters.Implementation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Cloud.KestrelHttpServer.Filters
{
   public interface IExceptionFilter
    { 
          Task OnException(ExceptionContext context);
    }
}
