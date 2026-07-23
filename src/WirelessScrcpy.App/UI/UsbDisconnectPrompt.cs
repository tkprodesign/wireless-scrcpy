using WirelessScrcpy.Core.Abstractions;

namespace WirelessScrcpy.App.UI;

public sealed class UsbDisconnectPrompt : IUsbDisconnectPrompt
{
    public Task<bool> ConfirmUsbDisconnectedAsync(CancellationToken cancellationToken = default)
    {
        bool confirmed = MessageBox.Show(
            "Disconnect the USB cable, then click OK to continue.",
            "Wireless Scrcpy",
            MessageBoxButtons.OKCancel,
            MessageBoxIcon.Information) == DialogResult.OK;
        return Task.FromResult(confirmed);
    }
}
