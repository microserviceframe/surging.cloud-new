using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Surging.Cloud.Swagger
{
    public class SwaggerSerializerFactory
    {
        public static JsonSerializer Create()
        {
            // TODO: Should this handle case where mvcJsonOptions.Value == null?
            return new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented,
                ContractResolver = new SwaggerContractResolver(new JsonSerializerSettings(){ DateFormatString = "yyyy-MM-dd HH:mm:ss", ContractResolver = new CamelCasePropertyNamesContractResolver()})
            };
        }
    }
}
