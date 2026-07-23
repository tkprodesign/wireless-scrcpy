using WirelessScrcpy.Core.Common;
using WirelessScrcpy.Core.Notifications;

namespace WirelessScrcpy.App.UI.Notifications;

public sealed class TrayBalloonNotifier : IUserNotifier
{
    private static readonly TimeSpan DisplayDuration = TimeSpan.FromSeconds(3);

    public async Task NotifyAsync(NotificationMessage message, CancellationToken cancellationToken)
    {
        ToolTipIcon icon = message.Severity switch
        {
            Severity.Warning => ToolTipIcon.Warning,
            Severity.Error => ToolTipIcon.Error,
            _ => ToolTipIcon.Info
        };
        using var notifyIcon = new NotifyIcon { Icon = SystemIcons.Application, Visible = true, Text = "Wireless Scrcpy" };
        notifyIcon.ShowBalloonTip((int)DisplayDuration.TotalMilliseconds, message.Title, message.Body, icon);
        await Task.Delay(DisplayDuration, cancellationToken).ConfigureAwait(false);
    }
}
