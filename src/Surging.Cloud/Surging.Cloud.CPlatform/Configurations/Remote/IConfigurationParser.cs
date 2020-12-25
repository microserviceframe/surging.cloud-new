using System.Collections.Generic;
using System.IO;

namespace Surging.Cloud.CPlatform.Configurations.Remote
{
    public interface IConfigurationParser
    {
        IDictionary<string, string> Parse(Stream input, string initialContext);
    }
}
