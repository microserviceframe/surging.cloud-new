using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Surging.Cloud.CPlatform.Support.Implementation
{
    public abstract class ServiceCommandBase: IServiceCommandProvider
    { 
        ConcurrentDictionary<string,  object> scripts = new ConcurrentDictionary<string,  object>();
        public abstract Task<ServiceCommand> GetCommand(string serviceId);
        public abstract Task<IEnumerable<ServiceCommand>> GetCommands(string serviceAppName);
       

        public async Task<object> Run(string text, params string[] InjectionNamespaces)
        {
            object result = scripts;
            var scriptOptions = ScriptOptions.Default.WithImports("System.Threading.Tasks");
            if (InjectionNamespaces != null)
            {
                foreach (var injectionNamespace in InjectionNamespaces)
                {
                    scriptOptions = scriptOptions.WithReferences(injectionNamespace);
                }
            }
            if (!scripts.ContainsKey(text))
            {
                result = scripts.GetOrAdd(text, await CSharpScript.EvaluateAsync(text, scriptOptions));
            }
            else
            {
                scripts.TryGetValue(text, out result);
            }
            return result;
        }
    }
}
