using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Cloud.ProxyGenerator.Interceptors
{
    public interface IInterceptor
    {
        Task Intercept(IInvocation invocation);
    }
}
