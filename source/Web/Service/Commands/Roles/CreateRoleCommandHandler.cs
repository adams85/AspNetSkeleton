﻿using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.DataAccess.Entities;
using AspNetSkeleton.Service.Transforms;
using System.Threading.Tasks;
using System.Threading;
using System.Data.Entity;

namespace AspNetSkeleton.Service.Commands.Roles
{
    public class CreateRoleCommandHandler : ICommandHandler<CreateRoleCommand>
    {
        readonly ICommandContext _commandContext;

        public CreateRoleCommandHandler(ICommandContext commandContext)
        {
            _commandContext = commandContext;
        }

        public async Task HandleAsync(CreateRoleCommand command, CancellationToken cancellationToken)
        {
            this.RequireSpecified(command.RoleName, c => c.RoleName);
            this.RequireValid(command.RoleName.IndexOf(',') < 0, c => c.RoleName);

            using (var scope = _commandContext.CreateDataAccessScope())
            {
                this.RequireUnique(
                    await scope.Context.Query<Role>().FilterByName(command.RoleName).AnyAsync(cancellationToken).ConfigureAwait(false),
                    c => c.RoleName);

                var role = new Role
                {
                    RoleName = command.RoleName
                };

                scope.Context.Create(role);

                await scope.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                command.OnKeyGenerated?.Invoke(command, role.RoleId);
            }
        }
    }
}