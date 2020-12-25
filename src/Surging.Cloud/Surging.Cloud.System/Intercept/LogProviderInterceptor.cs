using Surging.Cloud.CPlatform.Messages;
using Surging.Cloud.ProxyGenerator.Interceptors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Cloud.System.Intercept
{
    public class LogProviderInterceptor : IInterceptor
    {
        public async Task Intercept(IInvocation invocation)
        {
            var watch = Stopwatch.StartNew();
            await invocation.Proceed();
            var result = invocation.ReturnValue;
        }
    }
}
