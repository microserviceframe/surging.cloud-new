using Surging.Cloud.Domain.Entities;
using Surging.Cloud.Domain.PagedAndSorted;
using Surging.Cloud.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Cloud.Dapper.Repositories
{
    public interface IDapperRepository<TEntity, TPrimaryKey> : IRepository where TEntity : class, IEntity<TPrimaryKey>
    {
        Task InsertAsync(TEntity entity);

        Task<TPrimaryKey> InsertAndGetIdAsync(TEntity entity);

        Task InsertOrUpdateAsync(TEntity entity);

        Task<TPrimaryKey> InsertOrUpdateAndGetIdAsync(TEntity entity);

        Task UpdateAsync(TEntity entity);

        Task DeleteAsync(TEntity entity);

        Task DeleteAsync(Expression<Func<TEntity, bool>> predicate);

        Task InsertAsync(TEntity entity, DbConnection conn, DbTransaction trans);

        Task<TPrimaryKey> InsertAndGetIdAsync(TEntity entity, DbConnection conn, DbTransaction trans);

        Task InsertOrUpdateAsync(TEntity entity, DbConnection conn, DbTransaction trans);

        Task<TPrimaryKey> InsertOrUpdateAndGetIdAsync(TEntity entity, DbConnection conn, DbTransaction trans);

        Task UpdateAsync(TEntity entity, DbConnection conn, DbTransaction trans);

        Task DeleteAsync(TEntity entity, DbConnection conn, DbTransaction trans);

        Task DeleteAsync(Expression<Func<TEntity, bool>> predicate, DbConnection conn, DbTransaction trans);

        Task<int> GetCountAsync(Expression<Func<TEntity, bool>> predicate, bool dataPermission = true);

        Task<int> GetCountAsync(bool dataPermission = true);

        Task<int> GetCountAsync(Expression<Func<TEntity, bool>> predicate, DbConnection conn, DbTransaction trans, bool dataPermission = true);

        Task<int> GetCountAsync(DbConnection conn, DbTransaction trans, bool dataPermission = true);

        Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> predicate, bool dataPermission = true);

        Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, bool dataPermission = true);

        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, bool dataPermission = true);

        Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> predicate, bool dataPermission = true);

        Task<TEntity> GetAsync(TPrimaryKey id, bool dataPermission = true);

        Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate, bool dataPermission = true);

        Task<IEnumerable<TEntity>> GetAllAsync(bool dataPermission = true);

        Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> predicate, DbConnection conn, DbTransaction trans, bool dataPermission = true);

        Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, DbConnection conn, DbTransaction trans, bool dataPermission = true);

        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, DbConnection conn, DbTransaction trans, bool dataPermission = true);

        Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> predicate, DbConnection conn, DbTransaction trans, bool dataPermission = true);

        Task<TEntity> LastOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, DbConnection conn, DbTransaction trans, bool dataPermission = true);

        Task<TEntity> LastAsync(Expression<Func<TEntity, bool>> predicate, DbConnection conn, DbTransaction trans, bool dataPermission = true);

        Task<TEntity> GetAsync(TPrimaryKey id, DbConnection conn, DbTransaction trans, bool dataPermission = true);

        Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate, DbConnection conn, DbTransaction trans, bool dataPermission = true);

        Task<IEnumerable<TEntity>> GetAllAsync(DbConnection conn, DbTransaction trans, bool dataPermission = true);

        Task<IEnumerable<TEntity>> GetAllIncludeSoftDeleteAsync(Expression<Func<TEntity, bool>> predicate, bool dataPermission = true);

        Task<IEnumerable<TEntity>> GetAllIncludeSoftDeleteAsync(bool dataPermission = true);

        Task<IEnumerable<TEntity>> GetAllIncludeSoftDeleteAsync(Expression<Func<TEntity, bool>> predicate, DbConnection conn, DbTransaction trans, bool dataPermission = true);

        Task<IEnumerable<TEntity>> GetAllIncludeSoftDeleteAsync(DbConnection conn, DbTransaction trans, bool dataPermission = true);

        Task<IEnumerable<TEntity>> QueryAsync(string query, object parameters = null);

        Task<IEnumerable<TAny>> Query<TAny>(string query, object parameters = null) where TAny : class;


        Task<Tuple<IEnumerable<TEntity>,int>> GetPageAsync(Expression<Func<TEntity, bool>> predicate, int index, int count, IDictionary<string, SortType> sortProps, bool dataPermission = true);

        Task<Tuple<IEnumerable<TEntity>, int>> GetPageAsync(Expression<Func<TEntity, bool>> predicate, int index, int count, bool dataPermission = true);

        Task<Tuple<IEnumerable<TEntity>, int>> GetPageAsync(int index, int count, IDictionary<string, SortType> sortProps, bool dataPermission = true);

        Task<Tuple<IEnumerable<TEntity>, int>> GetPageAsync(int index, int count, bool dataPermission = true);
    }
}
