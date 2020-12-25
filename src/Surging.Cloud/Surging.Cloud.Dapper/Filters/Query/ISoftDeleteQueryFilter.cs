using Surging.Cloud.CPlatform.Ioc;
using Surging.Cloud.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Surging.Cloud.Dapper.Filters.Query
{
    public interface ISoftDeleteQueryFilter 
    {
        Expression<Func<TEntity, bool>> ExecuteFilter<TEntity, TPrimaryKey>(Expression<Func<TEntity, bool>> predicate = null) where TEntity : class, IEntity<TPrimaryKey>;
    }
}
