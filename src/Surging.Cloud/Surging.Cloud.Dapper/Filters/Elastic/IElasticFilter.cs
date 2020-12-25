using Surging.Cloud.CPlatform.Ioc;
using Surging.Cloud.Domain.Entities;
using System;

namespace Surging.Cloud.Dapper.Filters.Elastic
{
    public interface IElasticFilter<TEntity, TPrimaryKey> : ITransientDependency where TEntity : class, IEntity<TPrimaryKey>
    {
        bool ExecuteFilter(TEntity entity);

        Exception ElasticException { get; }
    }
}
