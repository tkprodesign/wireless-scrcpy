namespace WirelessScrcpy.Core.Notifications;

public interface IUserNotifier
{
    Task NotifyAsync(NotificationMessage message, CancellationToken cancellationToken);
}
