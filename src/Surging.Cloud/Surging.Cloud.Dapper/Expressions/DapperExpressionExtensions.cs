using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using DapperExtensions;
using Surging.Cloud.CPlatform.Utilities;
using Surging.Cloud.Domain.Entities;
namespace Surging.Cloud.Dapper.Expressions
{
    internal static class DapperExpressionExtensions
    {

        public static IPredicate ToPredicateGroup<TEntity, TPrimaryKey>(this Expression<Func<TEntity, bool>> expression) where TEntity : class, IEntity<TPrimaryKey>
        {
            if (expression != null)
            {
                var dev = new DapperExpressionVisitor<TEntity, TPrimaryKey>();
                IPredicate pg = dev.Process(expression);
                return pg;
            }

            var groups = new PredicateGroup
            {
                Operator = GroupOperator.And,
                Predicates = new List<IPredicate>()
            };
            return groups;

        }

    }
}
