using System.Collections.Generic;
using System.Linq;

namespace Surging.Cloud.CPlatform.Configurations
{
    public class MapRoutePathOption
    {
        public string SourceRoutePath { get; set; }

        public string TargetRoutePath { get; set; }

        public string HttpMethod { get; set; } = "POST";
    }

    public static class MapRoutePathOptionExtensions 
    {
        public static string GetRoutePath(this IEnumerable<MapRoutePathOption> mapRoutePaths, string path,string method) 
        {
            if (mapRoutePaths == null || !mapRoutePaths.Any()) 
            {
                return path;
            }
            if (mapRoutePaths.Any(p => p.TargetRoutePath == path && p.HttpMethod == method)) 
            {
                return mapRoutePaths.First(p => p.TargetRoutePath == path && p.HttpMethod == method).SourceRoutePath;
            }
            return path;
        }
    }
}
