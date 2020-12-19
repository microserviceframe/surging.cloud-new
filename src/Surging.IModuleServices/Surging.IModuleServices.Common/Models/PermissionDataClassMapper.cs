using DapperExtensions.Mapper;
using Surging.Core.Domain.Entities.Auditing;

namespace Surging.IModuleServices.Common.Models
{
    public class PermissionDataClassMapper : ClassMapper<PermissionData>
    {
        public PermissionDataClassMapper()
        {
            Map(p => p.Id).Key(KeyType.Identity);

            if (!typeof(ICreationAudited).IsAssignableFrom(typeof(PermissionData)))
            {
                Map(p => ((ICreationAudited)p).CreationTime).Ignore();
                Map(p => ((ICreationAudited)p).CreatorUserId).Ignore();
            }
            else
            {
                Map(p => ((ICreationAudited)p).CreationTime).Column("CreateTime");
                Map(p => ((ICreationAudited)p).CreatorUserId).Column("CreateBy");
            }

            if (!typeof(IModificationAudited).IsAssignableFrom(typeof(PermissionData)))
            {
                Map(p => ((IModificationAudited)p).LastModificationTime).Ignore();
                Map(p => ((IModificationAudited)p).LastModifierUserId).Ignore();
            }
            else
            {
                Map(p => ((IModificationAudited)p).LastModificationTime).Column("UpdateTime");
                Map(p => ((IModificationAudited)p).LastModifierUserId).Column("UpdateBy");
            }

            if (!typeof(IDeletionAudited).IsAssignableFrom(typeof(PermissionData)))
            {
                Map(p => ((IDeletionAudited)p).DeleterUserId).Ignore();
                Map(p => ((IDeletionAudited)p).DeletionTime).Ignore();
                Map(p => ((IDeletionAudited)p).IsDeleted).Ignore();
            }
            else
            {
                Map(p => ((IDeletionAudited)p).DeleterUserId).Column("DeleteBy");
                Map(p => ((IDeletionAudited)p).DeletionTime).Column("DeleteTime");
                Map(p => ((IDeletionAudited)p).IsDeleted).Column("IsDeleted");
            }

            AutoMap();
        }
    }
}