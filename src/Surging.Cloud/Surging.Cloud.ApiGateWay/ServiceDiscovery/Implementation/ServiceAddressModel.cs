using Surging.Cloud.CPlatform.Address;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.ApiGateWay.ServiceDiscovery.Implementation
{
    public class ServiceAddressModel 
    {
        public AddressModel Address { get; set; }

        public  bool IsHealth { get; set; }
    }
}
