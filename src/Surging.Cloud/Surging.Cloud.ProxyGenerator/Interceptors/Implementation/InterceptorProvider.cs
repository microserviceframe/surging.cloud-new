using Surging.Cloud.CPlatform.Runtime.Server;
using System.Collections.Generic;
using System.Linq;
using Surging.Cloud.CPlatform;
using System.Reflection;
using System.Collections;
using Surging.Cloud.ProxyGenerator.Utilitys;
using System.Collections.Concurrent;
using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Surging.Cloud.CPlatform.Serialization;
using Surging.Cloud.CPlatform.Utilities;
using Surging.Cloud.CPlatform.Intercept;

namespace Surging.Cloud.ProxyGenerator.Interceptors.Implementation
{
    public class InterceptorProvider : IInterceptorProvider
    {
        private readonly IServiceEntryManager _serviceEntryManager;
        private readonly ISerializer<string> _serializer;
        ConcurrentDictionary<Tuple<Type, Type>,bool> _derivedTypes = new ConcurrentDictionary<Tuple<Type, Type>, bool>();
        private readonly ILogger<InterceptorProvider> _logger;
        public InterceptorProvider(IServiceEntryManager serviceEntryManager)
        {
            _serviceEntryManager = serviceEntryManager;
            _serializer = ServiceLocator.GetService<ISerializer<string>>();
            _logger = ServiceLocator.GetService<ILogger<InterceptorProvider>>();
        }
        public IInvocation GetInvocation(object proxy, IDictionary<string, object> parameters,
            string serviceId,Type returnType)
        {
            var constructor = InvocationMethods.CompositionInvocationConstructor;
            return constructor.Invoke(new object[]{
                    parameters,
                    serviceId,
                    null,
                    null,
                    returnType,
                    proxy
                }) as IInvocation;
        }

        public IInvocation GetCacheInvocation(object proxy, IDictionary<string, object> parameters, string serviceId, Type returnType)
        {
            var entry = (from q in _serviceEntryManager.GetAllEntries()
                         let k = q.Attributes
                         where q.Descriptor.Id == serviceId
                         select q).FirstOrDefault();
            if (entry == null) 
            {
                return null;
            }
            var constructor = InvocationMethods.CompositionInvocationConstructor;
            return constructor.Invoke(new object[]{
                    parameters,
                    serviceId,
                    GetKey(parameters,serviceId),
                    entry.Attributes,
                    returnType,
                    proxy
                }) as IInvocation;
        }

        public string[] GetCacheKeyVaule(IDictionary<string, object> parameterValue)
        {
            return this.GetKey(parameterValue);
        }


        private string[] GetKey(IDictionary<string, object> parameters, string serviceId)
        {

            var result = new List<string>();

            var serviceEntry = _serviceEntryManager.GetAllEntries().Single(p => p.Descriptor.Id == serviceId);
            if (serviceEntry.CacheKeys.Any()) 
            {
                foreach (var cacheKey in serviceEntry.CacheKeys) 
                {
                    foreach (var parameter in parameters) 
                    {
                        var paramType = serviceEntry.ParamTypes[parameter.Key];
                        _logger.LogDebug($"{parameter.Key}的数据类型是:{paramType.FullName}");
                        if (paramType is IEnumerable)
                        {
                            continue;
                        }
                        if (paramType.IsClass && paramType != typeof(string))
                        {
                            object parameterValue = null;
                            var runtimeProperties = paramType.GetRuntimeProperties();
                            if (parameter.Value.GetType() == paramType)
                            {
                                _logger.LogDebug($"不需要反序列化");
                                parameterValue = parameter.Value;
                            }
                            else
                            {
                                _logger.LogDebug($"需要反序列化");
                                parameterValue = _serializer.Deserialize(parameter.Value.ToString(), paramType);
                            }
                           
                            var cacheKeyPropertie = runtimeProperties.FirstOrDefault(p => p.Name == cacheKey);
                            if (cacheKeyPropertie != null)
                            {
                                result.Add(cacheKeyPropertie.GetValue(parameterValue).ToString());
                                continue;
                            }
                        }
                        else 
                        {
                            if (parameter.Key.Equals(cacheKey, StringComparison.OrdinalIgnoreCase)) 
                            {
                                result.Add(parameter.Value.ToString());
                                continue;
                            }
                        }
                    }
                }
            }
            return result.ToArray();
        }

        private string[] GetKey(IDictionary<string, object> parameterValue)
        {
            if (parameterValue != null) 
            {
                var param = parameterValue.Values.FirstOrDefault();
                var reuslt = default(string[]);
                if (parameterValue.Count() > 0)
                {
                    reuslt = new string[] { param.ToString() };
                    if (!(param is IEnumerable))
                    {
                        var runtimeProperties = param.GetType().GetRuntimeProperties();
                        var properties = (from q in runtimeProperties
                                          let k = q.GetCustomAttribute<KeyAttribute>()
                                          where k != null
                                          orderby (k as KeyAttribute).SortIndex
                                          select q).ToList();

                        reuslt = properties.Count() > 0 ?
                                  properties.Select(p => p.GetValue(parameterValue.Values.FirstOrDefault()).ToString()).ToArray() : reuslt;
                    }
                }
                return reuslt;
            }
            return null;
          
        }


        private bool IsKeyAttributeDerivedType(Type baseType,Type derivedType)
        {
            bool result = false;
            var key = Tuple.Create(baseType, derivedType);
            if (!_derivedTypes.ContainsKey(key))
            {
                result =_derivedTypes.GetOrAdd(key, derivedType.IsSubclassOf(baseType) || derivedType == baseType);
            }
            else
            {
                _derivedTypes.TryGetValue(key, out result);
            }
            return result;
        }
    }
}
