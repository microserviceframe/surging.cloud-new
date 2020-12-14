using Newtonsoft.Json;
using Surging.Core.CPlatform.Transport.Implementation;
using System;

namespace Surging.Core.CPlatform.Runtime.Session
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

        public override bool InspectDataPermission 
        {
            get
            {
                var inspectDataPermission = RpcContext.GetContext().GetAttachment(ClaimTypes.InspectDataPermission);
                if (inspectDataPermission != null)
                {
                    return Convert.ToBoolean(inspectDataPermission);
                }
                return false;
            }
        }

        public override long[] DataPermissionOrgIds
        {
            get
            {
                var dataPermissionOrdIds = RpcContext.GetContext().GetAttachment(ClaimTypes.DataPermissionOrgIds);
                if (dataPermissionOrdIds != null)
                {
                    return dataPermissionOrdIds as long[];
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
