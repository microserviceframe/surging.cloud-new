using System;

namespace Surging.Cloud.Domain.Entities.Auditing
{
    [Serializable]
    public abstract class FullAuditedAndOrgEntity : FullAuditedEntity, IOrgAudited
    {
        public long? OrgId { get; set; }
    }

    [Serializable]
    public abstract class FullAuditedAndOrgEntity<TPrimaryKey> : FullAuditedEntity<TPrimaryKey>, IOrgAudited
    {
        public long? OrgId { get; set; }
    }
}
