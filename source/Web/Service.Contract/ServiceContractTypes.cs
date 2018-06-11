﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Karambolo.Common;
using Karambolo.Common.Collections;
using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.Service.Contract.DataObjects;
using AspNetSkeleton.Service.Contract.Queries;

namespace AspNetSkeleton.Service.Contract
{
    public static class ServiceContractTypes
    {
        static ServiceContractTypes()
        {
            var assemblyTypes = typeof(UserData).Assembly.GetTypes();

            DataObjectTypes = Enumerable.ToHashSet(assemblyTypes
                .Where(t => 
                    t.IsClass && !t.IsAbstract && !t.HasAttribute<CompilerGeneratedAttribute>() && 
                    t.Namespace.StartsWith(typeof(UserData).Namespace))).AsReadOnly();

            QueryTypes = Enumerable.ToHashSet(assemblyTypes
                .Where(t => 
                    t.IsClass && !t.IsAbstract && !t.HasAttribute<CompilerGeneratedAttribute>() && 
                    t.Namespace.StartsWith(typeof(GetUserQuery).Namespace))).AsReadOnly();

            CommandTypes = Enumerable.ToHashSet(assemblyTypes
                .Where(
                    t => t.IsClass && !t.IsAbstract && !t.HasAttribute<CompilerGeneratedAttribute>() && 
                    t.Namespace.StartsWith(typeof(CreateUserCommand).Namespace))).AsReadOnly();
        }

        public static readonly IReadOnlySet<Type> DataObjectTypes;
        public static readonly IReadOnlySet<Type> QueryTypes;
        public static readonly IReadOnlySet<Type> CommandTypes;
    }
}