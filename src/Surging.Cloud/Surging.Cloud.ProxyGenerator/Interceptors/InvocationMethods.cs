using Microsoft.Extensions.Logging;
using Surging.Cloud.ProxyGenerator.Interceptors.Implementation;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Surging.Cloud.ProxyGenerator.Interceptors
{
    public class InvocationMethods
    {
        public static readonly ConstructorInfo CompositionInvocationConstructor =
        typeof(ActionInvocation).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null,
                                                     new[]
                                                     {
                                                             typeof(IDictionary<string, object>),
                                                             typeof(string),
                                                             typeof(string[]),
                                                             typeof(List<Attribute>),
                                                             typeof(Type),
                                                             typeof(object)
                                                     },
                                                     null);


    }
}
