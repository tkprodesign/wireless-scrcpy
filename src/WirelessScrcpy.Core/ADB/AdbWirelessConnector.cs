using WirelessScrcpy.Core.Devices;

namespace WirelessScrcpy.Core.Adb;

public sealed class AdbWirelessConnector
{
    private readonly AdbClient _client;
    private readonly AdbCommandBuilder _commands;
    public AdbWirelessConnector(AdbClient client, AdbCommandBuilder commands) { _client = client; _commands = commands; }
    public Task<AdbCommandResult> ConnectAsync(NetworkEndpoint endpoint, CancellationToken cancellationToken = default) => _client.RunAsync(_commands.Connect(endpoint), cancellationToken);
}
