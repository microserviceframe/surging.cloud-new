namespace Surging.Cloud.CPlatform.Runtime.Session
{
    public abstract class SurgingSessionBase : ISurgingSession
    {
        public abstract long? UserId { get; }
        public abstract string UserName { get; }
        public abstract long? OrgId { get; }
        public abstract bool IsAllOrg { get; }
        public abstract long[] DataPermissionOrgIds { get; }

        
    }
}
