using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Cloud.CPlatform.Diagnostics
{
    public abstract class ParameterBinder : Attribute, IParameterResolver
    {
        public abstract object Resolve(object value);
    }
}
