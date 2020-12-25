using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Cloud.CPlatform.Configurations.Watch
{
    public abstract class ConfigurationWatch
    {
        protected ConfigurationWatch()
        {
        }

        public abstract Task Process();
    }
}
