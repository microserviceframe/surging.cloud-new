using Surging.Cloud.CPlatform.Address;
using Surging.Cloud.CPlatform.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Surging.Cloud.CPlatform.Utilities;

namespace Surging.Cloud.CPlatform.Routing.Implementation
{
    /// <summary>
    /// 服务路由事件参数。
    /// </summary>
    public class ServiceRouteEventArgs
    {
        public ServiceRouteEventArgs(ServiceRoute route)
        {
            Route = route;
        }

        /// <summary>
        /// 服务路由信息。
        /// </summary>
        public ServiceRoute Route { get; private set; }
    }

    /// <summary>
    /// 服务路由变更事件参数。
    /// </summary>
    public class ServiceRouteChangedEventArgs : ServiceRouteEventArgs
    {
        public ServiceRouteChangedEventArgs(ServiceRoute route, ServiceRoute oldRoute) : base(route)
        {
            OldRoute = oldRoute;
        }

        /// <summary>
        /// 旧的服务路由信息。
        /// </summary>
        public ServiceRoute OldRoute { get; set; }
    }

    /// <summary>
    /// 服务路由管理者基类。
    /// </summary>
    public abstract class ServiceRouteManagerBase : IServiceRouteManager
    {
        private readonly ISerializer<string> _serializer;

        public event EventHandler<ServiceRouteEventArgs> Created;
        public event EventHandler<ServiceRouteEventArgs> Removed;
        public event EventHandler<ServiceRouteChangedEventArgs> Changed;

        protected ServiceRouteManagerBase(ISerializer<string> serializer)
        {
            _serializer = serializer;
        }

        #region Implementation of IServiceRouteManager


        /// <summary>
        /// 获取所有可用的服务路由信息。
        /// </summary>
        /// <returns>服务路由集合。</returns>
        public abstract Task<IEnumerable<ServiceRoute>> GetRoutesAsync(bool needUpdateFromServiceCenter = false);

        /// <summary>
        /// 设置服务路由。
        /// </summary>
        /// <param name="routes">服务路由集合。</param>
        /// <returns>一个任务。</returns>
        public virtual Task SetRoutesAsync(IEnumerable<ServiceRoute> routes)
        {
            if (routes == null)
                throw new ArgumentNullException(nameof(routes));

            var descriptors = routes.Where(route => route != null).Select(route =>
            {
               var descriptor =  new ServiceRouteDescriptor
                {
                    AddressDescriptors = route.Address?.Select(address => new ServiceAddressDescriptor
                    {
                        Value = _serializer.Serialize(address)
                    }) ?? Enumerable.Empty<ServiceAddressDescriptor>(),
                    ServiceDescriptor = route.ServiceDescriptor
                };
               descriptor.ServiceDescriptor.TimeStamp = DateTimeConverter.DateTimeToUnixTimestamp(DateTime.Now);
               return descriptor;
            });

            return SetRoutesAsync(descriptors);
        }

        public virtual Task SetRouteAsync(ServiceRoute route)
        {
            if (route == null)
                throw new ArgumentNullException(nameof(route));
            var descriptor = new ServiceRouteDescriptor
            {
                AddressDescriptors = route.Address?.Select(address => new ServiceAddressDescriptor
                {
                    Value = _serializer.Serialize(address)
                }) ?? Enumerable.Empty<ServiceAddressDescriptor>(),
                ServiceDescriptor = route.ServiceDescriptor
            };
            return SetRouteAsync(descriptor);
        }

        public abstract Task<ServiceRoute> GetRouteByPathAsync(string path, string httpMethod);


        public abstract Task<ServiceRoute> GetRouteByServiceIdAsync(string serviceId, bool isCache = true);
       

        public abstract Task RemveAddressAsync(IEnumerable<AddressModel> address);

        public abstract Task RemveAddressAsync(IEnumerable<AddressModel> address, string serviceId);

        protected abstract Task RemveAddressAsync(IEnumerable<AddressModel> address, ServiceRoute route);

        /// <summary>
        /// 清空所有的服务路由。
        /// </summary>
        /// <returns>一个任务。</returns>
        public abstract Task ClearAsync();

        #endregion Implementation of IServiceRouteManager

        /// <summary>
        /// 设置服务路由。
        /// </summary>
        /// <param name="routes">服务路由集合。</param>
        /// <returns>一个任务。</returns>
        protected abstract Task SetRoutesAsync(IEnumerable<ServiceRouteDescriptor> routes);

        protected abstract Task SetRouteAsync(ServiceRouteDescriptor route);

        protected void OnCreated(params ServiceRouteEventArgs[] args)
        {
            if (Created == null)
                return;

            foreach (var arg in args)
                Created(this, arg);
        }

        protected void OnChanged(params ServiceRouteChangedEventArgs[] args)
        {
            if (Changed == null)
                return;

            foreach (var arg in args)
                Changed(this, arg);
        }

        protected void OnRemoved(params ServiceRouteEventArgs[] args)
        {
            if (Removed == null)
                return;

            foreach (var arg in args)
                Removed(this, arg);
        }


    }
}