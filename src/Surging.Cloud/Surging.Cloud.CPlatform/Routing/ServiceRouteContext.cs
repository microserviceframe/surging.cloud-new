using Surging.Cloud.CPlatform.Address;
using Surging.Cloud.CPlatform.Messages;
using Surging.Cloud.CPlatform.Routing.Implementation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.CPlatform.Routing
{
    public class ServiceRouteContext 
    { 
        public ServiceRoute  Route { get; set; }

        public RemoteInvokeResultMessage ResultMessage { get; set; }

        public RemoteInvokeMessage InvokeMessage { get; set; }
    }
}
