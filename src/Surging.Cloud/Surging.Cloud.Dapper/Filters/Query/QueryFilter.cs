using Surging.Cloud.Dapper.Utils;
using Surging.Cloud.Domain.Entities;
using System;
using System.Linq.Expressions;
using System.Reflection;
using Surging.Cloud.CPlatform.Runtime.Session;
using Surging.Cloud.Domain.Entities.Auditing;

namespace Surging.Cloud.Dapper.Filters.Query
{
    public class QueryFilter : IQueryFilter
    {
        private int IsDeleted => 0;
        
        //  protected readonly ILogger<QueryFilter> _logger;
        protected readonly ISurgingSession _loginUser;

        public QueryFilter()
        {
            // _logger = ServiceLocator.GetService<ILogger<QueryFilter>>();
            _loginUser = NullSurgingSession.Instance;
        }

        public Expression<Func<TEntity, bool>> ExecuteFilter<TEntity, TPrimaryKey>(Expression<Func<TEntity, bool>> predicate = null) where TEntity : class, IEntity<TPrimaryKey>
        {
            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                PropertyInfo propType = typeof(TEntity).GetProperty(nameof(ISoftDelete.IsDeleted));
                if (predicate == null)
                {
                    predicate = ExpressionUtils.MakePredicate<TEntity>(nameof(ISoftDelete.IsDeleted), IsDeleted, propType.PropertyType);
                }
                else
                {
                    ParameterExpression paramExpr = predicate.Parameters[0];
                    MemberExpression memberExpr = Expression.Property(paramExpr, nameof(ISoftDelete.IsDeleted));
                    BinaryExpression body = Expression.AndAlso(
                        predicate.Body,
                        Expression.Equal(memberExpr, Expression.Constant(IsDeleted, propType.PropertyType)));
                    predicate = Expression.Lambda<Func<TEntity, bool>>(body, paramExpr);
                }
            }

            if (typeof(IMultiTenant).IsAssignableFrom(typeof(TEntity)))
            {
                PropertyInfo propType = typeof(TEntity).GetProperty(nameof(IMultiTenant.TenantId));
                if (predicate == null)
                {
                    predicate = ExpressionUtils.MakePredicate<TEntity>(nameof(IMultiTenant.TenantId), _loginUser.TenantId, propType.PropertyType);
                }
                else
                {
                    ParameterExpression paramExpr = predicate.Parameters[0];
                    MemberExpression memberExpr = Expression.Property(paramExpr, nameof(IMultiTenant.TenantId));
                    BinaryExpression body = Expression.AndAlso(
                        predicate.Body,
                        Expression.Equal(memberExpr, Expression.Constant( _loginUser.TenantId, propType.PropertyType)));
                    predicate = Expression.Lambda<Func<TEntity, bool>>(body, paramExpr);
                }
            }

            return predicate;
        }

    }
}
