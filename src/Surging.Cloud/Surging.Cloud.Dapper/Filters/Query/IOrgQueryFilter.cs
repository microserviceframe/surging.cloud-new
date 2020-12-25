using System;
using System.Linq.Expressions;
using Surging.Cloud.Domain.Entities;

namespace Surging.Cloud.Dapper.Filters.Query
{
    public interface IOrgQueryFilter
    {
        Expression<Func<TEntity, bool>> ExecuteFilter<TEntity, TPrimaryKey>(Expression<Func<TEntity, bool>> predicate = null) where TEntity : class, IEntity<TPrimaryKey>;
    }
}
