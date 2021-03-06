﻿using System.Linq;
using AspNetSkeleton.Service.Contract.Commands;
using AspNetSkeleton.DataAccess.Entities;
using System.Threading.Tasks;
using System.Threading;
using AspNetSkeleton.DataAccess;

namespace AspNetSkeleton.Service.Commands.Notifications
{
    public class MarkNotificationsCommandHandler : ICommandHandler<MarkNotificationsCommand>
    {
        readonly ICommandContext _commandContext;

        public MarkNotificationsCommandHandler(ICommandContext commandContext)
        {
            _commandContext = commandContext;
        }

        public async Task HandleAsync(MarkNotificationsCommand command, CancellationToken cancellationToken)
        {
            using (var scope = _commandContext.CreateDataAccessScope())
            {
                IQueryable<Notification> linq = scope.Context.Query<Notification>();

                if (command.Id != null)
                    linq = linq.Where(n => n.Id.Value == command.Id.Value);

                if (command.State != null)
                    linq = linq.Where(n => (n.State & command.State.Value) == n.State);

                if (command.Count != null)
                    linq = linq.Take(command.Count.Value);

                var notifications = await linq.OrderBy(m => m.CreatedAt).ToArrayAsync(cancellationToken).ConfigureAwait(false);

                foreach (var notification in notifications)
                {
                    scope.Context.Track(notification);

                    notification.State = command.NewState;

                    scope.Context.Update(notification);
                }

                await scope.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
