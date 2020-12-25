
namespace Surging.Cloud.CPlatform.Runtime.Session
{
    public interface ISurgingSession
    {
        long? UserId { get; }

        long? OrgId { get; }
        
        public bool IsAllOrg { get; }

        public long[] DataPermissionOrgIds { get;  }

        string UserName { get; }

    }
}
