﻿using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Runtime.Server;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform;
using Surging.Core.CPlatform.Utilities;
using Autofac;
using Surging.Core.CPlatform.Exceptions;
using System.Web;

namespace Surging.Core.KestrelHttpServer.Filters.Implementation
{
   public class HttpRequestFilterAttribute : IActionFilter
    {
        internal const string Http405EndpointDisplayName = "405 HTTP Method Not Supported";
        internal const StatusCode Http405EndpointStatusCode = StatusCode.Http405EndpointStatusCode;
        private readonly IServiceRouteProvider _serviceRouteProvider;
        private readonly IServiceEntryLocate _serviceEntryLocate;
        private const int _order = 9998;
        public HttpRequestFilterAttribute()
        {
            _serviceRouteProvider = ServiceLocator.Current.Resolve<IServiceRouteProvider>(); ;
            _serviceEntryLocate = ServiceLocator.Current.Resolve<IServiceEntryLocate>(); ;
        }

        public int Order => _order;

        public Task OnActionExecuted(ActionExecutedContext filterContext)
        {
            return Task.CompletedTask;
        }

        public  async Task OnActionExecuting(ActionExecutingContext filterContext)
        { 
            var serviceEntry= _serviceEntryLocate.Locate(filterContext.Message);
            if (serviceEntry != null)
            {
                var httpMethods = serviceEntry.Methods;
                if (httpMethods.Count()>0 && !httpMethods.Any(p => String.Compare(p, filterContext.Context.Request.Method, true) == 0))
                {
                    filterContext.Result = new HttpResultMessage<object>
                    {
                        IsSucceed = false,
                        StatusCode = Http405EndpointStatusCode,
                        Message = Http405EndpointDisplayName
                    };
                }
            }
            else
            {
                var path = HttpUtility.UrlDecode(GetRoutePath(filterContext.Context.Request.Path.ToString()));
                var serviceRoute = await _serviceRouteProvider.GetRouteByPathOrRegexPath(path);
                if (serviceRoute == null)
                {
                    serviceRoute = await _serviceRouteProvider.GetRouteByPathOrRegexPath(filterContext.Message.RoutePath);

                    if (serviceRoute == null) 
                    {
                        throw new CPlatformException("Routing path not found", StatusCode.Http404EndpointStatusCode);
                    }
                }

                var httpMethods = serviceRoute.ServiceDescriptor.HttpMethod();
                if (!string.IsNullOrEmpty(httpMethods) && !httpMethods.Contains(filterContext.Context.Request.Method))
                {
                    filterContext.Result = new HttpResultMessage<object>
                    {
                        IsSucceed = false,
                        StatusCode = Http405EndpointStatusCode,
                        Message = Http405EndpointDisplayName
                    };
                }
            }
        }

        private string GetRoutePath(string path)
        {
            string routePath = "";
            var urlSpan = path.AsSpan();
            var len = urlSpan.IndexOf("?");
            if (urlSpan.LastIndexOf("/") == 0)
            {
                routePath = path;
            }
            else
            {
                if (len == -1)
                    routePath = urlSpan.TrimStart("/").ToString().ToLower();
                else
                    routePath = urlSpan.Slice(0, len).TrimStart("/").ToString().ToLower();
            }
            return routePath;
        }
    }
}
