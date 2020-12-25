using Surging.Cloud.CPlatform.EventBus.Events;
using System.Collections.Generic;

namespace Surging.Cloud.Domain.Entities
{
    public interface IAggregateRoot : IAggregateRoot<int>, IEntity
    {
    }

    public interface IAggregateRoot<TPrimaryKey> : IEntity<TPrimaryKey>
    {
        ICollection<IntegrationEvent> DomainEvents { get; }
    }
}
