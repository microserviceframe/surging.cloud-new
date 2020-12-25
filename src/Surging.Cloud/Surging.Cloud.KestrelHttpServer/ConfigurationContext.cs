using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Surging.Cloud.CPlatform.Module;
using Surging.Cloud.CPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.KestrelHttpServer
{
    public class ConfigurationContext
    {
        public ConfigurationContext( IServiceCollection services, 
            List<AbstractModule> modules,
            string[] virtualPaths,
           IConfigurationRoot configuration)
        {
            Services = Check.NotNull(services, nameof(services));
            Modules = Check.NotNull(modules, nameof(modules));
            VirtualPaths = Check.NotNull(virtualPaths, nameof(virtualPaths));
            Configuration = Check.NotNull(configuration, nameof(configuration));
        }

        public IConfigurationRoot Configuration { get; }
        public IServiceCollection Services { get; }

        public List<AbstractModule> Modules { get; }

        public string[] VirtualPaths { get; }
    }
}
