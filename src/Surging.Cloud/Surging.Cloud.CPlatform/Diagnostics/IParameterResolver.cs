using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.CPlatform.Diagnostics
{
    public interface IParameterResolver
    {
        object Resolve(object value);
    }
}
