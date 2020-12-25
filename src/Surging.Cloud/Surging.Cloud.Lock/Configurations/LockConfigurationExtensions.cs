using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Surging.Cloud.CPlatform.Utilities;
using System.IO;

namespace Surging.Cloud.Lock.Configurations
{
    public static class LockConfigurationExtensions
    {
        public static IConfigurationBuilder AddLockFile(this IConfigurationBuilder builder, string path)
        {
            return AddLockFile(builder, provider: null, path: path, basePath: null, optional: false, reloadOnChange: false);
        }

        public static IConfigurationBuilder AddLockFile(this IConfigurationBuilder builder, string path, bool optional)
        {
            return AddLockFile(builder, provider: null, path: path, basePath: null, optional: optional, reloadOnChange: false);
        }

        public static IConfigurationBuilder AddLockFile(this IConfigurationBuilder builder, string path, bool optional, bool reloadOnChange)
        {
            return AddLockFile(builder, provider: null, path: path, basePath: null, optional: optional, reloadOnChange: reloadOnChange);
        }

        public static IConfigurationBuilder AddLockFile(this IConfigurationBuilder builder, string path, string basePath, bool optional, bool reloadOnChange)
        {
            return AddLockFile(builder, provider: null, path: path, basePath: basePath, optional: optional, reloadOnChange: reloadOnChange);
        }

        public static IConfigurationBuilder AddLockFile(this IConfigurationBuilder builder, IFileProvider provider, string path, string basePath, bool optional, bool reloadOnChange)
        {
            Check.NotNull(builder, "builder");
            //获取一个环境变量的路径
            Check.CheckCondition(() => string.IsNullOrEmpty(path), "path");
            path = CPlatform.Utilities.EnvironmentHelper.GetEnvironmentVariable(path);
            if (provider == null && Path.IsPathRooted(path))
            {
                provider = new PhysicalFileProvider(Path.GetDirectoryName(path));
                path = Path.GetFileName(path);
            }
            //建立CacheConfigurationSource类，此类继承了FileConfigurationSource接口，并重写加入了json转换方法
            var source = new LockConfigurationSource
            {
                FileProvider = provider,
                Path = path,
                Optional = optional,
                ReloadOnChange = reloadOnChange
            };
            builder.Add(source);
            if (!string.IsNullOrEmpty(basePath))
                builder.SetBasePath(basePath);
            AppConfig.Path = path;
            AppConfig.Configuration = builder.Build();
            return builder;
        }
    }
}