using Surging.Cloud.CPlatform.Address;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Cloud.Protocol.Mqtt.Internal.Runtime
{
   public interface IMqttBrokerEntryManger
    {
        ValueTask<IEnumerable<AddressModel>> GetMqttBrokerAddress(string topic);

        Task CancellationReg(string topic,AddressModel addressModel);

        Task Register(string topic, AddressModel addressModel);
    }
}
