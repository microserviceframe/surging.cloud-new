using Microsoft.Extensions.Logging;
using Surging.Core.Caching;
using Surging.Core.CPlatform.Cache;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Runtime.Session;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.ProxyGenerator.Interceptors;
using Surging.Core.ProxyGenerator.Interceptors.Implementation;
using Surging.Core.ProxyGenerator.Interceptors.Implementation.Metadatas;
using System;
using System.Linq;
using System.Threading.Tasks;
using Metadatas = Surging.Core.ProxyGenerator.Interceptors.Implementation.Metadatas;

namespace Surging.Core.System.Intercept
{
    /// <summary>
    /// 缓存拦截器
    /// </summary>
    public class CacheProviderInterceptor : CacheInterceptor
    {
        private readonly IInterceptorProvider _interceptorProvider;
        private readonly IServiceRouteProvider _serviceRouteProvider;
        public CacheProviderInterceptor(IInterceptorProvider interceptorProvider, IServiceRouteProvider serviceRouteProvider)
        {
            _interceptorProvider = interceptorProvider;
            _serviceRouteProvider = serviceRouteProvider;
        }

        public override async Task Intercept(ICacheInvocation invocation)
        {
            try 
            {
                var attribute =
                      invocation.Attributes.Where(p => p is InterceptMethodAttribute)
                      .Select(p => p as InterceptMethodAttribute).FirstOrDefault();
                if (attribute != null)
                {
                    if (!attribute.Key.IsNullOrEmpty() && attribute.Key.Contains("{userId}", StringComparison.OrdinalIgnoreCase))
                    {
                        var loginUser = NullSurgingSession.Instance;
                        if (loginUser == null || !loginUser.UserId.HasValue)
                        {
                            await invocation.Proceed();
                            return;
                        }
                        else
                        {
                            attribute.Key = attribute.Key.Replace("{userId}", loginUser.UserId.Value.ToString());
                        }
                    }
                    if (attribute.CorrespondingKeys != null && attribute.CorrespondingKeys.Any(p => p.Contains("{userId}", StringComparison.OrdinalIgnoreCase)))
                    {
                        var loginUser = NullSurgingSession.Instance;

                        for (var i = 0; i < attribute.CorrespondingKeys.Length; i++)
                        {
                            if (loginUser == null || !loginUser.UserId.HasValue)
                            {
                                attribute.CorrespondingKeys[i] = attribute.CorrespondingKeys[i].Replace("{userId}", "*", StringComparison.OrdinalIgnoreCase);
                            }
                            else
                            {
                                attribute.CorrespondingKeys[i] = attribute.CorrespondingKeys[i].Replace("{userId}", loginUser.UserId.Value.ToString(), StringComparison.OrdinalIgnoreCase);
                            }
                        }
                    }
                    var cacheKey = invocation.CacheKey == null ? attribute.Key :
                          string.Format(attribute.Key ?? "", invocation.CacheKey);
                    var l2CacheKey = invocation.CacheKey == null ? attribute.L2Key :
                          string.Format(attribute.L2Key ?? "", invocation.CacheKey);

                    await CacheIntercept(attribute, cacheKey, invocation, l2CacheKey, attribute.EnableL2Cache);
                }
                else
                {
                    var route = await _serviceRouteProvider.Locate(invocation.ServiceId);
                    var cacheMetadata = route.ServiceDescriptor.GetCacheIntercept("Cache");
                    if (cacheMetadata != null)
                    {
                        var keyValues = _interceptorProvider.GetCacheKeyVaule(invocation.Arguments);
                        if (!cacheMetadata.Key.IsNullOrEmpty() && cacheMetadata.Key.Contains("{userId}", StringComparison.OrdinalIgnoreCase))
                        {
                            var loginUser = NullSurgingSession.Instance;
                            if (loginUser == null || !loginUser.UserId.HasValue)
                            {
                                await invocation.Proceed();
                                return;
                            }
                            else
                            {
                                cacheMetadata.Key = cacheMetadata.Key.Replace("{userId}", loginUser.UserId.Value.ToString());
                            }
                        }
                        if (cacheMetadata.CorrespondingKeys != null && cacheMetadata.CorrespondingKeys.Any(p => p.Contains("{userId}", StringComparison.OrdinalIgnoreCase)))
                        {
                            var loginUser = NullSurgingSession.Instance;

                            for (var i = 0; i < cacheMetadata.CorrespondingKeys.Length; i++)
                            {
                                if (loginUser == null || !loginUser.UserId.HasValue)
                                {
                                    cacheMetadata.CorrespondingKeys[i] = cacheMetadata.CorrespondingKeys[i].Replace("{userId}", "*", StringComparison.OrdinalIgnoreCase);
                                }
                                else
                                {
                                    cacheMetadata.CorrespondingKeys[i] = cacheMetadata.CorrespondingKeys[i].Replace("{userId}", loginUser.UserId.Value.ToString(), StringComparison.OrdinalIgnoreCase);
                                }
                            }
                        }
                        var cacheKey = keyValues == null ? cacheMetadata.Key :
                            string.Format(cacheMetadata.Key ?? "", keyValues);
                        var l2CacheKey = keyValues == null ? cacheMetadata.L2Key :
                             string.Format(cacheMetadata.L2Key ?? "", keyValues);
                        await CacheIntercept(cacheMetadata, cacheKey, invocation, l2CacheKey, cacheMetadata.EnableL2Cache);
                    }
                    else 
                    {
                        await invocation.Proceed();
                    }
                }
            } catch (Exception ex) 
            {
                var logger = ServiceLocator.GetService<ILogger<CacheProviderInterceptor>>();
                logger.LogWarning($"使用缓存拦截失败,原因:{ex.Message}");
                await invocation.Proceed();
            }
            
        }

