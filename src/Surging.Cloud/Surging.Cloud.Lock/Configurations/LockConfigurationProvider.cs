﻿using Microsoft.Extensions.Configuration;
using Surging.Cloud.CPlatform.Configurations.Remote;
using System.IO;

namespace Surging.Cloud.Lock.Configurations
{
    class LockConfigurationProvider : FileConfigurationProvider
    {
        public LockConfigurationProvider(LockConfigurationSource source) : base(source) { }

        /// <summary>
        /// 重写数据转换方法
        /// </summary>
        /// <param name="stream"></param>
        public override void Load(Stream stream)
        {
            var parser = new JsonConfigurationParser();
            this.Data = parser.Parse(stream, null);
        }
    }
}