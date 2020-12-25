using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.DotNettyWSServer.Runtime
{
   public interface IWSServiceEntryProvider
    {
        IEnumerable<WSServiceEntry> GetEntries();
    }
}
