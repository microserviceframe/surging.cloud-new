using Surging.Cloud.Caching.HashAlgorithms;
using Surging.Cloud.CPlatform.Cache;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Cloud.Caching.AddressResolvers
{
    public interface IAddressResolver
    {
        Task<ConsistentHashNode> Resolver(string cacheId, string item);
    }
}