        private async Task CacheIntercept(Metadatas.ServiceCacheIntercept cacheMetadata, string key, ICacheInvocation invocation, string l2Key, bool enableL2Cache)
        {
            ICacheProvider cacheProvider = null;
            switch (cacheMetadata.Mode)
            {
                case CacheTargetType.Redis:
                    {
                        cacheProvider = CacheContainer.GetService<ICacheProvider>(string.Format("{0}.{1}",
                           cacheMetadata.CacheSectionType.ToString(), CacheTargetType.Redis.ToString()));
                        break;
                    }
                case CacheTargetType.MemoryCache:
                    {
                        cacheProvider = CacheContainer.GetService<ICacheProvider>(CacheTargetType.MemoryCache.ToString());
                        break;
                    }
            }
            if (cacheProvider != null && !enableL2Cache)
            {
                await Invoke(cacheProvider, cacheMetadata, key, invocation);
            }
            else if (cacheProvider != null && enableL2Cache)
            {
                var l2CacheProvider = CacheContainer.GetService<ICacheProvider>(CacheTargetType.MemoryCache.ToString());
                if (l2CacheProvider != null) await Invoke(cacheProvider, l2CacheProvider, l2Key, cacheMetadata, key, invocation);
            }
        }

        private async Task CacheIntercept(InterceptMethodAttribute attribute, string key, ICacheInvocation invocation, string l2Key, bool enableL2Cache)
        {
            ICacheProvider cacheProvider = null;
            switch (attribute.Mode)
            {
                case CacheTargetType.Redis:
                    {
                        cacheProvider = CacheContainer.GetService<ICacheProvider>(string.Format("{0}.{1}",
                           attribute.CacheSectionType.ToString(), CacheTargetType.Redis.ToString()));
                        break;
                    }
                case CacheTargetType.MemoryCache:
                    {
                        cacheProvider = CacheContainer.GetService<ICacheProvider>(CacheTargetType.MemoryCache.ToString());
                        break;
                    }
            }
            if (cacheProvider != null && !enableL2Cache)
            {
                await Invoke(cacheProvider, attribute, key, invocation);
            }
            else if (cacheProvider != null && enableL2Cache)
            {
                var l2CacheProvider = CacheContainer.GetService<ICacheProvider>(CacheTargetType.MemoryCache.ToString());
                if (l2CacheProvider != null) await Invoke(cacheProvider, l2CacheProvider, l2Key, attribute, key, invocation);
            }
        }


        private async Task Invoke(ICacheProvider cacheProvider, ServiceCacheIntercept cacheMetadata, string key, ICacheInvocation invocation)
        {

            switch (cacheMetadata.Method)
            {
                case CachingMethod.Get:
                    {

                        var retrunValue = await cacheProvider.GetFromCacheFirst(key, async () =>
                        {
                            await invocation.Proceed();
                            if (invocation.RemoteInvokeResultMessage.StatusCode == CPlatform.Exceptions.StatusCode.Success)
                            {
                                return invocation.RemoteInvokeResultMessage.Result;
                            }
                            else
                            {
                                throw invocation.RemoteInvokeResultMessage.GetExceptionByStatusCode();
                            }

                        }, invocation.ReturnType, cacheMetadata.Time);
                        invocation.ReturnValue = retrunValue;
                        invocation.RemoteInvokeResultMessage = new RemoteInvokeResultMessage() { Result = retrunValue };
                        break;
                    }
                default:
                    {
                        await invocation.Proceed();
                        var keys = cacheMetadata.CorrespondingKeys.Select(correspondingKey => string.Format(correspondingKey, invocation.CacheKey)).ToList();
                        keys.ForEach(key => {
                            cacheProvider.RemoveAsync(key);
                        });
                        break;
                    }
            }
        }

       

