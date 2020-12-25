using Microsoft.Extensions.Logging;
using Surging.Cloud.CPlatform.Address;
using Surging.Cloud.CPlatform.Exceptions;
using Surging.Cloud.CPlatform.HashAlgorithms;
using Surging.Cloud.CPlatform.Messages;
using Surging.Cloud.CPlatform.Runtime.Client.Address.Resolvers;
using Surging.Cloud.CPlatform.Runtime.Client.HealthChecks;
using Surging.Cloud.CPlatform.Transport;
using Surging.Cloud.CPlatform.Transport.Implementation;
using Surging.Cloud.CPlatform.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Cloud.CPlatform.Runtime.Client.Implementation
{
    /// <summary>
    /// 远程调用服务
    /// </summary>
    public class RemoteInvokeService : IRemoteInvokeService
    {
        private readonly IAddressResolver _addressResolver;
        private readonly ITransportClientFactory _transportClientFactory;
        private readonly ILogger<RemoteInvokeService> _logger;
        private readonly IHealthCheckService _healthCheckService;

        public RemoteInvokeService(IHashAlgorithm hashAlgorithm, IAddressResolver addressResolver, ITransportClientFactory transportClientFactory, ILogger<RemoteInvokeService> logger, IHealthCheckService healthCheckService)
        {
            _addressResolver = addressResolver;
            _transportClientFactory = transportClientFactory;
            _logger = logger;
            _healthCheckService = healthCheckService;
        }

        #region Implementation of IRemoteInvokeService

        public async Task<RemoteInvokeResultMessage> InvokeAsync(RemoteInvokeContext context)
        {
            return await InvokeAsync(context, Task.Factory.CancellationToken);
        }

        public async Task<RemoteInvokeResultMessage> InvokeAsync(RemoteInvokeContext context, CancellationToken cancellationToken)
        {
            var invokeMessage = context.InvokeMessage;
            AddressModel address = null;

            try
            {
                address = await ResolverAddress(context, context.Item);
                var endPoint = address.CreateEndPoint();
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation($"使用地址：'{endPoint}'进行调用。");
                var client = await _transportClientFactory.CreateClientAsync(endPoint);
                RpcContext.GetContext().SetAttachment("RemoteAddress", address.ToString());
                var rpcResult = await client.SendAsync(invokeMessage, cancellationToken).WithCancellation(cancellationToken);
                return rpcResult;
            }
            catch (CommunicationException ex)
            {
                if (address != null)
                {
                    _logger.LogError($"使用地址：'{address.ToString()}'调用服务{context.InvokeMessage.ServiceId}通信错误,原因:{ex.Message}");
                }
                if (address != null)
                {
                    await _healthCheckService.MarkFailure(address);
                }
                throw;
            }
            catch (TimeoutException ex)
            {
                if (address != null)
                {
                    _logger.LogError($"使用地址：'{address.ToString()}'调用服务{context.InvokeMessage.ServiceId}超时,原因:{ex.Message}");
                }
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"发起请求中发生了错误，服务Id：{invokeMessage.ServiceId}。");

                throw;
            }            
        }

        public async Task<RemoteInvokeResultMessage> InvokeAsync(RemoteInvokeContext context, int requestTimeout)
        {
            var invokeMessage = context.InvokeMessage;
            AddressModel address = null;

            try
            {
                address = await ResolverAddress(context, context.Item);
                var endPoint = address.CreateEndPoint();
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation($"使用地址：'{endPoint}'进行调用。");
   
                var client = await _transportClientFactory.CreateClientAsync(endPoint);
                RpcContext.GetContext().SetAttachment("RemoteAddress", address.ToString());
                using (var cts = new CancellationTokenSource())
                {
                    var rpcResult = await client.SendAsync(invokeMessage, cts.Token).WithCancellation(cts, requestTimeout);
                    return rpcResult;
                }
            }
            catch (CommunicationException ex)
            {
                if (address != null)
                {
                   
                    _logger.LogError($"使用地址：'{address.ToString()}'调用服务{context.InvokeMessage.ServiceId}失败,原因:{ex.Message}");
                    await _healthCheckService.MarkFailure(address);
                }
                throw;
            }
            catch (TimeoutException ex)
            {
                if (address != null)
                {
                    _logger.LogError($"使用地址：'{address.ToString()}'调用服务{context.InvokeMessage.ServiceId}超时,原因:{ex.Message}");
                    //await _healthCheckService.MarkTimeout(address, invokeMessage.ServiceId);
                }
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"发起请求中发生了错误，服务Id：{invokeMessage.ServiceId}。错误信息：{exception.Message}");
                throw;
            }
        }

        private async Task<AddressModel> ResolverAddress(RemoteInvokeContext context, string item)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.InvokeMessage == null)
                throw new ArgumentNullException(nameof(context.InvokeMessage));

            if (string.IsNullOrEmpty(context.InvokeMessage.ServiceId))
                throw new ArgumentException("服务Id不能为空。", nameof(context.InvokeMessage.ServiceId));
            //远程调用信息
            var invokeMessage = context.InvokeMessage;
            //解析服务地址
            var address =  await _addressResolver.Resolver(invokeMessage.ServiceId, item);         
            if (address == null)
                throw new CPlatformException($"无法解析服务Id：{invokeMessage.ServiceId}的地址信息。");
            return address;
        }

        #endregion Implementation of IRemoteInvokeService
    }
}