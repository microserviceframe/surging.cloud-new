using Surging.Cloud.Domain.Entities;
using Surging.Cloud.Domain.Entities.Auditing;
using System;
using System.Linq;
using Surging.Cloud.CPlatform.Exceptions;

namespace Surging.Cloud.Dapper.Filters.Action
{
    public class ModificationAuditDapperActionFilter<TEntity, TPrimaryKey> : DapperActionFilterBase, IAuditActionFilter<TEntity, TPrimaryKey> where TEntity : class, IEntity<TPrimaryKey>
    {
        public void ExecuteFilter(TEntity entity)
        {
            if (typeof(IModificationAudited).IsAssignableFrom(typeof(TEntity)))
            {

                var record = entity as IModificationAudited;
                if (_loginUser.UserId.HasValue) 
                {
                    record.LastModifierUserId = _loginUser.UserId;
                }               
                record.LastModificationTime = DateTime.Now;

            }
            if (typeof(IMultiTenant).IsAssignableFrom(typeof(TEntity)))
            {

                var record = entity as IMultiTenant;
                if (_loginUser.TenantId != record.TenantId) 
                {
                    throw new BusinessException("您没有更新数据的权限");
                }

            }
            if (typeof(IOrgAudited).IsAssignableFrom(entity.GetType()))
            {
                if (((IOrgAudited) entity).OrgId.HasValue)
                {
                    if (!_loginUser.IsAllOrg && (_loginUser.DataPermissionOrgIds == null
                                                 || !_loginUser.DataPermissionOrgIds.Contains(((IOrgAudited) entity).OrgId.Value)))
                    {
                        throw new BusinessException("您没有更新数据的权限");
                    }
                }
                else
                {
                    ((IOrgAudited)entity).OrgId = _loginUser.OrgId;
                }
                
            }
        }
    }
}
