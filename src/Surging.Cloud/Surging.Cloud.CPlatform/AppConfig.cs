using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Surging.Cloud.CPlatform.Configurations;
using Surging.Cloud.CPlatform.DependencyResolution;
using Surging.Cloud.CPlatform.Runtime.Client.Address.Resolvers.Implementation.Selectors.Implementation;
using Surging.Cloud.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Surging.Cloud.CPlatform
{
    public class AppConfig
    {
        #region 字段
        private static AddressSelectorMode _loadBalanceMode = AddressSelectorMode.Polling;
        private static SurgingServerOptions _serverOptions = new SurgingServerOptions();
        private static IEnumerable<MapRoutePathOption> _mapRoutePathOptions = new List<MapRoutePathOption>();
        #endregion

        public static IConfigurationRoot Configuration { get; internal set; }

        /// <summary>
        /// 负载均衡模式
        /// </summary>
        public static AddressSelectorMode LoadBalanceMode
        {
            get
            {
                AddressSelectorMode mode = _loadBalanceMode; ;
                if (Configuration != null
                    && Configuration["AccessTokenExpireTimeSpan"] != null
                    && !Enum.TryParse(Configuration["AccessTokenExpireTimeSpan"], out mode))
                {
                    mode = _loadBalanceMode;
                }
                return mode;
            }
            internal set
            {
                _loadBalanceMode = value;
            }
        }

        public static IConfigurationSection GetSection(string name)
        {
            return Configuration?.GetSection(name);
        }


        public static SurgingServerOptions ServerOptions
        {
            get
            {
                return _serverOptions;
            }
            internal set
            {
                _serverOptions = value;
            }
        }


        public static IEnumerable<MapRoutePathOption> MapRoutePathOptions
        {
            get
            {
                return _mapRoutePathOptions;
            }
            internal set 
            {
                _mapRoutePathOptions = value;
            }
        
        }

        public static string PayloadKey { get; } = "payload";
    }
}
