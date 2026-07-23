namespace WirelessScrcpy.Core.Notifications;

public sealed class NotificationManager
{
    private readonly IUserNotifier _notifier;

    public NotificationManager(IUserNotifier notifier) => _notifier = notifier;

    public Task NotifyAsync(NotificationMessage message, CancellationToken cancellationToken = default) =>
        _notifier.NotifyAsync(message, cancellationToken);
}
