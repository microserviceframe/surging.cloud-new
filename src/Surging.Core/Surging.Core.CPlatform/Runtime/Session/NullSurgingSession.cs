namespace Surging.Core.CPlatform.Runtime.Session
{
    public class NullSurgingSession : SurgingSessionBase
    {
        private NullSurgingSession()
        {
        }

        public static ISurgingSession Instance { get; } = new RpcContextSession();

        public override long? UserId { get; } = Instance.UserId;
        public override string UserName { get; } = Instance.UserName;

        public override long? OrgId { get; } = Instance.OrgId;
        public override long[] DataPermissionOrgIds { get; } = Instance.DataPermissionOrgIds;

        public override bool IsAllOrg { get; } = Instance.IsAllOrg;

    }
}
