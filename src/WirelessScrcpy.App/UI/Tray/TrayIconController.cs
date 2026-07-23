using WirelessScrcpy.Core.Workflow;

namespace WirelessScrcpy.App.UI.Tray;

public sealed class TrayIconController : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _menu;

    public TrayIconController(TrayMenuFactory menuFactory, Action showStatus, Action connect, Action disconnect, Action exit)
    {
        _menu = menuFactory.Create(showStatus, connect, disconnect, exit);
        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "Wireless Scrcpy - Idle",
            ContextMenuStrip = _menu,
            Visible = true
        };
        _notifyIcon.DoubleClick += (_, _) => showStatus();
        Update(WorkflowState.Idle, "Ready.");
    }

    public void Update(WorkflowState state, string detail)
    {
        string text = $"Wireless Scrcpy - {state}";
        _notifyIcon.Text = text.Length > 63 ? text[..63] : text;
        SetEnabled(TrayMenuItemNames.Connect, state is WorkflowState.Idle or WorkflowState.Completed or WorkflowState.Failed);
        SetEnabled(TrayMenuItemNames.Disconnect, state is not (WorkflowState.Idle or WorkflowState.Completed or WorkflowState.Failed or WorkflowState.Stopping));
    }

    public void ShowBalloon(string title, string message, ToolTipIcon icon) => _notifyIcon.ShowBalloonTip(3000, title, message, icon);

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _menu.Dispose();
    }

    private void SetEnabled(string name, bool enabled)
    {
        if (_menu.Items[name] is ToolStripItem item)
        {
            item.Enabled = enabled;
        }
    }
}
