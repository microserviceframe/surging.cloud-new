using MongoDB.Driver;
using Surging.Cloud.CPlatform.Ioc;

namespace Surging.Cloud.MongoDb.Provider
{
    public interface IMongoDatabaseProvider: ITransientDependency
    {

        MongoDatabase GetDatabase();
    }
}
