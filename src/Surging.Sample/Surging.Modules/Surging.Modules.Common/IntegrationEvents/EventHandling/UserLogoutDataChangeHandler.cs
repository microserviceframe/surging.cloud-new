using Surging.Cloud.CPlatform.EventBus.Events;
using Surging.Cloud.CPlatform.Utilities;
using Surging.Cloud.EventBusRabbitMQ.Attributes;
using Surging.IModuleServices.Common;
using Surging.IModuleServices.Common.Models;
using Surging.IModuleServices.Common.Models.Events;
using System;
using System.Threading.Tasks;

namespace Surging.Modules.Common.IntegrationEvents.EventHandling
{
    [QueueConsumer("UserLogoutDateChangeHandler")]
    public class UserLogoutDataChangeHandler : IIntegrationEventHandler<LogoutEvent>
    {
        private readonly IUserService _userService;
        public UserLogoutDataChangeHandler()
        {
            _userService = ServiceLocator.GetService<IUserService>("User");
        }
        public async Task Handle(LogoutEvent @event)
        {
            Console.WriteLine($"消费1。");
            await _userService.Update(int.Parse(@event.UserId), new UserModel()
            {

            });
            Console.WriteLine($"消费1失败。");
            throw new Exception();
        }
    }
}
