using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.Zookeeper.Configurations
{
    public class ZookeeperOption
    {

        public double SessionTimeout { get; set; } = 20;

        public double ConnectionTimeout { get; set; } = 10;

        public double OperatingTimeout { get; set; } = 20;

        public string ConnectionString { get; set; }

        public string RoutePath { get; set; }

        public string SubscriberPath { get; set; }

        public string CommandPath { get; set; }

        public string ChRoot { get; set; }

        public string CachePath { get; set; }

        public string MqttRoutePath { get; set; }

        public string ReloadOnChange { get; set; }

        public string EnableChildrenMonitor { get; set; }
        
    }
}
