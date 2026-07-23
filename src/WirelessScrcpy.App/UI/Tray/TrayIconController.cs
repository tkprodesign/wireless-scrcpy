namespace WirelessScrcpy.App.UI.Tray;

public sealed class TrayIconController : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _menu;

    public TrayIconController(TrayMenuFactory menuFactory, Action showStatus, Action launch, Action exit)
    {
        _menu = menuFactory.Create(showStatus, launch, exit);
        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "Wireless Scrcpy",
            ContextMenuStrip = _menu,
            Visible = true
        };
        _notifyIcon.DoubleClick += (_, _) => showStatus();
    }

    public void ShowBalloon(string title, string message, ToolTipIcon icon) => _notifyIcon.ShowBalloonTip(3000, title, message, icon);
    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _menu.Dispose();
    }
}
