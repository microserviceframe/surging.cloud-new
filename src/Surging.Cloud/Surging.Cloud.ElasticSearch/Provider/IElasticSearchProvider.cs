using Elasticsearch.Net;
using Nest;

namespace Surging.Cloud.ElasticSearch.Provider
{
    public interface IElasticSearchProvider
    {
        //ElasticLowLevelClient GetElasticLowLevelClient();

        ElasticClient GetElasticClient();
    }
}
