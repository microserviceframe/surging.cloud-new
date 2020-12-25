using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.Swagger.Internal
{
   public interface IServiceSchemaProvider
    {
        IEnumerable<string> GetSchemaFilesPath();
    }
}
