using Surging.Core.Domain.Entities;
using Surging.Core.Domain.Entities.Auditing;
using System;
using System.Linq;
using Surging.Core.CPlatform.Exceptions;

namespace Surging.Core.Dapper.Filters.Action
{
    public class ModificationAuditDapperActionFilter<TEntity, TPrimaryKey> : DapperActionFilterBase, IAuditActionFilter<TEntity, TPrimaryKey> where TEntity : class, IEntity<TPrimaryKey>
    {
        public void ExecuteFilter(TEntity entity)
        {
            ////var loginUser = NullSurgingSession.Instance;
            if (typeof(IModificationAudited).IsAssignableFrom(typeof(TEntity)) && _loginUser != null)
            {

                var record = entity as IModificationAudited;
                if (_loginUser.UserId.HasValue) 
                {
                    record.LastModifierUserId = _loginUser.UserId;
                }               
                record.LastModificationTime = DateTime.Now;

            }
            if (typeof(IOrgAudited).IsAssignableFrom(entity.GetType()) && _loginUser != null)
            {
                if (((IOrgAudited) entity).OrgId.HasValue)
                {
                    if (!_loginUser.IsAllOrg && (_loginUser.DataPermissionOrgIds == null 
                                                 || !_loginUser.OrgId.HasValue 
                                                 || !_loginUser.DataPermissionOrgIds.Contains(_loginUser.OrgId.Value)))
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
