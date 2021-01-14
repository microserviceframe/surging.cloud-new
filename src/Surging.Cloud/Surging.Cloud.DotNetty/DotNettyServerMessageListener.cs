using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Microsoft.Extensions.Logging;
using Surging.Cloud.CPlatform;
using Surging.Cloud.CPlatform.Messages;
using Surging.Cloud.CPlatform.Transport;
using Surging.Cloud.CPlatform.Transport.Codec;
using Surging.Cloud.DotNetty.Adapter;
using System;
using System.Net;
using System.Threading.Tasks;
using Surging.Cloud.CPlatform.Address;
using Surging.Cloud.CPlatform.Runtime.Client.HealthChecks;
using Surging.Cloud.CPlatform.Utilities;

namespace Surging.Cloud.DotNetty
{
    public class DotNettyServerMessageListener : IMessageListener, IDisposable
    {
        #region Field

        private readonly ILogger<DotNettyServerMessageListener> _logger;
        private readonly ITransportMessageDecoder _transportMessageDecoder;
        private readonly ITransportMessageEncoder _transportMessageEncoder;
        private IChannel _channel;

        #endregion Field

        #region Constructor

        public DotNettyServerMessageListener(ILogger<DotNettyServerMessageListener> logger, ITransportMessageCodecFactory codecFactory)
        {
            _logger = logger;
            _transportMessageEncoder = codecFactory.GetEncoder();
            _transportMessageDecoder = codecFactory.GetDecoder();
        }

        #endregion Constructor

        #region Implementation of IMessageListener

        public event ReceivedDelegate Received;

        /// <summary>
        /// 触发接收到消息事件。
        /// </summary>
        /// <param name="sender">消息发送者。</param>
        /// <param name="message">接收到的消息。</param>
        /// <returns>一个任务。</returns>
        public async Task OnReceived(IMessageSender sender, TransportMessage message)
        {
            if (Received == null)
                return;
            await Received(sender, message);
        }

        #endregion Implementation of IMessageListener

        public async Task StartAsync(EndPoint endPoint)
        {
            IEventLoopGroup bossGroup = new MultithreadEventLoopGroup(1);
            IEventLoopGroup workerGroup = new MultithreadEventLoopGroup();//Default eventLoopCount is Environment.ProcessorCount * 2
            var bootstrap = new ServerBootstrap();
           
            if (AppConfig.ServerOptions.Libuv)
            {
                var dispatcher = new DispatcherEventLoopGroup();
                bossGroup = dispatcher;
                workerGroup = new WorkerEventLoopGroup(dispatcher);
                bootstrap.Channel<TcpServerChannel>();
            }
            else
            {
                bossGroup = new MultithreadEventLoopGroup(1);
                workerGroup = new MultithreadEventLoopGroup();
                bootstrap.Channel<TcpServerSocketChannel>();
            } 
            bootstrap
            .Option(ChannelOption.SoBacklog, AppConfig.ServerOptions.SoBacklog)
            .ChildOption(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
            .Group(bossGroup, workerGroup)
            .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
            {
                var pipeline = channel.Pipeline;
               
                pipeline.AddLast(new LengthFieldPrepender(4));
                pipeline.AddLast(new LengthFieldBasedFrameDecoder(int.MaxValue, 0, 4, 0, 4));
                if (AppConfig.ServerOptions.EnableHealthCheck)
                {
                    pipeline.AddLast(new IdleStateHandler(AppConfig.ServerOptions.HealthCheckWatchIntervalInSeconds * 2, 0, 0));
                    pipeline.AddLast(new ChannelInboundHandlerAdapter());
                    
                }
                pipeline.AddLast(DotNettyConstants.TransportMessageAdapterName, new TransportMessageChannelHandlerAdapter(_transportMessageDecoder));
                pipeline.AddLast(new ServerHandler(async (contenxt, message) =>
                {
                    var sender = new DotNettyServerMessageSender(_transportMessageEncoder, contenxt);
                    await OnReceived(sender, message);
                }, _logger));
            }));
            try
            {
                _channel = await bootstrap.BindAsync(endPoint);
                _logger.LogInformation($"Rpc服务主机(Tcp协议){AppConfig.ServerOptions.HostName}启动成功,RPC服务地址：{endPoint}.");
                    
            }
            catch(Exception ex)
            {
                _logger.LogError($"Rpc服务主机(Tcp协议){AppConfig.ServerOptions.HostName}启动失败,原因：{ex.Message},RPC服务地址：{endPoint}.");
                throw ex;
            }
        }

        public void CloseAsync()
        {
            Task.Run(async () =>
            {
                await _channel.EventLoop.ShutdownGracefullyAsync();
                await _channel.CloseAsync();
            }).Wait();
        }

        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public async void Dispose()
        {
            await _channel.CloseAsync();
        }

        #endregion Implementation of IDisposable

        #region Help Class

        private class ServerHandler : ChannelHandlerAdapter
        {
            private readonly Action<IChannelHandlerContext, TransportMessage> _readAction;
            private readonly ILogger _logger;
            private readonly IHealthCheckService _healthCheckService;

            public ServerHandler(Action<IChannelHandlerContext, TransportMessage> readAction, ILogger logger)
            {
                _readAction = readAction;
                _logger = logger;
                _healthCheckService = ServiceLocator.GetService<IHealthCheckService>();
            }

            #region Overrides of ChannelHandlerAdapter

            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                Task.Run(() =>
                {
                    var transportMessage = (TransportMessage)message;
                    _readAction(context, transportMessage);
                });
            }
            
            public override void ChannelReadComplete(IChannelHandlerContext context)
            {
                context.Flush();
            }

            public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
            {
                context.Channel.CloseAsync();//客户端主动断开需要应答，否则socket变成CLOSE_WAIT状态导致socket资源耗尽
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(exception,$"与服务器：{context.Channel.RemoteAddress}通信时发送了错误。");
            }

            public override async Task CloseAsync(IChannelHandlerContext context)
            {
                await MarkServiceUnHealth(context);
                await base.CloseAsync(context);
            }

            public async override void ChannelActive(IChannelHandlerContext context)
            {
               await MarkServiceHealth(context);
               base.ChannelActive(context);
            }

            private async Task MarkServiceUnHealth(IChannelHandlerContext context)
            {
                var iPEndPoint = context.Channel.RemoteAddress as IPEndPoint;
                var ipAddressModel = new IpAddressModel(iPEndPoint.Address.MapToIPv4().ToString(),
                    iPEndPoint.Port);
                await _healthCheckService.MarkFailure(ipAddressModel);
            }
            
            private async Task MarkServiceHealth(IChannelHandlerContext context)
            {
                var iPEndPoint = context.Channel.RemoteAddress as IPEndPoint;
                var ipAddressModel = new IpAddressModel(iPEndPoint.Address.MapToIPv4().ToString(),
                    iPEndPoint.Port);
                await _healthCheckService.MarkHealth(ipAddressModel);
            }


            #endregion Overrides of ChannelHandlerAdapter
        }

        #endregion Help Class
    }
}