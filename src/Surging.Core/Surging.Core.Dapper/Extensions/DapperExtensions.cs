using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Surging.Core.CPlatform.Runtime.Session;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.Domain.PagedAndSorted;

namespace Surging.Core.Dapper.Extensions
{
    public static class DapperExtensions
    {
        private static int IsDeleted => 0;
        public static async Task<IEnumerable<T>> QueryDataPermissionAsync<T>(this DbConnection connection,
            string sql,
            IDictionary<string, object> sqlParams,

            IDictionary<string, SortType> sortTypes = null,
            string orgIdFieldName = "OrgId",
            string deleteField = "IsDeleted")
        where T : class
        {
            string queryCountSql = UpdateSql(ref sql, sqlParams, sortTypes, orgIdFieldName, deleteField);

            return await connection.QueryAsync<T>(sql, sqlParams);
        }




        public static async Task<Tuple<IEnumerable<T>, long>> QueryDataPermissionPageAsync<T>(this DbConnection connection,
            string sql,
            IDictionary<string, object> sqlParams,
            int pageIndex,
            int pageCount,
            IDictionary<string, SortType> sortTypes = null,
            string orgIdFieldName = "OrgId",
            string deleteField = "IsDeleted")
            where T : class
        {
            sql = UpdateSql(ref sql, sqlParams, sortTypes, orgIdFieldName, deleteField);
            var queryCountSql = "SELECT COUNT(ID) FROM " + sql.Substring(sql.ToLower().IndexOf("from") + "from".Length);

            switch (DbSetting.Instance.DbType)
            {
                case DbType.MySql:
                    sql += $" LIMIT {(pageIndex - 1) * pageCount},{pageCount}";
                    break;
                case DbType.SqlServer:
                    sql += $" OFFSET {(pageIndex - 1) * pageCount} ROWS FETCH NEXT {pageCount} ROWS ONLY";
                    break;
                case DbType.Oracle:
                    sql += $" ROWNO  BETWEEN  {(pageIndex - 1) * pageCount} AND {pageIndex * pageCount}";
                    break;
            }
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            var count = await connection.ExecuteScalarAsync<int>(queryCountSql, sqlParams);
            var queryResult = await connection.QueryAsync<T>(sql, sqlParams);
            return new Tuple<IEnumerable<T>, long>(queryResult, count);

        }

        private static string UpdateSql(ref string sql, IDictionary<string, object> sqlParams, IDictionary<string, SortType> sortTypes, string orgIdFieldName, string deleteField)
        {
            if (!sql.ToLower().Contains("where"))
            {
                sql += " WHERE 1=1 ";
            }
            if (!deleteField.IsNullOrEmpty())
            {
                sql += $" AND {deleteField}=@Deleted";
                if (!sqlParams.ContainsKey("Deleted"))
                {
                    sqlParams.Add("Deleted", IsDeleted);
                }
            }

            var permissionOrgIds = GetPermissionOrgIds();
            if (permissionOrgIds != null && permissionOrgIds.Any())
            {
                sql += " AND (";
                foreach (var permissionOrgId in permissionOrgIds)
                {
                    sql += $" {orgIdFieldName}=@PermissionOrgId{permissionOrgId} OR";
                    if (!sqlParams.ContainsKey($"@PermissionOrgId{permissionOrgId}"))
                    {
                        sqlParams.Add($"@PermissionOrgId{permissionOrgId}", permissionOrgId);
                    }
                }

                sql = sql.Remove(sql.Length - 2);
                sql += ")";
            }
            if (sortTypes != null && sortTypes.Any())
            {
                sql += " ORDER BY ";
                foreach (var sortType in sortTypes)
                {
                    sql += $" {sortType.Key} {sortType.Value}";
                }
            }
            else
            {
                sql += " ORDER BY Id Desc";
            }

            return sql;
        }

        public static async Task<long> QueryDataPermissionPageAsync<T>(this DbConnection connection,
            string sql,
            IDictionary<string, object> sqlParams,
            string orgIdFieldName = "OrgId",
            string deleteField = "IsDeleted")
            where T : class
        {
            sql = UpdateSql(sql, sqlParams, orgIdFieldName, deleteField);
            return await connection.ExecuteScalarAsync<long>(sql, sqlParams);
        }

        private static string UpdateSql(string sql, IDictionary<string, object> sqlParams, string orgIdFieldName, string deleteField)
        {
            if (!sql.ToLower().Contains("where"))
            {
                sql += " WHERE 1=1 ";
            }
            if (!deleteField.IsNullOrEmpty())
            {
                sql += $" AND {deleteField}=@Deleted";
                if (!sqlParams.ContainsKey("Deleted"))
                {
                    sqlParams.Add("Deleted", IsDeleted);
                }
            }

            var permissionOrgIds = GetPermissionOrgIds();
            if (permissionOrgIds != null && permissionOrgIds.Any())
            {
                sql += " AND (";
                foreach (var permissionOrgId in permissionOrgIds)
                {
                    sql += $" {orgIdFieldName}=@PermissionOrgId{permissionOrgId} OR";
                    if (!sqlParams.ContainsKey($"@PermissionOrgId{permissionOrgId}"))
                    {
                        sqlParams.Add($"@PermissionOrgId{permissionOrgId}", permissionOrgId);
                    }
                }

                sql = sql.Remove(sql.Length - 2);
                sql += ")";
            }

            return sql;
        }

        private static long[] GetPermissionOrgIds()
        {
            var loginUser = NullSurgingSession.Instance;
            if (!loginUser.UserId.HasValue)
            {
                return null;
            }

            if (loginUser.IsAllOrg)
            {
                return null;
            }
            var permissionOrgIds = new long[] { loginUser.OrgId.HasValue ? loginUser.OrgId.Value : -1 };
            if (loginUser.DataPermissionOrgIds != null && loginUser.DataPermissionOrgIds.Any())
            {
                permissionOrgIds = loginUser.DataPermissionOrgIds;
            }

            return permissionOrgIds;
        }
    }
}