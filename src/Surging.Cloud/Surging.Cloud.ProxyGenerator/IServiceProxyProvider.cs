using Surging.Cloud.CPlatform;
using Surging.Cloud.CPlatform.Runtime;
using Surging.Cloud.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Cloud.ProxyGenerator
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
