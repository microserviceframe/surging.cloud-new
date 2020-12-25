namespace Surging.Cloud.Domain.Entities.Auditing
{
    public interface IAudited : ICreationAudited, IModificationAudited
    {
    }

    public interface IAudited<TUser> : IAudited, ICreationAudited<TUser>, IModificationAudited<TUser>
        where TUser : IEntity<long>
    {
    }
}
