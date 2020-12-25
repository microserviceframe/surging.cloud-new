
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Surging.Cloud.CPlatform;
using Surging.Cloud.CPlatform.Transport.Implementation;
using Surging.Cloud.CPlatform.Utilities;
using Surging.Cloud.Dapper;
using Surging.Cloud.Dapper.Extensions;
using Surging.Cloud.Dapper.Repositories;
using Surging.Cloud.Domain.PagedAndSorted;
using Surging.Cloud.Domain.PagedAndSorted.Extensions;
using Surging.Cloud.ProxyGenerator;
using Surging.IModuleServices.Common;
using Surging.IModuleServices.Common.Models;

namespace Surging.Modules.Common.Domain
{
    public class TestPermissionDataService : ProxyServiceBase, ITestPermissionDataService
    {
        private readonly IDapperRepository<PermissionData, long> _permissionDataRepository;

        public TestPermissionDataService(IDapperRepository<PermissionData, long> permissionDataRepository)
        {
            _permissionDataRepository = permissionDataRepository;
        }

        public async Task<IPagedResult<PermissionData>> Search(QueryPermissionData query)
        {
            RpcContext.GetContext().SetAttachment(ClaimTypes.DataPermissionOrgIds, new long[] { 2, 5});
            //var result = await _permissionDataRepository.GetPageAsync(p =>
            //    p.UserName.Contains(query.UserName) && p.Address.Contains(query.Address),query.PageIndex,query.PageCount);
            //return result.Item1.GetPagedResult(result.Item2);
            var sql = "SELECT * FROM PermissionData Where 1=1 ";
            var sqlParams = new Dictionary<string, object>();
            if (!query.UserName.IsNullOrEmpty()) 
            {
                sql += " AND UserName like @UserName";
                sqlParams.Add("UserName", $"%{query.UserName}%");
            }
            if (!query.Address.IsNullOrEmpty())
            {
                sql += " AND Address like @Address";
                sqlParams.Add("Address", $"%{query.Address}%");
            }
            var result = await Connection.QueryDataPermissionPageAsync<PermissionData>(sql,sqlParams,query.PageIndex,query.PageCount
                );
            var result2 = await Connection.QueryDataPermissionAsync<PermissionData>(sql, sqlParams);
            var s = await _permissionDataRepository.GetAllAsync();
            return result.Item1.GetPagedResult((int)result.Item2); ;


        }

        protected virtual DbConnection Connection
        {
            get
            {
                if (DbSetting.Instance == null)
                {
                    throw new Exception("未设置数据库连接");
                }
                DbConnection conn = new MySqlConnection(DbSetting.Instance.ConnectionString);
                return conn;
            }
        }
    }
}