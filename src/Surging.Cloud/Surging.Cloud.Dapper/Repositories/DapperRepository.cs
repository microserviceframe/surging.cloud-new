using Dapper;
using DapperExtensions;
using Microsoft.Extensions.Logging;
using Surging.Cloud.CPlatform.Exceptions;
using Surging.Cloud.CPlatform.Utilities;
using Surging.Cloud.Dapper.Expressions;
using Surging.Cloud.Dapper.Filters.Action;
using Surging.Cloud.Dapper.Filters.Elastic;
using Surging.Cloud.Dapper.Filters.Query;
using Surging.Cloud.Domain.Entities;
using Surging.Cloud.Domain.PagedAndSorted;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Surging.Cloud.Dapper.Repositories
{
    public class DapperRepository<TEntity, TPrimaryKey> : DapperRepositoryBase, IDapperRepository<TEntity, TPrimaryKey>
        where TEntity : class, IEntity<TPrimaryKey>
    {
        private readonly IQueryFilter _queryFilter;
        private readonly IOrgQueryFilter _orgQueryFilter;
        private readonly IAuditActionFilter<TEntity, TPrimaryKey> _creationActionFilter;
        private readonly IAuditActionFilter<TEntity, TPrimaryKey> _modificationActionFilter;
        private readonly IAuditActionFilter<TEntity, TPrimaryKey> _deletionAuditDapperActionFilter;
        
        private readonly ILogger<DapperRepository<TEntity, TPrimaryKey>> _logger;

        public DapperRepository(IQueryFilter queryFilter,
            IOrgQueryFilter orgQueryFilter,
            ILogger<DapperRepository<TEntity, TPrimaryKey>> logger)
        {
            _queryFilter = queryFilter;
            _orgQueryFilter = orgQueryFilter;
            _logger = logger;
            _creationActionFilter =
                ServiceLocator.GetService<IAuditActionFilter<TEntity, TPrimaryKey>>(
                    typeof(CreationAuditDapperActionFilter<TEntity, TPrimaryKey>).Name);
            _modificationActionFilter =
                ServiceLocator.GetService<IAuditActionFilter<TEntity, TPrimaryKey>>(
                    typeof(ModificationAuditDapperActionFilter<TEntity, TPrimaryKey>).Name);
            _deletionAuditDapperActionFilter =
                ServiceLocator.GetService<IAuditActionFilter<TEntity, TPrimaryKey>>(
                    typeof(DeletionAuditDapperActionFilter<TEntity, TPrimaryKey>).Name);
        }

        public async Task InsertAsync(TEntity entity)
        {
            try
            {
                using (var conn = GetDbConnection())
                {
                    if (conn.State != System.Data.ConnectionState.Open)
                    {
                        conn.Open();
                    }

                    _creationActionFilter.ExecuteFilter(entity);

                    conn.Insert<TEntity>(entity);
                }
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex.Message, ex);
                }

                throw new DataAccessException(ex.Message, ex);
            }
        }


        public async Task<TPrimaryKey> InsertAndGetIdAsync(TEntity entity)
        {
            try
            {
                using (var conn = GetDbConnection())
                {
                    if (conn.State != System.Data.ConnectionState.Open)
                    {
                        conn.Open();
                    }

                    _creationActionFilter.ExecuteFilter(entity);
                    conn.Insert(entity);
                    return entity.Id;
                }
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex.Message, ex);
                }

                throw new DataAccessException(ex.Message, ex);
            }
        }

        public async Task InsertOrUpdateAsync(TEntity entity)
        {
            try
            {
                if (entity.Id == null)
                {
                    _creationActionFilter.ExecuteFilter(entity);
                    await InsertAsync(entity);
                }
                else
                {
                    var existEntity = await SingleOrDefaultAsync(CreateEqualityExpressionForId(entity.Id),false);
                    if (existEntity == null)
                    {
                        _creationActionFilter.ExecuteFilter(entity);
                        await InsertAsync(entity);
                    }
                    else
                    {
                        _modificationActionFilter.ExecuteFilter(entity);
                        await UpdateAsync(entity);
                    }
                }
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex.Message, ex);
                }

                throw new DataAccessException(ex.Message, ex);
            }
        }

        public async Task<TPrimaryKey> InsertOrUpdateAndGetIdAsync(TEntity entity)
        {
            try
            {
                if (entity.Id == null)
                {
                    _creationActionFilter.ExecuteFilter(entity);
                    return await InsertAndGetIdAsync(entity);
                }
                else
                {
                    var existEntity = await SingleOrDefaultAsync(CreateEqualityExpressionForId(entity.Id));
                    if (existEntity == null)
                    {
                        _creationActionFilter.ExecuteFilter(entity);
                        return await InsertAndGetIdAsync(entity);
                    }
                    else
                    {
                        _modificationActionFilter.ExecuteFilter(entity);
                        await UpdateAsync(entity);
                        return entity.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex.Message, ex);
                }

                throw new DataAccessException(ex.Message, ex);
            }
        }

        public async Task DeleteAsync(TEntity entity)
        {
            try
            {
                using (var conn = GetDbConnection())
                {
                    if (conn.State != System.Data.ConnectionState.Open)
                    {
                        conn.Open();
                    }

                    if (entity is ISoftDelete)
                    {
                        _deletionAuditDapperActionFilter.ExecuteFilter(entity);
                        conn.Update(entity);
                    }
                    else
                    {
                        conn.Delete(entity);
                    }
                }
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex.Message, ex);
                }

                throw new DataAccessException(ex.Message, ex);
            }
        }

        public async Task DeleteAsync(Expression<Func<TEntity, bool>> predicate)
        {
            IEnumerable<TEntity> items = await GetAllAsync(predicate);
            using (var conn = GetDbConnection())
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    conn.Open();
                }

                using (var trans = conn.BeginTransaction())
                {
                    foreach (TEntity entity in items)
                    {
                        await DeleteAsync(entity, conn, trans);
                    }

                    trans.Commit();
                }

                conn.Close();
            }
        }

        public async Task UpdateAsync(TEntity entity)
        {
            try
            {
                using (var conn = GetDbConnection())
                {
                    if (conn.State != System.Data.ConnectionState.Open)
                    {
                        conn.Open();
                    }

                    _modificationActionFilter.ExecuteFilter(entity);
                    conn.Update(entity);
                }
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex.Message, ex);
                }

                throw new DataAccessException(ex.Message, ex);
            }
        }

        public async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, bool dataPermission = true)
        {
            try
            {
                using (var conn = GetDbConnection())
                {
                    if (conn.State != System.Data.ConnectionState.Open)
                    {
                        conn.Open();
                    }

                    predicate = _queryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
                    if (dataPermission)
                    {
                        predicate = _orgQueryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
                    }

                    var pg = predicate.ToPredicateGroup<TEntity, TPrimaryKey>();
                    var result = conn.GetList<TEntity>(pg).FirstOrDefault();
                    return result;
                }
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex.Message, ex);
                }

                throw new DataAccessException(ex.Message, ex);
            }
        }

        public async Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> predicate, bool dataPermission = true)
        {
            try
            {
                using (var conn = GetDbConnection())
                {
                    if (conn.State != System.Data.ConnectionState.Open)
                    {
                        conn.Open();
                    }

                    predicate = _queryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
                    if (dataPermission)
                    {
                        predicate = _orgQueryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
                    }
                    var pg = predicate.ToPredicateGroup<TEntity, TPrimaryKey>();
                    var result = conn.GetList<TEntity>(pg).First();
                    return result;
                }
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex.Message, ex);
                }

                throw new DataAccessException(ex.Message, ex);
            }
        }


        public async Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> predicate, bool dataPermission = true)
        {
            try
            {
                using (var conn = GetDbConnection())
                {
                    if (conn.State != System.Data.ConnectionState.Open)
                    {
                        conn.Open();
                    }

                    predicate = _queryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
                    if (dataPermission)
                    {
                        predicate = _orgQueryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
                    }
                    var pg = predicate.ToPredicateGroup<TEntity, TPrimaryKey>();
                    var result = conn.GetList<TEntity>(pg).Single();
                    return result;
                }
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex.Message, ex);
                }

                throw new DataAccessException(ex.Message, ex);
            }
        }


        public async Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, bool dataPermission = true)
        {
            try
            {
                using (var conn = GetDbConnection())
                {
                    if (conn.State != System.Data.ConnectionState.Open)
                    {
                        conn.Open();
                    }

                    predicate = _queryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
                    if (dataPermission)
                    {
                        predicate = _orgQueryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
                    }
                    var pg = predicate.ToPredicateGroup<TEntity, TPrimaryKey>();
                    var result = conn.GetList<TEntity>(pg).SingleOrDefault();
                    return result;
                }
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex.Message, ex);
                }

                throw new DataAccessException(ex.Message, ex);
            }
        }

        public Task<TEntity> GetAsync(TPrimaryKey id,bool dataPermission)
        {
            return SingleAsync(CreateEqualityExpressionForId(id),dataPermission);
        }


        public async Task<IEnumerable<TEntity>> GetAllAsync(bool dataPermission = true)
        {
            try
            {
                using (var conn = GetDbConnection())
                {
                    if (conn.State != System.Data.ConnectionState.Open)
                    {
                        conn.Open();
                    }

                    var predicate = _queryFilter.ExecuteFilter<TEntity, TPrimaryKey>();
                    if (dataPermission)
                    {
                        predicate = _orgQueryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
                    }
                    var pg = predicate.ToPredicateGroup<TEntity, TPrimaryKey>();
                    var list = conn.GetList<TEntity>(pg);
                    return list;
                }
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex.Message, ex);
                }

                throw new DataAccessException(ex.Message, ex);
            }
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate, bool dataPermission = true)
        {
            try
            {
                using (var conn = GetDbConnection())
                {
                    if (conn.State != System.Data.ConnectionState.Open)
                    {
                        conn.Open();
                    }

                    predicate = _queryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
                    if (dataPermission)
                    {
                        predicate = _orgQueryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
                    }
                    var pg = predicate.ToPredicateGroup<TEntity, TPrimaryKey>();
                    var list = conn.GetList<TEntity>(pg);
                    return list;
                }
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex.Message, ex);
                }

                throw new DataAccessException(ex.Message, ex);
            }
        }


        public async Task<IEnumerable<TEntity>> QueryAsync(string query, object parameters = null)
        {
            if (string.IsNullOrEmpty(query))
            {
                throw new ArgumentNullException("Sql语句不允许为空");
            }

            try
            {
                using (var conn = GetDbConnection())
                {
                    if (conn.State != System.Data.ConnectionState.Open)
                    {
                        conn.Open();
                    }

                    var result = await conn.QueryAsync<TEntity>(query, parameters);
                    return result;
                }
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex.Message, ex);
                }

                throw new DataAccessException(ex.Message, ex);
            }
        }

        public async Task<IEnumerable<TAny>> Query<TAny>(string query, object parameters = null) where TAny : class
        {
            if (string.IsNullOrEmpty(query))
            {
                throw new ArgumentNullException("Sql语句不允许为空");
            }

            try
            {
                using (var conn = GetDbConnection())
                {
                    if (conn.State != System.Data.ConnectionState.Open)
                    {
                        conn.Open();
                    }

                    var result = await conn.QueryAsync<TAny>(query, parameters);
                    return result;
                }
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex.Message, ex);
                }

                throw new DataAccessException(ex.Message, ex);
            }
        }


        public async Task InsertAsync(TEntity entity, DbConnection conn, DbTransaction trans)
        {
            try
            {
                _creationActionFilter.ExecuteFilter(entity);
                conn.Insert<TEntity>(entity, trans);
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex.Message, ex);
                }

                throw new DataAccessException(ex.Message, ex);
            }
        }

        public Task<TPrimaryKey> InsertAndGetIdAsync(TEntity entity, DbConnection conn, DbTransaction trans)
        {
            try
            {
                _creationActionFilter.ExecuteFilter(entity);
                conn.Insert<TEntity>(entity, trans);

                return Task.FromResult(entity.Id);
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex.Message, ex);
                }

                throw new DataAccessException(ex.Message, ex);
            }
        }

        public async Task InsertOrUpdateAsync(TEntity entity, DbConnection conn, DbTransaction trans)
        {
            try
            {
                if (entity.Id == null)
                {
                    _creationActionFilter.ExecuteFilter(entity);
                    await InsertAsync(entity, conn, trans);
                }
                else
                {
                    var existEntity = await SingleOrDefaultAsync(CreateEqualityExpressionForId(entity.Id), conn, trans,false);
                    if (existEntity == null)
                    {
                        _creationActionFilter.ExecuteFilter(entity);
                        await InsertAsync(entity, conn, trans);
                    }
                    else
                    {
                        _modificationActionFilter.ExecuteFilter(entity);
                        await UpdateAsync(entity, conn, trans);
                    }
                }
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex.Message, ex);
                }

                throw new DataAccessException(ex.Message, ex);
            }
        }

        public async Task<TPrimaryKey> InsertOrUpdateAndGetIdAsync(TEntity entity, DbConnection conn,
            DbTransaction trans)
        {
            try
            {
                if (entity.Id == null)
                {
                    _creationActionFilter.ExecuteFilter(entity);
                    return await InsertAndGetIdAsync(entity, conn, trans);
                }
                else
                {
                    var existEntity = await SingleOrDefaultAsync(CreateEqualityExpressionForId(entity.Id), conn, trans);
                    if (existEntity == null)
                    {
                        _creationActionFilter.ExecuteFilter(entity);
                        return await InsertAndGetIdAsync(entity, conn, trans);
                    }
                    else
                    {
                        _modificationActionFilter.ExecuteFilter(entity);
                        await UpdateAsync(entity, conn, trans);
                        return entity.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex.Message, ex);
                }

                throw new DataAccessException(ex.Message, ex);
            }
        }

        public Task UpdateAsync(TEntity entity, DbConnection conn, DbTransaction trans)
        {
            try
            {
                _modificationActionFilter.ExecuteFilter(entity);
                conn.Update(entity, trans);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex.Message, ex);
                }

                throw new DataAccessException(ex.Message, ex);
            }
        }

        public Task DeleteAsync(TEntity entity, DbConnection conn, DbTransaction trans)
        {
            try
            {
                if (entity is ISoftDelete)
                {
                    _deletionAuditDapperActionFilter.ExecuteFilter(entity);
                    UpdateAsync(entity, conn, trans);
                }
                else
                {
                    conn.Delete(entity, trans);
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex.Message, ex);
                }

                throw new DataAccessException(ex.Message, ex);
            }
        }

        public async Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, DbConnection conn, DbTransaction trans)
        {
            IEnumerable<TEntity> items = await GetAllAsync(predicate);
            foreach (TEntity entity in items)
            {
                await DeleteAsync(entity, conn, trans);
            }
        }

        protected static Expression<Func<TEntity, bool>> CreateEqualityExpressionForId(TPrimaryKey id)
        {
            ParameterExpression lambdaParam = Expression.Parameter(typeof(TEntity));

            BinaryExpression lambdaBody = Expression.Equal(
                Expression.PropertyOrField(lambdaParam, "Id"),
                Expression.Constant(id, typeof(TPrimaryKey))
            );

            return Expression.Lambda<Func<TEntity, bool>>(lambdaBody, lambdaParam);
        }

        public async Task<int> GetCountAsync(bool dataPermission = true)
        {
            try
            {
                using (var conn = GetDbConnection())
                {
                    if (conn.State != System.Data.ConnectionState.Open)
                    {
                        conn.Open();
                    }

                    var predicate = _queryFilter.ExecuteFilter<TEntity, TPrimaryKey>();
                    if (dataPermission)
                    {
                        predicate = _orgQueryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
                    }
                    var pg = predicate.ToPredicateGroup<TEntity, TPrimaryKey>();
                    var count = conn.Count<TEntity>(pg);
                    return count;
                }
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex.Message, ex);
                }

                throw new DataAccessException(ex.Message, ex);
            }
        }

        public Task<int> GetCountAsync(Expression<Func<TEntity, bool>> predicate, DbConnection conn, DbTransaction trans, bool dataPermission = true) 
        {
            try
            {
                predicate = _queryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
                predicate = _orgQueryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
                var pg = predicate.ToPredicateGroup<TEntity, TPrimaryKey>();
                var count = conn.Count<TEntity>(pg, transaction: trans);
                return Task.FromResult(count);
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex.Message, ex);
                }

                throw new DataAccessException(ex.Message, ex);
            }
        }

        public async Task<int> GetCountAsync(Expression<Func<TEntity, bool>> predicate, bool dataPermission = true)
        {
            try
            {
                using (var conn = GetDbConnection())
                {
                    if (conn.State != System.Data.ConnectionState.Open)
                    {
                        conn.Open();
                    }

                    predicate = _queryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
                    if (dataPermission)
                    {
                        predicate = _orgQueryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
                    }
                    var pg = predicate.ToPredicateGroup<TEntity, TPrimaryKey>();
                    var count = conn.Count<TEntity>(pg);
                    return count;
                }
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex.Message, ex);
                }

                throw new DataAccessException(ex.Message, ex);
            }
        }

        public Task<Tuple<IEnumerable<TEntity>, int>> GetPageAsync(Expression<Func<TEntity, bool>> predicate, int index,
            int count, IDictionary<string, SortType> sortProps, bool dataPermission = true)
        {
            try
            {
                using (var conn = GetDbConnection())
                {
                    IList<ISort> sorts = new List<ISort>();

                    if (sortProps != null && sortProps.Any())
                    {
                        foreach (var sortProp in sortProps)
                        {
                            var sort = new Sort()
                            {
                                PropertyName = sortProp.Key,
                                Ascending = sortProp.Value == SortType.Asc ? true : false
                            };
                            sorts.Add(sort);
                        }

                        ;
                    }
                    else
                    {
                        sorts.Add(new Sort()
                        {
                            PropertyName = "Id",
                            Ascending = false
                        });
                    }

                    predicate = _queryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
                    if (dataPermission)
                    {
                        predicate = _orgQueryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
                    }
                    var pg = predicate.ToPredicateGroup<TEntity, TPrimaryKey>();
                    var pageList = conn.GetPage<TEntity>(pg, sorts, index - 1, count).ToList();
                   
                    var totalCount = conn.Count<TEntity>(pg);
                    return Task.FromResult(new Tuple<IEnumerable<TEntity>, int>(pageList, totalCount));
                }
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex.Message, ex);
                }

                throw new DataAccessException(ex.Message, ex);
            }
        }

        public Task<Tuple<IEnumerable<TEntity>, int>> GetPageAsync(Expression<Func<TEntity, bool>> predicate, int index,
            int count, bool dataPermission = true)
        {
            return GetPageAsync(predicate, index, count, null,dataPermission);
        }

        public Task<Tuple<IEnumerable<TEntity>, int>> GetPageAsync(int index, int count,
            IDictionary<string, SortType> sortProps, bool dataPermission = true)
        {
            try
            {
                using (var conn = GetDbConnection())
                {
                    IList<ISort> sorts = new List<ISort>();

                    if (sortProps != null && sortProps.Any())
                    {
                        foreach (var sortProp in sortProps)
                        {
                            var sort = new Sort()
                            {
                                PropertyName = sortProp.Key,
                                Ascending = sortProp.Value == SortType.Asc ? true : false
                            };
                            sorts.Add(sort);
                        }

                        ;
                    }
                    else
                    {
                        var sort = new Sort()
                        {
                            PropertyName = "Id",
                            Ascending = true
                        };
                        sorts.Add(sort);
                    }

                    var predicate = _queryFilter.ExecuteFilter<TEntity, TPrimaryKey>();
                    if (dataPermission)
                    {
                        predicate = _orgQueryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
                    }
                    var pg = predicate.ToPredicateGroup<TEntity, TPrimaryKey>();
                    var pageList = conn.GetPage<TEntity>(pg, sorts, index - 1, count).ToList();
                    var totalCount = conn.Count<TEntity>(pg);
                    return Task.FromResult(new Tuple<IEnumerable<TEntity>, int>(pageList, totalCount));
                }
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex.Message, ex);
                }

                throw new DataAccessException(ex.Message, ex);
            }
        }

        public Task<Tuple<IEnumerable<TEntity>, int>> GetPageAsync(int index, int count, bool dataPermission = true)
        {
            return GetPageAsync(index, count, null,dataPermission);
        }

        public Task<int> GetCountAsync(DbConnection conn, DbTransaction trans, bool dataPermission = true)
        {
            try
            {
                var predicate = _queryFilter.ExecuteFilter<TEntity, TPrimaryKey>();
                if (dataPermission)
                {
                    predicate = _orgQueryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
                }
                var pg = predicate.ToPredicateGroup<TEntity, TPrimaryKey>();
                var count = conn.Count<TEntity>(pg, transaction: trans);
                return Task.FromResult(count);
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex.Message, ex);
                }

                throw new DataAccessException(ex.Message, ex);
            }
        }

        public Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> predicate, DbConnection conn,
            DbTransaction trans, bool dataPermission = true)
        {
            predicate = _queryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
            if (dataPermission)
            {
                predicate = _orgQueryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
            }
            var pg = predicate.ToPredicateGroup<TEntity, TPrimaryKey>();
            var result = conn.GetList<TEntity>(pg, transaction: trans).Single();
            return Task.FromResult(result);
        }

        public Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, DbConnection conn,
            DbTransaction trans, bool dataPermission = true)
        {
            predicate = _queryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
            if (dataPermission)
            {
                predicate = _orgQueryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
            }
            var pg = predicate.ToPredicateGroup<TEntity, TPrimaryKey>();
            var result = conn.GetList<TEntity>(pg, transaction: trans).SingleOrDefault();
            return Task.FromResult(result);
        }

        public Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, DbConnection conn,
            DbTransaction trans, bool dataPermission = true)
        {
            predicate = _queryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
            if (dataPermission)
            {
                predicate = _orgQueryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
            }
            var pg = predicate.ToPredicateGroup<TEntity, TPrimaryKey>();
            var result = conn.GetList<TEntity>(pg, transaction: trans).FirstOrDefault();
            return Task.FromResult(result);
        }


        public Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> predicate, DbConnection conn,
            DbTransaction trans, bool dataPermission = true)
        {
            predicate = _queryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
            if (dataPermission)
            {
                predicate = _orgQueryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
            }
            var pg = predicate.ToPredicateGroup<TEntity, TPrimaryKey>();
            var result = conn.GetList<TEntity>(pg, transaction: trans).First();
            return Task.FromResult(result);
        }

        public Task<TEntity> LastOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, DbConnection conn,
            DbTransaction trans, bool dataPermission = true)
        {
            predicate = _queryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
            if (dataPermission)
            {
                predicate = _orgQueryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
            }
            var pg = predicate.ToPredicateGroup<TEntity, TPrimaryKey>();
            var result = conn.GetList<TEntity>(pg, transaction: trans).LastOrDefault();
            return Task.FromResult(result);
        }


        public Task<TEntity> LastAsync(Expression<Func<TEntity, bool>> predicate, DbConnection conn,
            DbTransaction trans, bool dataPermission = true)
        {
            predicate = _queryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
            if (dataPermission)
            {
                predicate = _orgQueryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
            }
            var pg = predicate.ToPredicateGroup<TEntity, TPrimaryKey>();
            var result = conn.GetList<TEntity>(pg, transaction: trans).Last();
            return Task.FromResult(result);
        }

        public Task<TEntity> GetAsync(TPrimaryKey id, DbConnection conn, DbTransaction trans, bool dataPermission = true)
        {
            return SingleAsync(CreateEqualityExpressionForId(id), conn, trans,dataPermission);
        }

        public Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate, DbConnection conn,
            DbTransaction trans, bool dataPermission = true)
        {
            predicate = _queryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
            if (dataPermission)
            {
                predicate = _orgQueryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
            }
            var pg = predicate.ToPredicateGroup<TEntity, TPrimaryKey>();
            var list = conn.GetList<TEntity>(pg, transaction: trans);
            return Task.FromResult(list);
        }

        public Task<IEnumerable<TEntity>> GetAllAsync(DbConnection conn, DbTransaction trans, bool dataPermission = true)
        {
            var predicate = _queryFilter.ExecuteFilter<TEntity, TPrimaryKey>();
            if (dataPermission)
            {
                predicate = _orgQueryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
            }
            var pg = predicate.ToPredicateGroup<TEntity, TPrimaryKey>();
            var list = conn.GetList<TEntity>(pg, transaction: trans);
            return Task.FromResult(list);
        }

        public async Task<IEnumerable<TEntity>> GetAllIncludeSoftDeleteAsync(Expression<Func<TEntity, bool>> predicate, bool dataPermission = true)
        {
            try
            {
                using (var conn = GetDbConnection())
                {
                    if (conn.State != System.Data.ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    if (dataPermission)
                    {
                        predicate = _orgQueryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
                    }
                    var pg = predicate.ToPredicateGroup<TEntity, TPrimaryKey>();
                    var list = conn.GetList<TEntity>(pg);
                    return list;
                }
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex.Message, ex);
                }

                throw new DataAccessException(ex.Message, ex);
            }
        }

        public async Task<IEnumerable<TEntity>> GetAllIncludeSoftDeleteAsync(bool dataPermission = true)
        {
            try
            {
                using (var conn = GetDbConnection())
                {
                    if (conn.State != System.Data.ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    if (dataPermission)
                    {
                       var predicate = _orgQueryFilter.ExecuteFilter<TEntity, TPrimaryKey>();
                       var list = conn.GetList<TEntity>(predicate);
                       return list;
                    }
                    else
                    {
                        var list = conn.GetList<TEntity>();
                        return list;
                    }

                 
                }
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(ex.Message, ex);
                }

                throw new DataAccessException(ex.Message, ex);
            }
        }

        public async Task<IEnumerable<TEntity>> GetAllIncludeSoftDeleteAsync(Expression<Func<TEntity, bool>> predicate,
            DbConnection conn, DbTransaction trans, bool dataPermission = true)
        {

            if (dataPermission)
            {
                predicate = _orgQueryFilter.ExecuteFilter<TEntity, TPrimaryKey>(predicate);
            }
            var pg = predicate.ToPredicateGroup<TEntity, TPrimaryKey>();
            var list = conn.GetList<TEntity>(pg, transaction: trans);
            return list;
        }

        public async Task<IEnumerable<TEntity>> GetAllIncludeSoftDeleteAsync(DbConnection conn, DbTransaction trans, bool dataPermission = true)
        {
            if (dataPermission)
            {
               var predicate = _orgQueryFilter.ExecuteFilter<TEntity, TPrimaryKey>();
               var list = conn.GetList<TEntity>(predicate);
               return list;
            }
            else
            {
                var list = conn.GetList<TEntity>();
                return list;
            }

        }
    }
}