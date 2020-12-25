namespace Surging.Cloud.Swagger
{
    public class BasicAuthScheme : SecurityScheme
    {
        public BasicAuthScheme()
        {
            Type = "basic";
        }
    }
}
