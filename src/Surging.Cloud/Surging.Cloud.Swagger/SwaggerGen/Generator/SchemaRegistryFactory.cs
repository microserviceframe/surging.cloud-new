using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Surging.Cloud.SwaggerGen
{
    public class SchemaRegistryFactory : ISchemaRegistryFactory
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly SchemaRegistryOptions _schemaRegistryOptions;

        public SchemaRegistryFactory(
            IOptions<JsonSerializerSettings> mvcJsonOptionsAccessor,
            IOptions<SchemaRegistryOptions> schemaRegistryOptionsAccessor)
            : this(mvcJsonOptionsAccessor.Value, schemaRegistryOptionsAccessor.Value)
        { }

        public SchemaRegistryFactory(
            JsonSerializerSettings jsonSerializerSettings,
            SchemaRegistryOptions schemaRegistryOptions)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
            _schemaRegistryOptions = schemaRegistryOptions;
        }

        public ISchemaRegistry Create()
        {
            return new SchemaRegistry( _jsonSerializerSettings, _schemaRegistryOptions);
        }
    }
}
