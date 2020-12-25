using Surging.Cloud.Protocol.Mqtt.Internal.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Cloud.Protocol.Mqtt.Internal.Services
{
    public interface IWillService
    {
        void Add(string deviceid, MqttWillMessage willMessage);

        Task SendWillMessage(string deviceId);

        void Remove(string deviceid);
    }
}
