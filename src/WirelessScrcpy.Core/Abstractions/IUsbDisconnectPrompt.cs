namespace WirelessScrcpy.Core.Abstractions;

public interface IUsbDisconnectPrompt
{
    Task<bool> ConfirmUsbDisconnectedAsync(CancellationToken cancellationToken = default);
}
