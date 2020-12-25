using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.CPlatform.Cache
{
   public  interface ICacheNodeProvider
    {
        IEnumerable<ServiceCache> GetServiceCaches();
    }
}
