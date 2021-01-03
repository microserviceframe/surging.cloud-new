using Surging.Cloud.Domain.Entities;
using Surging.Cloud.Domain.Entities.Auditing;
using System;
using System.Linq;
using Surging.Cloud.CPlatform.Exceptions;

namespace Surging.Cloud.Dapper.Filters.Action
{
    public class DeletionAuditDapperActionFilter<TEntity, TPrimaryKey> : DapperActionFilterBase, IAuditActionFilter<TEntity, TPrimaryKey> where TEntity : class, IEntity<TPrimaryKey>
    {
        public void ExecuteFilter(TEntity entity)
        {
            if (entity is ISoftDelete)
            {
                ((ISoftDelete)entity).IsDeleted = IsDeleted;
                if (typeof(IHasDeletionTime).IsAssignableFrom(entity.GetType()))
                {
                    ((IHasDeletionTime)entity).DeletionTime = DateTime.Now;
                }
                if (typeof(IDeletionAudited).IsAssignableFrom(entity.GetType()))
                {
                   
                    ((IDeletionAudited)entity).DeletionTime = DateTime.Now;
                    ((IDeletionAudited)entity).DeleterUserId = _loginUser.UserId;
                }
            }
            
            if (typeof(IMultiTenant).IsAssignableFrom(typeof(TEntity)))
            {

                var record = entity as IMultiTenant;
                if (_loginUser.TenantId != record.TenantId) 
                {
                    throw new BusinessException("您没有删除该数据的权限");
                }

            }
            if (typeof(IOrgAudited).IsAssignableFrom(entity.GetType()))
            {
                if (((IOrgAudited) entity).OrgId.HasValue)
                {
                    if (!_loginUser.IsAllOrg && (_loginUser.DataPermissionOrgIds == null
                                                 || !_loginUser.DataPermissionOrgIds.Contains(((IOrgAudited) entity).OrgId.Value)))
                    {
                        throw new BusinessException("您没有删除该数据的权限");
                    }
                }

            }
        }
    }
}
