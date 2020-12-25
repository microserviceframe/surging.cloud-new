using Newtonsoft.Json;
using Surging.Cloud.CPlatform.Transport.Implementation;
using System;
using Surging.Cloud.CPlatform.Serialization;
using Surging.Cloud.CPlatform.Utilities;

namespace Surging.Cloud.CPlatform.Runtime.Session
{
    public class RpcContextSession : SurgingSessionBase
    {
        internal RpcContextSession()
        {
        }

        public override long? UserId
        {
            get
            {
                var userId = RpcContext.GetContext().GetAttachment(ClaimTypes.UserId);
                if (userId != null) 
                {
                    return Convert.ToInt64(userId);
                }
                return null;
            }
        }

        public override string UserName
        {
            get
            {
                var userName = RpcContext.GetContext().GetAttachment(ClaimTypes.UserName);
                if (userName != null)
                {
                    return userName.ToString();
                }
                return null;
            }
        }

        public override long? OrgId 
        {
            get
            {
                var orgId = RpcContext.GetContext().GetAttachment(ClaimTypes.OrgId);
                if (orgId != null)
                {
                    return Convert.ToInt64(orgId);
                }
                return null;
            }
        }

        public override long[] DataPermissionOrgIds
        {
            get
            {
                var dataPermissionOrdIds = RpcContext.GetContext().GetAttachment(ClaimTypes.DataPermissionOrgIds);
                if (dataPermissionOrdIds != null)
                {
                    if (dataPermissionOrdIds is long[])
                    {
                        return (long[])dataPermissionOrdIds;
                    }
                    var serializer = ServiceLocator.GetService<ISerializer<object>>();
                    return serializer.Deserialize<object, long[]>(dataPermissionOrdIds);
                }
                return null;
            }
        }

        public override bool IsAllOrg
        {
            get
            {
                var isAllOrg = RpcContext.GetContext().GetAttachment(ClaimTypes.IsAllOrg);
                if (isAllOrg != null)
                {
                    return Convert.ToBoolean(isAllOrg);
                }
                return false;
            }
        }
    }
}
