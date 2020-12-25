using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Surging.Cloud.SwaggerGen
{
    public class SwaggerApplicationConvention : IApplicationModelConvention
    {
        public void Apply(ApplicationModel application)
        {
            application.ApiExplorer.IsVisible = true;
        }
    }
}