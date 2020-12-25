using Surging.Cloud.CPlatform.Utilities;
using Surging.Cloud.Protocol.Mqtt.Internal.Enums;
using Surging.Cloud.Protocol.Mqtt.Internal.Messages;
using Surging.Cloud.Protocol.Mqtt.Internal.Services;
using Surging.Cloud.ProxyGenerator;
using Surging.IModuleServices.Common;
using Surging.IModuleServices.Common.Models;
using Surging.IModuleServices.User;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Modules.Common.Domain
{
    public class ControllerService : MqttBehavior, IControllerService
    {
        public override async Task<bool> Authorized(string username, string password)
        {
            bool result = false;
            if (username == "admin" && password == "123456")
                result= true;
            return await Task.FromResult(result);
        }

       public async Task<bool> IsOnline(string deviceId)
        {
            var text = await GetService<IManagerService>().SayHello("fanly");
            return  await base.GetDeviceIsOnine(deviceId);
        }

        public async Task Publish(string deviceId, WillMessage message)
        {
            var willMessage = new MqttWillMessage
            {
                WillMessage = message.Message,
                Qos = message.Qos,
                Topic = message.Topic,
                WillRetain = message.WillRetain
            };
            await Publish(deviceId, willMessage);
            await RemotePublish(deviceId, willMessage);
        }
    }
}
