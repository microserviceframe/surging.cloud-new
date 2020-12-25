using Surging.Cloud.Domain.PagedAndSorted;

namespace Surging.IModuleServices.Common.Models
{
    public class QueryPermissionData : PagedResultRequestDto
    {
        public string UserName
        {
            get;
            set;
        }

        public string Address { get; set; }
    }
}