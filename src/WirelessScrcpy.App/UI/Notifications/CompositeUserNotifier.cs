using WirelessScrcpy.Core.Notifications;

namespace WirelessScrcpy.App.UI.Notifications;

public sealed class CompositeUserNotifier : IUserNotifier
{
    private readonly IUserNotifier _primary;
    private readonly IUserNotifier _fallback;

    public CompositeUserNotifier(WindowsToastNotifier primary, TrayBalloonNotifier fallback)
    {
        _primary = primary;
        _fallback = fallback;
    }

    public async Task NotifyAsync(NotificationMessage message, CancellationToken cancellationToken)
    {
        try
        {
            await _primary.NotifyAsync(message, cancellationToken).ConfigureAwait(false);
        }
        catch (InvalidOperationException)
        {
            await _fallback.NotifyAsync(message, cancellationToken).ConfigureAwait(false);
        }
    }
}
