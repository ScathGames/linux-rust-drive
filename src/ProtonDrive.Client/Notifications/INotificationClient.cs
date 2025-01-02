using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ProtonDrive.Client.Notifications.Contracts;

namespace ProtonDrive.Client.Notifications;

public interface INotificationClient
{
    Task<IReadOnlyCollection<Notification>> GetNotificationsAsync(string userSubscriptionPlanCode, CancellationToken cancellationToken);
}
