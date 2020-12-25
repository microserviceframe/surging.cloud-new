using Surging.Cloud.Domain.Entities;

namespace Surging.Cloud.Domain.Repositories
{
    public interface IRepository<TEntity> : IRepository<TEntity, int> where TEntity : class, IEntity<int>
    {
    }
}
