using Surging.Core.Domain.Entities;
using Surging.Core.Domain.Entities.Auditing;
using System;
using System.Linq;
using Surging.Core.CPlatform.Exceptions;

namespace Surging.Core.Dapper.Filters.Action
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
            if (typeof(IOrgAudited).IsAssignableFrom(entity.GetType()) && _loginUser != null)
            {
                if (((IOrgAudited) entity).OrgId.HasValue)
                {
                    if (!_loginUser.IsAllOrg && (_loginUser.DataPermissionOrgIds == null 
                                                 || !_loginUser.OrgId.HasValue 
                                                 || !_loginUser.DataPermissionOrgIds.Contains(_loginUser.OrgId.Value)))
                    {
                        throw new BusinessException("您没有删除该数据的权限");
                    }
                }

            }
        }
    }
}
