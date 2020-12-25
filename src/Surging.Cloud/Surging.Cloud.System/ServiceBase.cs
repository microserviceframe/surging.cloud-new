using Autofac;
using Surging.Cloud.CPlatform.Utilities;
using Surging.Cloud.ProxyGenerator;
using Surging.Cloud.ProxyGenerator.Utilitys;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.System
{
    public  abstract class ServiceBase
    {
        public T CreateProxy<T>(string key) where T : class
        {
            return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy<T>(key);
        }

        public object CreateProxy(Type type)
        {
            return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy(type);
        }

        public object CreateProxy(string key, Type type)
        {
            return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy(key, type);
        }
        public T CreateProxy<T>() where T : class
        {
            return ServiceLocator.GetService<IServiceProxyFactory>().CreateProxy<T>();
        }

        public T GetService<T>()
        {
            return ServiceLocator.GetService<T>();
        }

        public T GetService<T>(string key)
        {
            return ServiceLocator.GetService<T>(key);
        }

        public object GetService(Type type)
        {
            return ServiceLocator.GetService(type);
        }

        public object GetService(string key, Type type)
        {
            return ServiceLocator.GetService(key, type);
        }
    }
}
