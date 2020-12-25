using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.Consul.WatcherProvider
{
    public interface IClientWatchManager
    {
        Dictionary<string, HashSet<Watcher>> DataWatches { get; set; }
    }
}
