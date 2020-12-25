using Surging.Cloud.CPlatform.Module;
using Surging.Cloud.ProxyGenerator.Interceptors;
using Surging.Cloud.ProxyGenerator.Interceptors.Implementation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.ProxyGenerator
{
    public static class  RegistrationExtensions
    {
        public static void AddClientIntercepted(this ContainerBuilderWrapper builder,  Type interceptorServiceType)
        { 
            builder.RegisterType(interceptorServiceType).As<IInterceptor>().SingleInstance();
            builder.RegisterType<InterceptorProvider>().As<IInterceptorProvider>().SingleInstance();
        }

        public static void AddClientIntercepted(this ContainerBuilderWrapper builder, params Type[] interceptorServiceTypes)
        { 
            builder.RegisterTypes(interceptorServiceTypes).As<IInterceptor>().SingleInstance();
            builder.RegisterType<InterceptorProvider>().As<IInterceptorProvider>().SingleInstance();
     
        }
    }
}
