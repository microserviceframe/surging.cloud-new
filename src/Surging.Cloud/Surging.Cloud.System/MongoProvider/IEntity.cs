using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Surging.Cloud.System.MongoProvider
{
    public interface IEntity
    {
        [BsonId]
        string Id { get; set; }
    }
}
