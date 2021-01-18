using Surging.Cloud.CPlatform.Address;
using Surging.Cloud.CPlatform.Messages;
using Surging.Cloud.CPlatform.Routing;
using Surging.Cloud.CPlatform.Transport;
using Surging.Cloud.CPlatform.Transport.Implementation;
using Surging.Cloud.CPlatform.Utilities;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Surging.Cloud.CPlatform.Runtime.Server.Implementation
{
    /// <summary>
    /// 服务主机基类。
    /// </summary>
    public abstract class ServiceHostAbstract : IServiceHost
    {
        #region Field

        private readonly IServiceExecutor _serviceExecutor;
        protected AddressModel HosAddress { get; } = NetUtils.GetHostAddress();

        public IServiceExecutor ServiceExecutor { get => _serviceExecutor; }

        /// <summary>
        /// 消息监听者。
        /// </summary>
        protected IMessageListener MessageListener { get; } = new MessageListener();

        #endregion Field

        #region Constructor

        protected ServiceHostAbstract(IServiceExecutor serviceExecutor)
        {
            _serviceExecutor = serviceExecutor;
            MessageListener.Received += MessageListener_Received;
        }

        #endregion Constructor

        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public virtual void Dispose() 
        {
            
            
        }

        #endregion Implementation of IDisposable

        #region Implementation of IServiceHost

        /// <summary>
        /// 启动主机。
        /// </summary>
        /// <param name="endPoint">主机终结点。</param>
        /// <returns>一个任务。</returns>
        public abstract Task StartAsync(EndPoint endPoint);

        #endregion Implementation of IServiceHost

        #region Private Method

        private async Task MessageListener_Received(IMessageSender sender, TransportMessage message)
        {
            await _serviceExecutor.ExecuteAsync(sender, message);
        }

        public abstract Task StartAsync(string ip,int port);

        #endregion Private Method
    }
}