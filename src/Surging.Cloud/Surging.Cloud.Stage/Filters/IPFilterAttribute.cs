using Microsoft.AspNetCore.Http;
using Surging.Cloud.ApiGateWay;
using Surging.Cloud.CPlatform.Messages;
using Surging.Cloud.CPlatform.Transport.Implementation;
using Surging.Cloud.KestrelHttpServer.Filters;
using Surging.Cloud.KestrelHttpServer.Filters.Implementation;
using Surging.Cloud.Stage.Internal;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Cloud.Stage.Filters
{
    public class IPFilterAttribute : IActionFilter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIPChecker _ipChecker;
        private const int _order = 9999;
        public IPFilterAttribute(IHttpContextAccessor httpContextAccessor, IIPChecker ipChecker)
        {
            _httpContextAccessor = httpContextAccessor;
            _ipChecker = ipChecker;
        }

        public int Order => _order;

        public Task OnActionExecuted(ActionExecutedContext filterContext)
        {
            return Task.CompletedTask;
        }

        public Task OnActionExecuting(ActionExecutingContext filterContext)
        {
            var address = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress;
            RpcContext.GetContext().SetAttachment("RemoteIpAddress", address.ToString());
            if (_ipChecker.IsBlackIp(address, filterContext.Message.RoutePath))
            {
                filterContext.Result = new HttpResultMessage<object> { IsSucceed = false, StatusCode = CPlatform.Exceptions.StatusCode.UnAuthentication, Message = "Your IP address is not allowed" };
            }
            return Task.CompletedTask;
        }
    }
}
