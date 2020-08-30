using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Runtime;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.ProxyGenerator
{
    /// <summary>
    /// 代理服务接口
    /// </summary>
   public interface  IServiceProxyProvider
    {
        
        Task<T> Invoke<T>(IDictionary<string, object> parameters, string routePath, HttpMethod httpMethod);

        Task<T> Invoke<T>(IDictionary<string, object> parameters, string routePath, HttpMethod httpMethod, string serviceKey);
    }
}
