
namespace Surging.Core.CPlatform.Runtime.Session
{
    public interface ISurgingSession
    {
        long? UserId { get; }

        long? OrgId { get; }

        public bool InspectDataPermission { get;  }

        public bool IsAllOrg { get; }

        public long[] DataPermissionOrgIds { get;  }

        string UserName { get; }

    }
}
