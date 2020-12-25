
using Surging.Cloud.CPlatform.Ioc;
using Surging.Cloud.Domain.Entities;

namespace Surging.Cloud.Dapper.Filters.Action
{
    public interface IAuditActionFilter<TEntity, TPrimaryKey> : ITransientDependency where TEntity : class, IEntity<TPrimaryKey>
    {
        void ExecuteFilter(TEntity entity);

    }
}
