namespace WirelessScrcpy.App.UI;

public sealed class WinFormsDialogService
{
    public void ShowError(IWin32Window owner, string message) => MessageBox.Show(owner, message, "Wireless Scrcpy", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
