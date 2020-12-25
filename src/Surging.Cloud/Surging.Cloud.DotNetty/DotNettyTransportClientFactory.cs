using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Utilities;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Microsoft.Extensions.Logging;
using Surging.Cloud.CPlatform;
using Surging.Cloud.CPlatform.Address;
using Surging.Cloud.CPlatform.Exceptions;
using Surging.Cloud.CPlatform.Messages;
using Surging.Cloud.CPlatform.Runtime.Client.HealthChecks;
using Surging.Cloud.CPlatform.Runtime.Server;
using Surging.Cloud.CPlatform.Transport;
using Surging.Cloud.CPlatform.Transport.Codec;
using Surging.Cloud.CPlatform.Transport.Implementation;
using Surging.Cloud.DotNetty.Adapter;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Surging.Cloud.DotNetty
{
    /// <summary>
    /// 基于DotNetty的传输客户端工厂。
    /// </summary>
    public class DotNettyTransportClientFactory : ITransportClientFactory, IDisposable
    {
        #region Field

        private readonly ITransportMessageEncoder _transportMessageEncoder;
        private readonly ITransportMessageDecoder _transportMessageDecoder;
        private readonly ILogger<DotNettyTransportClientFactory> _logger;
        private readonly IServiceExecutor _serviceExecutor;
        private readonly IHealthCheckService _healthCheckService;

        private readonly ConcurrentDictionary<EndPoint, Lazy<Task<ITransportClient>>> _clients =
            new ConcurrentDictionary<EndPoint, Lazy<Task<ITransportClient>>>();

        private readonly Bootstrap _bootstrap;

        private static readonly AttributeKey<IMessageSender> messageSenderKey =
            AttributeKey<IMessageSender>.ValueOf(typeof(DotNettyTransportClientFactory), nameof(IMessageSender));

        private static readonly AttributeKey<IMessageListener> messageListenerKey =
            AttributeKey<IMessageListener>.ValueOf(typeof(DotNettyTransportClientFactory), nameof(IMessageListener));

        private static readonly AttributeKey<EndPoint> origEndPointKey =
            AttributeKey<EndPoint>.ValueOf(typeof(DotNettyTransportClientFactory), nameof(EndPoint));

        #endregion Field

        #region Constructor

        public DotNettyTransportClientFactory(ITransportMessageCodecFactory codecFactory,
            IHealthCheckService healthCheckService, ILogger<DotNettyTransportClientFactory> logger)
            : this(codecFactory, healthCheckService, logger, null)
        {
        }

        public DotNettyTransportClientFactory(ITransportMessageCodecFactory codecFactory,
            IHealthCheckService healthCheckService, ILogger<DotNettyTransportClientFactory> logger,
            IServiceExecutor serviceExecutor)
        {
            _transportMessageEncoder = codecFactory.GetEncoder();
            _transportMessageDecoder = codecFactory.GetDecoder();
            _logger = logger;
            _healthCheckService = healthCheckService;
            _serviceExecutor = serviceExecutor;
            _bootstrap = GetBootstrap();
            _bootstrap.Handler(new ActionChannelInitializer<ISocketChannel>(c =>
            {
                var pipeline = c.Pipeline;
                pipeline.AddLast(new LengthFieldPrepender(4));
                pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));
                if (AppConfig.ServerOptions.EnableHealthCheck)
                {
                    pipeline.AddLast(new IdleStateHandler(0, AppConfig.ServerOptions.HealthCheckWatchIntervalInSeconds, 0));
                    pipeline.AddLast(DotNettyConstants.HeartBeatName, new HeartBeatHandler(_healthCheckService, this));
                }

                pipeline.AddLast(DotNettyConstants.TransportMessageAdapterName,
                    new TransportMessageChannelHandlerAdapter(_transportMessageDecoder));
                pipeline.AddLast(DotNettyConstants.ClientChannelHandler,
                    new DefaultChannelHandler(this, healthCheckService));
            }));
        }

        #endregion Constructor

        #region Implementation of ITransportClientFactory

        /// <summary>
        /// 创建客户端。
        /// </summary>
        /// <param name="endPoint">终结点。</param>
        /// <returns>传输客户端实例。</returns>
        public async Task<ITransportClient> CreateClientAsync(EndPoint endPoint)
        {
            var key = endPoint;
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"准备为服务端地址：{key}创建客户端。");
            try
            {
                return await _clients.GetOrAdd(key
                    , k => new Lazy<Task<ITransportClient>>(async () =>
                        {
                            //客户端对象
                            var bootstrap = _bootstrap;
                            //异步连接返回channel
                            var channel = await bootstrap.ConnectAsync(k);
                            var messageListener = new MessageListener();
                            //设置监听
                            channel.GetAttribute(messageListenerKey).Set(messageListener);
                            //实例化发送者
                            var messageSender = new DotNettyMessageClientSender(_transportMessageEncoder, channel);
                            messageSender.OnChannelUnActived += HandleChannelUnActived;
                            //设置channel属性
                            channel.GetAttribute(messageSenderKey).Set(messageSender);
                            channel.GetAttribute(origEndPointKey).Set(k);
                            //创建客户端
                            var client = new TransportClient(messageSender, messageListener, _logger, _serviceExecutor);
                            return client;
                        }
                    )).Value; //返回实例
            }
            catch (Exception ex)
            {
                //移除
                _clients.TryRemove(key, out var value);
                var ipEndPoint = endPoint as IPEndPoint;
                if (ipEndPoint == null)
                {
                    throw ex;
                }
                else
                {
                    throw new CommunicationException($"服务提供者{ipEndPoint.Address}:{ipEndPoint.Port}无法连接", ex);
                }
            }
        }

        private void HandleChannelUnActived(object sender, EndPoint e)
        {
            if (_clients.ContainsKey(e))
            {
                _clients.TryRemove(e, out var client);
            }
        }

        internal void RemoveClient(EndPoint e)
        {
            if (_clients.ContainsKey(e))
            {
                _clients.TryRemove(e, out var client);
            }
        }

        #endregion Implementation of ITransportClientFactory

        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            foreach (var client in _clients.Values)
            {
                (client as IDisposable)?.Dispose();
            }
        }

        #endregion Implementation of IDisposable

        private static Bootstrap GetBootstrap()
        {
            IEventLoopGroup group;

            var bootstrap = new Bootstrap();
            if (AppConfig.ServerOptions.Libuv)
            {
                group = new EventLoopGroup();
                bootstrap.Channel<TcpServerChannel>();
            }
            else
            {
                group = new MultithreadEventLoopGroup();
                bootstrap.Channel<TcpServerSocketChannel>();
            }

            bootstrap
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.ConnectTimeout, TimeSpan.FromMilliseconds(AppConfig.ServerOptions.RpcConnectTimeout))
                .Option(ChannelOption.TcpNodelay, true)
                .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
               // .Option(ChannelOption.SoKeepalive,true)
                .Group(group);

            return bootstrap;
        }

        protected class DefaultChannelHandler : ChannelHandlerAdapter
        {
            private readonly DotNettyTransportClientFactory _factory;
            private readonly IHealthCheckService _healthCheckService;

            public DefaultChannelHandler(DotNettyTransportClientFactory factory,
                IHealthCheckService healthCheckService)
            {
                _factory = factory;
                _healthCheckService = healthCheckService;
            }

            #region Overrides of ChannelHandlerAdapter

            public async override void ChannelInactive(IChannelHandlerContext context)
            {
                await RemoveServiceProvider(context);
                base.ChannelInactive(context);
            }

            public async override void ChannelActive(IChannelHandlerContext context)
            {
                await MarkServiceProviderHealth(context);
                base.ChannelActive(context);
            }

            public async override Task CloseAsync(IChannelHandlerContext context)
            {
                await RemoveServiceProvider(context);
                await base.CloseAsync(context);
            }

            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                var transportMessage = message as TransportMessage;
                var messageListener = context.Channel.GetAttribute(messageListenerKey).Get();
                var messageSender = context.Channel.GetAttribute(messageSenderKey).Get();
                messageListener.OnReceived(messageSender, transportMessage);
            }

            public async override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
            {
                if (!exception.IsBusinessException())
                {
                    await RemoveServiceProvider(context);
                }
            }

            #endregion Overrides of ChannelHandlerAdapter
            
            private async Task RemoveServiceProvider(IChannelHandlerContext context)
            {
                var providerServerEndpoint = context.Channel.RemoteAddress as IPEndPoint;
                var providerServerAddress = new IpAddressModel(providerServerEndpoint.Address.MapToIPv4().ToString(),
                    providerServerEndpoint.Port);
                _factory.RemoveClient(providerServerEndpoint);
                _factory.RemoveClient(context.Channel.GetAttribute(origEndPointKey).Get());
                await _healthCheckService.MarkFailure(providerServerAddress);
                if (context.Channel.Open || context.Channel.Active)
                {
                    await context.CloseAsync();
                    
                }
            }
            
            private async Task MarkServiceProviderHealth(IChannelHandlerContext context)
            {
                var providerServerEndpoint = context.Channel.RemoteAddress as IPEndPoint;
                var providerServerAddress = new IpAddressModel(providerServerEndpoint.Address.MapToIPv4().ToString(),
                    providerServerEndpoint.Port);
                await _healthCheckService.MarkHealth(providerServerAddress);
            }
        }
    }
}