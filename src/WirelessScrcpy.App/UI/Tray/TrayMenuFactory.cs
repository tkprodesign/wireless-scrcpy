namespace WirelessScrcpy.App.UI.Tray;

public sealed class TrayMenuFactory
{
    public ContextMenuStrip Create(Action showStatus, Action connect, Action disconnect, Action exit)
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Open Wireless Scrcpy", null, (_, _) => showStatus()).Name = TrayMenuItemNames.Show;
        menu.Items.Add("Connect", null, (_, _) => connect()).Name = TrayMenuItemNames.Connect;
        menu.Items.Add("Disconnect", null, (_, _) => disconnect()).Name = TrayMenuItemNames.Disconnect;
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => exit()).Name = TrayMenuItemNames.Exit;
        return menu;
    }
}

public static class TrayMenuItemNames
{
    public const string Show = "Show";
    public const string Connect = "Connect";
    public const string Disconnect = "Disconnect";
    public const string Exit = "Exit";
}
