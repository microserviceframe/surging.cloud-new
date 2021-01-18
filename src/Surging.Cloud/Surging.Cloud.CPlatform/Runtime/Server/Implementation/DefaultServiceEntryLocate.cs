using Surging.Cloud.CPlatform.Exceptions;
using Surging.Cloud.CPlatform.Messages;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Surging.Cloud.CPlatform.Runtime.Server.Implementation
{
    /// <summary>
    /// 默认的服务条目定位器。
    /// </summary>
    public class DefaultServiceEntryLocate : IServiceEntryLocate
    {
        private readonly IServiceEntryManager _serviceEntryManager;

        public DefaultServiceEntryLocate(IServiceEntryManager serviceEntryManager)
        {
            _serviceEntryManager = serviceEntryManager;
        }

        #region Implementation of IServiceEntryLocate

        /// <summary>
        /// 定位服务条目。
        /// </summary>
        /// <param name="invokeMessage">远程调用消息。</param>
        /// <returns>服务条目。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ServiceEntry Locate(RemoteInvokeMessage invokeMessage)
        {
            var serviceEntries = _serviceEntryManager.GetEntries();
            return serviceEntries.SingleOrDefault(i => i.Descriptor.Id == invokeMessage.ServiceId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ServiceEntry Locate(HttpMessage httpMessage)
        {
            string routePath = httpMessage.RoutePath;
            if (httpMessage.RoutePath.AsSpan().IndexOf("/") == -1)
                routePath = $"/{routePath}";
            var serviceEntries = _serviceEntryManager.GetEntries(); //_serviceEntryManager.GetAllEntries();
            var requestServiceEntries = serviceEntries.Where(i => i.RoutePath == routePath );
                // && i.Methods.Contains(httpMessage.HttpMethod) && !i.Descriptor.GetMetadata<bool>("IsOverload")
         
            if (requestServiceEntries.Count() > 1) 
            {
                throw new CPlatformException($"存在{requestServiceEntries.Count()}个{httpMessage.RoutePath}-{httpMessage.HttpMethod}的路由配置,请检查路由配置信息",StatusCode.RouteError);
            }
            return requestServiceEntries.FirstOrDefault();
        }

        #endregion Implementation of IServiceEntryLocate
    }
}