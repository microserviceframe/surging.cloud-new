using Surging.Cloud.CPlatform;
using Surging.Cloud.CPlatform.Runtime.Server;
using Surging.Cloud.Protocol.Mqtt.Internal.Services;
using System.Linq;

namespace Surging.Cloud.Protocol.Mqtt.Internal.Runtime.Implementation
{
    public class DefaultMqttBehaviorProvider : IMqttBehaviorProvider
    {
        #region Field

        private readonly IServiceEntryProvider _serviceEntryProvider;
        private readonly CPlatformContainer _serviceProvider;
        private MqttBehavior _mqttBehavior;

        #endregion Field

        #region Constructor

        public DefaultMqttBehaviorProvider(IServiceEntryProvider serviceEntryProvider,  CPlatformContainer serviceProvider)
        {
            _serviceEntryProvider = serviceEntryProvider;
            _serviceProvider = serviceProvider;
        }

        #endregion Constructor

        public MqttBehavior GetMqttBehavior()
        {
            if (_mqttBehavior == null)
            {
                 _mqttBehavior = _serviceEntryProvider.GetTypes()
                    .Select(type=> _serviceProvider.GetInstances(type) as MqttBehavior ).Where(p=>p!=null).FirstOrDefault(); 
            }
            return _mqttBehavior;
        }
    }
}
