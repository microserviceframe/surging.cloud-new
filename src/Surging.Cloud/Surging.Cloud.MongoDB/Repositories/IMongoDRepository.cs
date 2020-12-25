using Surging.Cloud.Domain.Entities;
using System.Linq;

namespace Surging.Cloud.MongoDb.Repositories
{
    public interface IMongoDRepository<TEntity, TPrimaryKey> where TEntity : class, IEntity<TPrimaryKey>
    {
        IQueryable<TEntity> GetAll();

        TEntity Get(TPrimaryKey id);

        TEntity FirstOrDefault(TPrimaryKey id);

        TEntity Insert(TEntity entity);

        TEntity Update(TEntity entity);

        void Delete(TEntity entity);

        void Delete(TPrimaryKey id);
    }
}
