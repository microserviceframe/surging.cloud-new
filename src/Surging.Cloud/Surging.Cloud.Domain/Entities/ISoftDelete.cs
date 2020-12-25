
namespace Surging.Cloud.Domain.Entities
{
    public interface ISoftDelete
    {
        int IsDeleted { get; set; }
    }
}
