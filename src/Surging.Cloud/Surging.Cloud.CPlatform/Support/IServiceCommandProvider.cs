using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Cloud.CPlatform.Support
{
    public interface IServiceCommandProvider
    {
        Task<ServiceCommand> GetCommand(string serviceId);
        
        Task<IEnumerable<ServiceCommand>> GetCommands(string serviceAppName);
        
        Task<object> Run(string text, params string[] InjectionNamespaces);
    }
}
