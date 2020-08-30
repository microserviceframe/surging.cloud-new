using Surging.Core.CPlatform.Runtime;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ApiGateWay.Configurations
{
   public class ServiceAggregation
    {
        public string RoutePath { get; set; }

        public HttpMethod HttpMethod { get; set; }

        public string ServiceKey { get; set; }

        public Dictionary<string, object> Params { get; set; }

        public string Key { get; set; }
    }
}
