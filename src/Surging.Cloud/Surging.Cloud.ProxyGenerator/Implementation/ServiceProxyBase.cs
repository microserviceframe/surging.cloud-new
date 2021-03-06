﻿using Newtonsoft.Json.Linq;
using Surging.Cloud.CPlatform;
using Surging.Cloud.CPlatform.Convertibles;
using Surging.Cloud.CPlatform.Messages;
using Surging.Cloud.CPlatform.Runtime.Client;
using Surging.Cloud.CPlatform.Support;
using Surging.Cloud.ProxyGenerator.Interceptors;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Surging.Cloud.CPlatform.Utilities;
using System.Linq;
using Microsoft.Extensions.Logging;
using Surging.Cloud.CPlatform.Exceptions;
using Surging.Cloud.CPlatform.Transport.Implementation;

namespace Surging.Cloud.ProxyGenerator.Implementation
{
    /// <summary>
    /// 一个抽象的服务代理基类。
    /// </summary>
    public abstract class ServiceProxyBase
    {
        #region Field
        private readonly IRemoteInvokeService _remoteInvokeService;
        private readonly ITypeConvertibleService _typeConvertibleService;
        private readonly string _serviceKey;
        private readonly CPlatformContainer _serviceProvider;
        private readonly IServiceCommandProvider _commandProvider;
        private readonly IBreakeRemoteInvokeService _breakeRemoteInvokeService;
        private readonly IEnumerable<IInterceptor> _interceptors;
        private readonly IInterceptor _cacheInterceptor;
        protected readonly ILogger<ServiceProxyBase> _logger;
        #endregion Field

        #region Constructor

        protected ServiceProxyBase(IRemoteInvokeService remoteInvokeService,
            ITypeConvertibleService typeConvertibleService, String serviceKey, CPlatformContainer serviceProvider)
        {
            _remoteInvokeService = remoteInvokeService;
            _typeConvertibleService = typeConvertibleService;
            _serviceKey = serviceKey;
            _serviceProvider = serviceProvider;
            _commandProvider = serviceProvider.GetInstances<IServiceCommandProvider>();
            _breakeRemoteInvokeService = serviceProvider.GetInstances<IBreakeRemoteInvokeService>();
            _logger = serviceProvider.GetInstances<ILogger<ServiceProxyBase>>();
            _interceptors = new List<IInterceptor>();
            if (serviceProvider.Current.IsRegistered<IInterceptor>())
            {
                var interceptors = serviceProvider.GetInstances<IEnumerable<IInterceptor>>();
                _interceptors = interceptors.Where(p => !typeof(CacheInterceptor).IsAssignableFrom(p.GetType()));
                _cacheInterceptor = interceptors.Where(p => typeof(CacheInterceptor).IsAssignableFrom(p.GetType())).FirstOrDefault();
            }


        }
        #endregion Constructor

        #region Protected Method
        /// <summary>
        /// 远程调用。
        /// </summary>
        /// <typeparam name="T">返回类型。</typeparam>
        /// <param name="parameters">参数字典。</param>
        /// <param name="serviceId">服务Id。</param>
        /// <returns>调用结果。</returns>
        protected async Task<T> Invoke<T>(IDictionary<string, object> parameters, string serviceId)
        {
            object result = default(T);
            var command = await _commandProvider.GetCommand(serviceId);
            RemoteInvokeResultMessage message = null;
            var decodeJOject = typeof(T) == UtilityType.ObjectType;
            IInvocation invocation = null;
            var existsInterceptor = _interceptors.Any();
            if ((_cacheInterceptor == null || !command.RequestCacheEnabled) && !existsInterceptor)
            {
                message = await _breakeRemoteInvokeService.InvokeAsync(parameters, serviceId, _serviceKey, decodeJOject);
                if (message == null || !message.IsSucceedRemoteInvokeCalled())
                {
                    return await FallBackRetryInvoke<T>(parameters, serviceId, command);
                }
            }
            if (_cacheInterceptor != null && command.RequestCacheEnabled)
            {
                invocation = GetCacheInvocation(parameters, serviceId, typeof(T));
                if (invocation != null)
                {
                    var interceptReuslt = await Intercept(_cacheInterceptor, invocation);
                    message = interceptReuslt.Item1;
                    result = interceptReuslt.Item2 == null ? default(T) : interceptReuslt.Item2;
                }
                else
                {
                    message = await _breakeRemoteInvokeService.InvokeAsync(parameters, serviceId, _serviceKey, decodeJOject);
                    if (message == null || !message.IsSucceedRemoteInvokeCalled())
                    {
                        return await FallBackRetryInvoke<T>(parameters, serviceId, command);
                    }
                }

            }
            if (existsInterceptor)
            {
                invocation = invocation == null ? GetInvocation(parameters, serviceId, typeof(T)) : invocation;
                foreach (var interceptor in _interceptors)
                {
                    var interceptReuslt = await Intercept(interceptor, invocation);
                    message = interceptReuslt.Item1;
                    result = interceptReuslt.Item2 == null ? default(T) : interceptReuslt.Item2;
                }
            }
            if (message != null)
            {
                result = await GetInvokeResult<T>(message);
            }
            return (T)result;
        }

