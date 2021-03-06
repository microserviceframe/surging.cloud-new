﻿using Surging.Cloud.ApiGateWay.ServiceDiscovery.Implementation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Cloud.ApiGateWay.ServiceDiscovery
{
    public interface IServiceRegisterProvider
    {

        Task<IEnumerable<ServiceAddressModel>> GetAddressAsync(string condition = null);
    }
}
