namespace WirelessScrcpy.Core.Adb;

public sealed class AdbServerManager
{
    private readonly AdbClient _client;
    public AdbServerManager(AdbClient client) => _client = client;
    public Task<AdbCommandResult> EnsureStartedAsync(CancellationToken cancellationToken = default) => _client.StartServerAsync(cancellationToken);
}
