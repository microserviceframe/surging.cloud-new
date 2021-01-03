using Microsoft.Extensions.Logging;
using Surging.Cloud.Domain.Entities;
using Surging.Cloud.Domain.Entities.Auditing;
using System;
using System.Linq;
using Surging.Cloud.CPlatform.Exceptions;

namespace Surging.Cloud.Dapper.Filters.Action
{
    public class CreationAuditDapperActionFilter<TEntity, TPrimaryKey> : DapperActionFilterBase, IAuditActionFilter<TEntity, TPrimaryKey> where TEntity : class, IEntity<TPrimaryKey>
    {

        public void ExecuteFilter(TEntity entity)
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entity.GetType()))
            {
                ((ISoftDelete)entity).IsDeleted = Normal;
            }
            if (typeof(IHasCreationTime).IsAssignableFrom(entity.GetType()))
            {
                ((IHasCreationTime)entity).CreationTime = DateTime.Now;
            }
            CheckAndSetId(entity);
            
            if (typeof(ICreationAudited).IsAssignableFrom(typeof(TEntity)))
            {
                _logger.LogDebug($"当前操作数据的用户为:{_loginUser.UserId} - {_loginUser.UserName}");
                var record = entity as ICreationAudited;
                record.CreatorUserId = _loginUser.UserId;
            }

            if (typeof(IHasModificationTime).IsAssignableFrom(entity.GetType()))
            {
                ((IHasModificationTime)entity).LastModificationTime = DateTime.Now;
            }
            if (typeof(IModificationAudited).IsAssignableFrom(entity.GetType()))
            {
                ((IModificationAudited)entity).LastModifierUserId = _loginUser.UserId;
            }
            
            if (typeof(IMultiTenant).IsAssignableFrom(entity.GetType()))
            {
                ((IMultiTenant)entity).TenantId = _loginUser.TenantId;
            }

            if (typeof(IOrgAudited).IsAssignableFrom(entity.GetType()))
            {
                if (((IOrgAudited) entity).OrgId.HasValue)
                {
                    if (!_loginUser.IsAllOrg && (_loginUser.DataPermissionOrgIds == null 
                                                 || !_loginUser.OrgId.HasValue 
                                                 || !_loginUser.DataPermissionOrgIds.Contains(_loginUser.OrgId.Value)))
                    {
                        throw new BusinessException("您没有插入数据的权限");
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
