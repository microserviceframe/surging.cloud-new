﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Surging.Cloud.CPlatform.Utilities;

namespace Surging.Cloud.CPlatform.Module
{
    public class ModuleProvider: IModuleProvider
    {
        private readonly List<AbstractModule> _modules;
        private readonly string[] _virtualPaths;
        private readonly ILogger<ModuleProvider> _logger;

        /// <summary>
        /// 模块提供器 
        /// </summary>
        /// <param name="modules"></param>
        /// <param name="logger"></param>
        public ModuleProvider(List<AbstractModule> modules,
            string[] virtualPaths,
            ILogger<ModuleProvider> logger)
        {
            _modules = modules;
            _virtualPaths = virtualPaths;
            _logger = logger;
        }

        public List<AbstractModule> Modules { get => _modules; }

        public string[] VirtualPaths { get => _virtualPaths; }

        public virtual void Initialize()
        {
            _modules.ForEach(p =>
            {
                try
                {
                    Type[] types = { typeof(SystemModule), typeof(BusinessModule), typeof(EnginePartModule), typeof(AbstractModule) };
                    if (p.Enable && !p.IsInitialize)
                    {
                        p.Initialize(new AppModuleContext(_modules, _virtualPaths, ServiceLocator.Current));
                        p.IsInitialize = true;
                    }
                    
                    var type = p.GetType().BaseType;
                    if (types.Any(ty => ty == type))
                        p.Dispose();
                }
                catch(Exception ex)
                {
                    if (_logger.IsEnabled(LogLevel.Error))
                    {
                        _logger.LogError(ex.Message, ex);
                    }
                       
                    throw ex;
                }
            });

            WriteLog();
        }

        public void WriteLog()
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _modules.ForEach(p =>
                {
                    if (p.Enable)
                        _logger.LogDebug($"已初始化加载模块，类型：{p.TypeName}模块名：{p.ModuleName}。");
                });
            }
        }
    }
}
