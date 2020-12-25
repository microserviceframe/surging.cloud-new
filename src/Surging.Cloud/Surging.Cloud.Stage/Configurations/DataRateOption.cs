using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.Stage.Configurations
{
    public class DataRateOption
    { 
        public double BytesPerSecond { get; set; } 
        public TimeSpan GracePeriod { get; set; }
    }
}
