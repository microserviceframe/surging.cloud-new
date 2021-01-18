﻿using DotNetty.Codecs.Http;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Logging;
using Surging.Cloud.CPlatform;
using Surging.Cloud.CPlatform.Configurations;
using Surging.Cloud.CPlatform.Exceptions;
using Surging.Cloud.CPlatform.Messages;
using Surging.Cloud.CPlatform.Routing;
using Surging.Cloud.CPlatform.Routing.Template;
using Surging.Cloud.CPlatform.Serialization;
using Surging.Cloud.CPlatform.Transport;
using Surging.Cloud.CPlatform.Transport.Codec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using TaskCompletionSource = DotNetty.Common.Concurrency.TaskCompletionSource;

namespace Surging.Cloud.Protocol.Http
{
    class DotNettyHttpServerMessageListener : IMessageListener, IDisposable
    {
        #region Field

        private readonly ILogger<DotNettyHttpServerMessageListener> _logger;
        private readonly ITransportMessageDecoder _transportMessageDecoder;
        private readonly ITransportMessageEncoder _transportMessageEncoder;
        private IChannel _channel;
        private readonly ISerializer<string> _serializer;
        private readonly IServiceRouteProvider _serviceRouteProvider;

        #endregion Field

        #region Constructor

        public DotNettyHttpServerMessageListener(ILogger<DotNettyHttpServerMessageListener> logger,
            ITransportMessageCodecFactory codecFactory, 
            ISerializer<string> serializer, 
            IServiceRouteProvider serviceRouteProvider)
        {
            _logger = logger;
            _transportMessageEncoder = codecFactory.GetEncoder();
            _transportMessageDecoder = codecFactory.GetDecoder();
            _serializer = serializer;
            _serviceRouteProvider = serviceRouteProvider;
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
            var serverCompletion = new TaskCompletionSource();
            var bossGroup = new MultithreadEventLoopGroup(1);
            var workerGroup = new MultithreadEventLoopGroup();//Default eventLoopCount is Environment.ProcessorCount * 2
            var bootstrap = new ServerBootstrap();
            bootstrap
            .Group(bossGroup, workerGroup)
            .Channel<TcpServerSocketChannel>()
            .Option(ChannelOption.SoReuseport, true)
            .ChildOption(ChannelOption.SoReuseaddr, true)
            .Option(ChannelOption.SoBacklog, AppConfig.ServerOptions.SoBacklog)
            .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
            {
                IChannelPipeline pipeline = channel.Pipeline;
                pipeline.AddLast("encoder", new HttpResponseEncoder());
                pipeline.AddLast(new HttpRequestDecoder(int.MaxValue, 8192, 8192, true));
                pipeline.AddLast(new HttpObjectAggregator(int.MaxValue));
                pipeline.AddLast(new ServerHandler(async (contenxt, message) =>
                {
                    var sender = new DotNettyHttpServerMessageSender(_transportMessageEncoder, contenxt, _serializer);
                    await OnReceived(sender, message);
                }, _logger, _serializer, _serviceRouteProvider));
                serverCompletion.TryComplete();
            }));
            try
            {
                _channel = await bootstrap.BindAsync(endPoint);
                _logger.LogInformation($"Rpc服务主机(Http协议){AppConfig.ServerOptions.HostName}启动成功,RPC服务地址:{endPoint}。");
            }
            catch(Exception ex)
            {
                _logger.LogError($"Rpc服务主机(Http协议){AppConfig.ServerOptions.HostName}启动失败,原因:{ex.Message},RPC服务地址：{endPoint}。 ");
                throw ex;
            }

        }

        public void CloseAsync()
        {
            Task.Run(async () =>
            {
                await _channel.EventLoop.ShutdownGracefullyAsync();
                await _channel.CloseAsync();
            }).GetAwaiter().GetResult();
        }

        #region Implementation of IDisposable

        
        public void Dispose()
        {
            Task.Run(async () =>
            {
                await _channel.DisconnectAsync();
            }).GetAwaiter().GetResult();
        }

        #endregion Implementation of IDisposable

        #region Help Class
        private class ServerHandler : SimpleChannelInboundHandler<IFullHttpRequest>
        {
            readonly TaskCompletionSource completion = new TaskCompletionSource();

            private readonly Action<IChannelHandlerContext, TransportMessage> _readAction;
            private readonly ILogger _logger;
            private readonly ISerializer<string> _serializer;
            private readonly IServiceRouteProvider _serviceRouteProvider;

            public ServerHandler(Action<IChannelHandlerContext, TransportMessage> readAction, 
                ILogger logger, 
                ISerializer<string> serializer,
                IServiceRouteProvider serviceRouteProvider)
            {
                _readAction = readAction;
                _logger = logger;
                _serializer = serializer;
                _serviceRouteProvider = serviceRouteProvider;
            }

            public bool WaitForCompletion()
            {
                this.completion.Task.Wait(TimeSpan.FromSeconds(5));
                return this.completion.Task.Status == TaskStatus.RanToCompletion;
            }

            protected override void ChannelRead0(IChannelHandlerContext ctx, IFullHttpRequest msg)
            {
                var data = new byte[msg.Content.ReadableBytes];
                msg.Content.ReadBytes(data);

                Task.Run(async () =>
                {
                    var parameters = GetParameters(HttpUtility.UrlDecode(msg.Uri), out string path);
                    path = AppConfig.MapRoutePathOptions.GetRoutePath(path, msg.Method.Name);
                    var serviceRoute = await _serviceRouteProvider.GetRouteByPathOrRegexPath(path,msg.Method.Name.ToUpper());
                    if (serviceRoute == null)
                    {
                        throw new CPlatformException($"未能找到路径为{path}-{msg.Method.Name.ToUpper()}的路由信息", StatusCode.Http404EndpointStatusCode);
                    }
                    parameters.Remove("servicekey", out object serviceKey);
                    if (data.Length > 0)
                        parameters = _serializer.Deserialize<string, IDictionary<string, object>>(System.Text.Encoding.ASCII.GetString(data)) ?? new Dictionary<string, object>();
                    if (String.Compare(serviceRoute.ServiceDescriptor.RoutePath, path, true) != 0)
                    {
                        var @params = RouteTemplateSegmenter.Segment(serviceRoute.ServiceDescriptor.RoutePath, path);
                        foreach (var param in @params)
                        {
                            parameters.Add(param.Key,param.Value);
                        }
                    }
                    if (msg.Method.Name == "POST")
                    {
                        _readAction(ctx, new TransportMessage(new HttpMessage
                        {
                            Parameters = parameters,
                            RoutePath = serviceRoute.ServiceDescriptor.RoutePath,
                            HttpMethod = msg.Method.Name.ToUpper(),
                            ServiceKey = serviceKey?.ToString()
                        }));
                    }
                    else
                    {
                        _readAction(ctx, new TransportMessage(new HttpMessage
                        {
                            Parameters = parameters,
                            RoutePath = serviceRoute.ServiceDescriptor.RoutePath,
                            HttpMethod = msg.Method.Name.ToUpper(),
                            ServiceKey = serviceKey?.ToString()
                        }));
                    }
                });
            }

            public IDictionary<string, object> GetParameters(string msg, out string routePath)
            {
                var urlSpan = msg.AsSpan();
                var len = urlSpan.IndexOf("?");
                if (len == -1)
                {
                    routePath = urlSpan.TrimStart("/").ToString().ToLower();
                    return new  Dictionary<string, object>();
                }
                routePath = urlSpan.Slice(0, len).TrimStart("/").ToString().ToLower();
                var paramStr = urlSpan.Slice(len + 1).ToString();
                var parameters = paramStr.Split('&');
                return parameters.ToList().Select(p => p.Split("=")).ToDictionary(p => p[0].ToLower(), p => (object)p[1]);
            }

            public override void ExceptionCaught(IChannelHandlerContext context, Exception exception) => this.completion.TrySetException(exception);
        }

        #endregion Help Class
    }
}
