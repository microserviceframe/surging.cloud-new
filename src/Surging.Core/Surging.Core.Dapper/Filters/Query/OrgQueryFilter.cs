using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Surging.Core.CPlatform.Runtime.Session;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.Dapper.Utils;
using Surging.Core.Domain.Entities;
using Surging.Core.Domain.Entities.Auditing;

namespace Surging.Core.Dapper.Filters.Query
{
    public class OrgQueryFilter : IOrgQueryFilter
    {
      //  protected readonly ILogger<OrgQueryFilter> _logger;
        protected readonly ISurgingSession _loginUser;

        public OrgQueryFilter()
        {
           // _logger = ServiceLocator.GetService<ILogger<OrgQueryFilter>>();
            _loginUser = NullSurgingSession.Instance;
        }

        public Expression<Func<TEntity, bool>> ExecuteFilter<TEntity, TPrimaryKey>(Expression<Func<TEntity, bool>> predicate = null) where TEntity : class, IEntity<TPrimaryKey>
        {
            if (!_loginUser.UserId.HasValue)
            {
                return predicate;
            }

            if (_loginUser.IsAllOrg)
            {
                return predicate;
            }

            if (typeof(IOrgAudited).IsAssignableFrom(typeof(TEntity)))
            {
                PropertyInfo propType = typeof(TEntity).GetProperty(nameof(IOrgAudited.OrgId));
                
                var permissionOrgIds = new long[] { _loginUser.OrgId.HasValue ? _loginUser.OrgId.Value : -1 };
                if (_loginUser.DataPermissionOrgIds != null && _loginUser.DataPermissionOrgIds.Any() )
                {
                    permissionOrgIds = _loginUser.DataPermissionOrgIds;

                }
                Expression<Func<TEntity, bool>> permissionOrgPredicate = null;
                foreach (var permissionOrgId in permissionOrgIds)
                {
                    if (permissionOrgPredicate == null)
                    {
                        permissionOrgPredicate = ExpressionUtils.MakePredicate<TEntity>(nameof(IOrgAudited.OrgId), permissionOrgId, propType.PropertyType);
                    }
                    else 
                    {
                        ParameterExpression paramExpr = permissionOrgPredicate.Parameters[0];
                        MemberExpression memberExpr = Expression.Property(paramExpr, nameof(IOrgAudited.OrgId));
                        BinaryExpression body = Expression.OrElse(
                            permissionOrgPredicate.Body,
                            Expression.Equal(memberExpr, Expression.Constant(permissionOrgId, propType.PropertyType)));
                        permissionOrgPredicate = Expression.Lambda<Func<TEntity, bool>>(body, paramExpr);
                    }
                }
                if (predicate == null)
                {
                    predicate = permissionOrgPredicate;
                }
                else 
                {
                    var invokedExpr = Expression.Invoke(predicate, permissionOrgPredicate.Parameters);
                    var expr = Expression.Lambda<Func<TEntity, bool>>
                        (Expression.AndAlso(permissionOrgPredicate.Body, invokedExpr), permissionOrgPredicate.Parameters);
                    return expr;
                }

            }


            return predicate;
        }
    }
}