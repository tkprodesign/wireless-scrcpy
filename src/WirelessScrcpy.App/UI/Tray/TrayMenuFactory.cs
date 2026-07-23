namespace WirelessScrcpy.App.UI.Tray;

public sealed class TrayMenuFactory
{
    public ContextMenuStrip Create(Action showStatus, Action launch, Action exit)
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Show Status", null, (_, _) => showStatus());
        menu.Items.Add("Launch", null, (_, _) => launch());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => exit());
        return menu;
    }
}
