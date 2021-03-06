﻿using StackExchange.Redis;
using Surging.Cloud.CPlatform.Cache;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Cloud.Caching.Interfaces
{
    public interface ICacheClient<T>
    {
        IServer GetServer(CacheEndpoint endpoint, int connectTimeout);

        T GetClient(CacheEndpoint endpoint, int connectTimeout);

      
        Task<bool> ConnectionAsync(CacheEndpoint endpoint, int connectTimeout);

    }
}
