﻿using Consul;
using Surging.Cloud.Consul.Internal.Cluster.Implementation.Selectors.Implementation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Cloud.Consul.Internal
{
    public  interface IConsulClientProvider
    {
        Task<ConsulClient> GetClient();

        Task<IEnumerable<ConsulClient>> GetClients();

        //Task Check();
    }
}
