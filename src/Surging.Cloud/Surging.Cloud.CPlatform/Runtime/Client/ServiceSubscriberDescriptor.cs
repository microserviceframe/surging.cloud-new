using Surging.Cloud.CPlatform.Routing;
using System.Collections.Generic;

namespace Surging.Cloud.CPlatform.Runtime.Client
{
    public class ServiceSubscriberDescriptor
    {
        /// <summary>
        /// 服务地址描述符集合。
        /// </summary>
        public IEnumerable<ServiceAddressDescriptor> AddressDescriptors { get; set; }

        /// <summary>
        /// 服务描述符。
        /// </summary>
        public ServiceDescriptor ServiceDescriptor { get; set; }
    }
}
