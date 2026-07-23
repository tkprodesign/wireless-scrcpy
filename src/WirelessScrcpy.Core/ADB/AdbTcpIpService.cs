namespace WirelessScrcpy.Core.Adb;

public sealed class AdbTcpIpService
{
    private readonly AdbClient _client;
    private readonly AdbCommandBuilder _commands;
    public AdbTcpIpService(AdbClient client, AdbCommandBuilder commands) { _client = client; _commands = commands; }
    public Task<AdbCommandResult> EnableAsync(string serial, int port, CancellationToken cancellationToken = default) => _client.RunAsync(_commands.TcpIp(serial, port), cancellationToken);
}
