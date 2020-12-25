namespace Surging.Cloud.Domain.Entities.Auditing
{
    public class FullAuditedAndOrgAggregateRoot : FullAuditedAggregateRoot<int>, IOrgAudited
    {
        public long? OrgId { get; set; }
    }

    public class FullAuditedAndOrgAggregateRoot<TPrimaryKey> : FullAuditedAggregateRoot<TPrimaryKey>, IOrgAudited
    {
        public long? OrgId { get; set; }
    }
}
