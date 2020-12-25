using Surging.Cloud.Domain.Entities.Auditing;

namespace Surging.IModuleServices.Common.Models
{
    public class PermissionData : FullAuditedEntity<long>, IOrgAudited
    {
        public string UserName { get; set; }

        public string Address { get; set; }
        
        public long? OrgId { get; set; }
    }
}