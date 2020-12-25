using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.ApiGateWay.OAuth
{
    public class ConfigInfo
    {
        public ConfigInfo(string authorizationRoutePath):this(authorizationRoutePath,null, 24)
        {

        }
        
        public ConfigInfo(string authorizationRoutePath,string authorizationServiceKey, int defaultExpired)
        {
            AuthorizationServiceKey = authorizationServiceKey;
            AuthorizationRoutePath = authorizationRoutePath;
            DefaultExpired = defaultExpired;
        }
        public string AuthorizationServiceKey { get; set; }
        /// <summary>
        /// 授权服务路由地址
        /// </summary>
        public string AuthorizationRoutePath { get; set; }
        /// <summary>
        /// token 有效期
        /// </summary>
        public int DefaultExpired { get; set; } = 24;
    };
}
