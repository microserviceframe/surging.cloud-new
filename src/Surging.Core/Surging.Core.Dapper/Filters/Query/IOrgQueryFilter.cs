using System;
using System.Linq.Expressions;
using Surging.Core.Domain.Entities;

namespace Surging.Core.Dapper.Filters.Query
{
    public interface IOrgQueryFilter
    {
        Expression<Func<TEntity, bool>> ExecuteFilter<TEntity, TPrimaryKey>(Expression<Func<TEntity, bool>> predicate = null) where TEntity : class, IEntity<TPrimaryKey>;
    }
}
