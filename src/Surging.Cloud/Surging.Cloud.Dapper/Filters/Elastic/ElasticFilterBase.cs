using Nest;
using Surging.Cloud.CPlatform.Utilities;
using Surging.Cloud.ElasticSearch.Provider;

namespace Surging.Cloud.Dapper.Filters.Elastic
{
    public abstract class ElasticFilterBase
    {
        private readonly IElasticSearchProvider _elasticSearchProvider;
        protected readonly ElasticClient _elasticClient;
        protected readonly bool _isUseElasticSearchModule;

        public ElasticFilterBase()
        {
            _elasticSearchProvider = ServiceLocator.GetService<IElasticSearchProvider>();
            _elasticClient = _elasticSearchProvider.GetElasticClient();
            _isUseElasticSearchModule = DbSetting.Instance.UseElasicSearchModule;
        }
    }
}
