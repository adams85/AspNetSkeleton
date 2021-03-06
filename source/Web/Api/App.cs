﻿using System.Collections.Generic;
using System.IO;
using AspNetSkeleton.Core;
using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace AspNetSkeleton.Api
{
    public class App : AppBase
    {
        readonly string _listenUrl;

        public App(IEnumerable<IAppConfiguration> configurations, TextWriter statusWriter, IComponentContext context)
            : base(configurations, statusWriter, context)
        {
            var settings = context.Resolve<IOptions<ApiSettings>>().Value;
            _listenUrl = settings.ListenUrl;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseUrls(_listenUrl);
        }
    }
}
