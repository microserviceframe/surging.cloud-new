namespace Surging.Cloud.Domain.Entities.Auditing
{
    public interface IFullAudited : IAudited, IDeletionAudited
    {
    }

    public interface IFullAudited<TUser> : IAudited<TUser>, IFullAudited, IDeletionAudited<TUser>
        where TUser : IEntity<long>
    {
    }
}
