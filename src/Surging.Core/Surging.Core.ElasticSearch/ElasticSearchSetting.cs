namespace Surging.Core.ElasticSearch
{
    public class ElasticSearchSetting
    {
        public string Address { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string DefaultIndexName { get; set; } = "default";

        public int ConnectionLimit { get; set; } = 80;

        public int RequestTimeout { get; set; } = 5000;

        //public string Analyzer = "standard";

        //public string SearchAnalyzer = "standard";

        public int NumberOfShards { get; set; } = 12;

        public int NumberOfReplicas { get; set; } = 1;

        public static ElasticSearchSetting Instance { get; internal set; }
    }
}
