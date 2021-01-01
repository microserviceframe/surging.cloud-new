using System.Threading.Tasks;
using Surging.Cloud.KestrelHttpServer;
using Surging.Cloud.ProxyGenerator;
using Surging.IModuleServices.Common;

namespace Surging.Modules.Common.Domain
{
    public class FileService : ProxyServiceBase, IFileService
    {
        public async Task<IActionResult> Preview(string fileId)
        {
            var captchaBytes = Utils.CreateCaptcha("xxx111");
            return new ImageResult(captchaBytes,"image/png");
        }
    }
}