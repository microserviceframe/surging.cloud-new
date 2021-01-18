﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Surging.Cloud.CPlatform;
using Surging.Cloud.CPlatform.Module;
using Surging.Cloud.CPlatform.Runtime.Server;
using Surging.Cloud.KestrelHttpServer;
using Surging.Cloud.Swagger.Builder;
using Surging.Cloud.Swagger.Internal;
using Surging.Cloud.Swagger.Swagger.Filters;
using Surging.Cloud.Swagger.SwaggerUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Autofac;

namespace Surging.Cloud.Swagger
{
    public class SwaggerModule: KestrelHttpModule
    {
        private  IServiceSchemaProvider _serviceSchemaProvider; 
        private  IServiceEntryProvider _serviceEntryProvider;

        public override void Initialize(AppModuleContext context)
        {
            var serviceProvider = context.ServiceProvoider;
            _serviceSchemaProvider = serviceProvider.Resolve<IServiceSchemaProvider>();
            _serviceEntryProvider = serviceProvider.Resolve<IServiceEntryProvider>();
        }

        public override void Initialize(ApplicationInitializationContext context)
        {
            var info = AppConfig.SwaggerConfig.Info == null
          ? AppConfig.SwaggerOptions : AppConfig.SwaggerConfig.Info;
            if (info != null)
            {
                context.Builder.UseSwagger();
                context.Builder.UseSwaggerUI(c =>
                {
                    var areaName = AppConfig.SwaggerConfig.Options?.IngressName;
                    c.SwaggerEndpoint($"../swagger/{info.Version}/swagger.json", info.Title, areaName);
                    var isOnlyGenerateLocalHostDocs = AppConfig.SwaggerConfig.Options?.IsOnlyGenerateLocalHostDocs;
                    if (isOnlyGenerateLocalHostDocs != null && isOnlyGenerateLocalHostDocs.Value)
                    {
                        c.SwaggerEndpoint(_serviceEntryProvider.GetEntries(), areaName);
                    }
                    else {
                        c.SwaggerEndpoint(_serviceEntryProvider.GetALLEntries(), areaName);
                    }
                   
                });
            }
        }

        public override void RegisterBuilder(ConfigurationContext context)
        {
            var serviceCollection = context.Services;
            var info = AppConfig.SwaggerConfig.Info == null
                     ? AppConfig.SwaggerOptions : AppConfig.SwaggerConfig.Info;
            var swaggerOptions = AppConfig.SwaggerConfig.Options;
            if (info != null)
            {
                serviceCollection.AddSwaggerGen(options =>
                {
                    if (context.Modules.Any(p => p.ModuleName == "StageModule" && p.Enable)) {
                        options.OperationFilter<AddAuthorizationOperationFilter>();
                    }
                    options.SwaggerDoc(info.Version, info);
                    if (swaggerOptions != null && swaggerOptions.IgnoreFullyQualified)
                    {
                        options.IgnoreFullyQualified();
                    }
                    options.GenerateSwaggerDoc(_serviceEntryProvider.GetALLEntries());
                    options.DocInclusionPredicateV2((docName, apiDesc) =>
                    {
                        if (docName == info.Version)
                            return true;
                        var assembly = apiDesc.Type.Assembly;

                        var title = assembly
                            .GetCustomAttributes(true)
                            .OfType<AssemblyTitleAttribute>();

                        return title.Any(v => v.Title == docName);
                    });
                    var xmlPaths = _serviceSchemaProvider.GetSchemaFilesPath();
                    foreach (var xmlPath in xmlPaths)
                    {
                        options.IncludeXmlComments(xmlPath);
                    }
                    options.CustomSchemaIds((type) => type.FullName);
                });
            }
        }

        /// <summary>
        /// Inject dependent third-party components
        /// </summary>
        /// <param name="builder"></param>
        protected override void RegisterBuilder(ContainerBuilderWrapper builder)
        {
            var section = CPlatform.AppConfig.GetSection("Swagger");
            if (section.Exists())
            {
                AppConfig.SwaggerOptions = section.Get<Info>();
                AppConfig.SwaggerConfig = section.Get<DocumentConfiguration>();
            }
            builder.RegisterType(typeof(DefaultServiceSchemaProvider)).As(typeof(IServiceSchemaProvider)).SingleInstance();

        }
    }
}
