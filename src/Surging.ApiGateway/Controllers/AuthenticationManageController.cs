using Microsoft.AspNetCore.Mvc;
using Surging.Cloud.ApiGateWay.ServiceDiscovery;
using Surging.Cloud.ApiGateWay.Utilities;
using Surging.Cloud.CPlatform.Address;
using Surging.Cloud.CPlatform.Utilities;
using System.Linq;
using System.Threading.Tasks;

namespace Surging.ApiGateway.Controllers
{
    public class AuthenticationManageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> EditServiceToken([FromServices]IServiceDiscoveryProvider serviceDiscoveryProvider, string address)
        {
            var list = await serviceDiscoveryProvider.GetAddressAsync(address); ;
            return View(list.FirstOrDefault());
        }

        [HttpPost]
        public async Task<IActionResult> EditServiceToken([FromServices]IServiceDiscoveryProvider serviceDiscoveryProvider, IpAddressModel model)
        {
           await serviceDiscoveryProvider.EditServiceToken(model);
            return Json(ServiceResult.Create(true));
        }
    }
}
