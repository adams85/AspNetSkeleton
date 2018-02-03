﻿using System.IO;
using AspNetSkeleton.Base;
using AspNetSkeleton.Core.Hosting;
using AspNetSkeleton.Core.Hosting.Operations;
using DasMulli.Win32.ServiceUtils;

namespace AspNetSkeleton.Service.Host.Hosting
{
    public class HostWindowsService : HostWindowsServiceBase
    {
        const string name = "AspNetSkeleton.Service";
        const string description = "AspNetSkeleton logic layer component implemented as an ASP.NET Core application.";

        public HostWindowsService(IHost host, IAppEnvironment environment) : base(host)
        {
            Definition = new ServiceDefinitionBuilder()
                .WithServiceName(name)
                .WithDisplayName(name)
                .WithDescription(description)
                .WithBinaryPath($"dotnet.exe \"{Path.Combine(environment.AppBasePath, environment.AppName + ".dll")}\" {ServiceOperation.Name}")
                .WithCredentials(Win32ServiceCredentials.LocalSystem)
                .WithAutoStart(true)
                .Build();
        }

        public override ServiceDefinition Definition { get; }
    }
}