        /// <summary>
        /// 远程调用。
        /// </summary>
        /// <param name="parameters">参数字典。</param>
        /// <param name="serviceId">服务Id。</param>
        /// <returns>调用任务。</returns>
        protected async Task Invoke(IDictionary<string, object> parameters, string serviceId)
        {
            var existsInterceptor = _interceptors.Any();
            RemoteInvokeResultMessage message = null;
            if (!existsInterceptor)
                message = await _breakeRemoteInvokeService.InvokeAsync(parameters, serviceId, _serviceKey, false);
            else
            {
                var invocation = GetInvocation(parameters, serviceId, typeof(Task));
                foreach (var interceptor in _interceptors)
                {
                    var interceptReuslt = await Intercept(interceptor, invocation);
                    message = interceptReuslt.Item1;
                }
            }
            if (message == null || !message.IsSucceedRemoteInvokeCalled())
            {
                var command = await _commandProvider.GetCommand(serviceId);
                await FallBackRetryInvoke(parameters, serviceId, command);
            }
        }

        public async Task<object> CallInvoke(IInvocation invocation)
        {
            var cacheInvocation = invocation as ICacheInvocation;
            var parameters = invocation.Arguments;
            var serviceId = invocation.ServiceId;
            var type = invocation.ReturnType;
            var message = await _breakeRemoteInvokeService.InvokeAsync(parameters, serviceId, _serviceKey, type == typeof(Task) ? false : true);
            if (message == null || !message.IsSucceedRemoteInvokeCalled())
            {
                var command = await _commandProvider.GetCommand(serviceId);
                return await CallInvokeBackFallBackRetryInvoke(parameters, serviceId, command, type, type == typeof(Task) ? false : true);
            }
            if (type == typeof(Task)) return message;
            return await GetInvokeResult(message, invocation.ReturnType);
        }

        private async Task<Tuple<RemoteInvokeResultMessage, object>> Intercept(IInterceptor interceptor, IInvocation invocation)
        {
            await interceptor.Intercept(invocation);
            return new Tuple<RemoteInvokeResultMessage, object>(invocation.RemoteInvokeResultMessage, invocation.ReturnValue);
        }

        private IInvocation GetInvocation(IDictionary<string, object> parameters, string serviceId, Type returnType)
        {
            var invocation = _serviceProvider.GetInstances<IInterceptorProvider>();
            return invocation.GetInvocation(this, parameters, serviceId, returnType);
        }

        private IInvocation GetCacheInvocation(IDictionary<string, object> parameters, string serviceId, Type returnType)
        {
            var invocation = _serviceProvider.GetInstances<IInterceptorProvider>();
            return invocation.GetCacheInvocation(this, parameters, serviceId, returnType);
        }

        private Task<T> GetInvokeResult<T>(RemoteInvokeResultMessage message)
        {
            return Task.Run(() =>
            {
                object result = default(T);
                if (message.StatusCode == StatusCode.Success)
                {
                    if (message.Result != null)
                    {
                        result = _typeConvertibleService.Convert(message.Result, typeof(T));
                    }
                }
                else
                {
                    throw message.GetExceptionByStatusCode();
                }

                return (T) result;
            });
        }

        private Task<object> GetInvokeResult(RemoteInvokeResultMessage message, Type returnType)
        {
            return Task.Run(() =>
            {
                object result;
                if (message.StatusCode == StatusCode.Success)
                {
                    if (message.Result != null)
                    {
                        result = _typeConvertibleService.Convert(message.Result, returnType);
                    }
                    else
                    {
                        result = message.Result;
                    }

                }
                else
                {
                    throw message.GetExceptionByStatusCode();
                }

                return result;

            });
        }

        private async Task<T> FallBackRetryInvoke<T>(IDictionary<string, object> parameters, string serviceId, ServiceCommand command)
        {
            if (command.FallBackName != null && _serviceProvider.IsRegistered<IFallbackInvoker>(command.FallBackName) && command.Strategy == StrategyType.FallBack)
            {
                var invoker = _serviceProvider.GetInstances<IFallbackInvoker>(command.FallBackName);
                return await invoker.Invoke<T>(parameters, serviceId, _serviceKey);
            }
            else
            {
                var invoker = _serviceProvider.GetInstances<IClusterInvoker>(command.Strategy.ToString());
                return await invoker.Invoke<T>(parameters, serviceId, _serviceKey, typeof(T) == UtilityType.ObjectType);
            }
        }

        private async Task<object> CallInvokeBackFallBackRetryInvoke(IDictionary<string, object> parameters, string serviceId, ServiceCommand command,Type returnType,bool decodeJOject)
        {
            if (command.FallBackName != null && _serviceProvider.IsRegistered<IFallbackInvoker>(command.FallBackName) && command.Strategy == StrategyType.FallBack)
            {
                var invoker = _serviceProvider.GetInstances<IFallbackInvoker>(command.FallBackName);
                return await invoker.Invoke(parameters, returnType, serviceId, _serviceKey);
            }
            else
            {
                var invoker = _serviceProvider.GetInstances<IClusterInvoker>(command.Strategy.ToString());
                return await invoker.Invoke(parameters, returnType, serviceId, _serviceKey, decodeJOject);
            }
        }

        private async Task FallBackRetryInvoke(IDictionary<string, object> parameters, string serviceId, ServiceCommand command)
        {
            if (command.FallBackName != null && _serviceProvider.IsRegistered<IFallbackInvoker>(command.FallBackName) && command.Strategy == StrategyType.FallBack)
            {
                var invoker = _serviceProvider.GetInstances<IFallbackInvoker>(command.FallBackName);
                await invoker.Invoke<object>(parameters, serviceId, _serviceKey);
            }
            else
            {
                var invoker = _serviceProvider.GetInstances<IClusterInvoker>(command.Strategy.ToString());
                await invoker.Invoke(parameters, serviceId, _serviceKey, true);
            }
        }

        #endregion Protected Method
    }
}