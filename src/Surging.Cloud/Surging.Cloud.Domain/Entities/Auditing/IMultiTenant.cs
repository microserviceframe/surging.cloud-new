namespace Surging.Cloud.Domain.Entities.Auditing
{
    public interface IMultiTenant
    {
        long?  TenantId { get; set; }
    }
}