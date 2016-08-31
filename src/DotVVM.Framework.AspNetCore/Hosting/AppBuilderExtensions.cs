using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using DotVVM.Framework.Configuration;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using DotVVM.Framework.Hosting.Middlewares;

namespace DotVVM.Framework.Hosting
{
    public static class AppBuilderExtensions
    {
        public static DotvvmConfiguration CreateConfiguration(string applicationRootDirectory, IServiceProvider serviceProvider)
        {
            // load or create default configuration
            var configuration = serviceProvider.GetService<DotvvmConfiguration>();
            configuration.ApplicationPhysicalPath = applicationRootDirectory;
            return configuration;
        }

        internal static DotvvmConfiguration UseDotVVM(this IApplicationBuilder app, string applicationRootDirectory, bool errorPages = true)
        {
            var configuration = CreateConfiguration(applicationRootDirectory, app.ApplicationServices);
#if Owin
            configuration.ServiceLocator.RegisterSingleton<IDataProtectionProvider>(app.GetDataProtectionProvider);
#endif
            
            // add middlewares
            if (errorPages)
                app.UseMiddleware<DotvvmErrorPageMiddleware>();
            
            app.UseMiddleware<DotvvmEmbeddedResourceMiddleware>();
            app.UseMiddleware<DotvvmFileUploadMiddleware>(configuration);
            app.UseMiddleware<JQueryGlobalizeCultureMiddleware>();

            app.UseMiddleware<DotvvmReturnedFileMiddleware>(configuration);

            app.UseMiddleware<DotvvmMiddleware>(configuration);

            return configuration;
        }

        public static DotvvmConfiguration UseDotVVM<TStartup>(this IApplicationBuilder app, string applicationRootDirectory, bool errorPages = true)
            where TStartup: IDotvvmStartup, new()
        {
            var config = app.UseDotVVM(applicationRootDirectory, errorPages);
            new TStartup().Configure(config, applicationRootDirectory);
            return config;
        }
    }
}