        private async Task Invoke(ICacheProvider cacheProvider, ICacheProvider l2CacheProvider, string l2Key, ServiceCacheIntercept cacheMetadata, string key, ICacheInvocation invocation)
        {
            switch (cacheMetadata.Method)
            {
                case CachingMethod.Get:
                    {
                        var retrunValue = await cacheProvider.GetFromCacheFirst(l2CacheProvider, l2Key, key, async () =>
                        {
                            await invocation.Proceed();
                            var remoteInvokeResultMessage = invocation.RemoteInvokeResultMessage;
                            if (remoteInvokeResultMessage.StatusCode == CPlatform.Exceptions.StatusCode.Success)
                            {
                                return remoteInvokeResultMessage.Result;
                            }
                            else
                            {
                                throw remoteInvokeResultMessage.GetExceptionByStatusCode();
                            }

                        }, invocation.ReturnType, cacheMetadata.Time);
                        //invocation.ReturnValue = retrunValue;
                        break;
                    }
                default:
                    {
                        await invocation.Proceed();
                        var keys = cacheMetadata.CorrespondingKeys.Select(correspondingKey => string.Format(correspondingKey, invocation.CacheKey)).ToList();
                        keys.ForEach(cacheProvider.Remove);
                        break;
                    }
            }
        }

      
       
        private async Task Invoke(ICacheProvider cacheProvider, InterceptMethodAttribute attribute, string key, ICacheInvocation invocation)
        {

            switch (attribute.Method)
            {
                case CachingMethod.Get:
                    {
                        
                        var retrunValue = await cacheProvider.GetFromCacheFirst(key, async () =>
                        {
                            await invocation.Proceed();
                            if (invocation.RemoteInvokeResultMessage.StatusCode == CPlatform.Exceptions.StatusCode.Success)
                            {
                                return invocation.RemoteInvokeResultMessage.Result;
                            }
                            else
                            {
                                throw invocation.RemoteInvokeResultMessage.GetExceptionByStatusCode();
                            }

                        }, invocation.ReturnType, attribute.Time);
                        invocation.ReturnValue = retrunValue;
                        invocation.RemoteInvokeResultMessage = new RemoteInvokeResultMessage() { Result = retrunValue };
                        break;
                    }
                default:
                    {
                        await invocation.Proceed();
                        var keys = attribute.CorrespondingKeys.Select(correspondingKey => string.Format(correspondingKey, invocation.CacheKey)).ToList();
                        keys.ForEach(key => {
                            cacheProvider.RemoveAsync(key);
                        });
                        break;
                    }
            }
        }


        private async Task Invoke(ICacheProvider cacheProvider, ICacheProvider l2cacheProvider, string l2Key, InterceptMethodAttribute attribute, string key, ICacheInvocation invocation)
        {

            switch (attribute.Method)
            {
                case CachingMethod.Get:
                    {
                        var retrunValue = await cacheProvider.GetFromCacheFirst(l2cacheProvider, l2Key, key, async () =>
                         {
                             await invocation.Proceed();
                             var remoteInvokeResultMessage = invocation.RemoteInvokeResultMessage;
                             if (remoteInvokeResultMessage.StatusCode == CPlatform.Exceptions.StatusCode.Success)
                             {
                                 return remoteInvokeResultMessage.Result;
                             }
                             else 
                             {
                                 throw remoteInvokeResultMessage.GetExceptionByStatusCode();
                             }

                         }, invocation.ReturnType, attribute.Time);
                        //invocation.ReturnValue = retrunValue;
                        break;
                    }
                default:
                    {
                        await invocation.Proceed();
                        var keys = attribute.CorrespondingKeys.Select(correspondingKey => string.Format(correspondingKey, invocation.CacheKey)).ToList();
                        keys.ForEach(cacheProvider.Remove);
                        break;
                    }
            }
        }
    }
}
