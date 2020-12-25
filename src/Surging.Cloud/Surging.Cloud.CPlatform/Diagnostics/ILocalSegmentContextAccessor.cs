using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.CPlatform.Diagnostics
{
    public interface ILocalSegmentContextAccessor
    {
        SegmentContext Context { get; set; }
    }
}
