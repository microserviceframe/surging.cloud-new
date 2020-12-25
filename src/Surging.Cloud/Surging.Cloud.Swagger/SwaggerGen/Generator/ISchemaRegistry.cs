using Surging.Cloud.Swagger;
using System;
using System.Collections.Generic;
namespace Surging.Cloud.SwaggerGen
{
    public interface ISchemaRegistry
    {
        Schema GetOrRegister(Type type);

        Schema GetOrRegister(string parmName, Type type);

        IDictionary<string, Schema> Definitions { get; }
    }
}
