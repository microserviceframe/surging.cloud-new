﻿
namespace Surging.Core.Dapper
{
    public class DbSetting
    {
        public DbType DbType { get; set; }

        public string ConnectionString { get; set; }

        public bool UseElasicSearchModule { get; internal set; }

        public static DbSetting Instance { get; internal set; }
    }
}
