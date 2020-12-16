using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using DapperExtensions;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.Domain.Entities;
namespace Surging.Core.Dapper.Expressions
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
