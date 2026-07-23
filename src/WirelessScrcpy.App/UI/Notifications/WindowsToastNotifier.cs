using WirelessScrcpy.Core.Notifications;

namespace WirelessScrcpy.App.UI.Notifications;

public sealed class WindowsToastNotifier : IUserNotifier
{
    public Task NotifyAsync(NotificationMessage message, CancellationToken cancellationToken) =>
        throw new InvalidOperationException("Native toast registration is unavailable in the foundation host; tray notifications are used instead.");
}
