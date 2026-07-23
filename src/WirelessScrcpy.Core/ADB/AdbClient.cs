using WirelessScrcpy.Core.Abstractions;
using WirelessScrcpy.Core.Diagnostics;

namespace WirelessScrcpy.Core.Adb;

public sealed class AdbClient
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(15);
    private readonly IProcessRunner _runner;
    private readonly AdbCommandBuilder _commands;

    public AdbClient(IProcessRunner runner, AdbCommandBuilder commands)
    {
        _runner = runner;
        _commands = commands;
    }

    public string AdbPath { get; private set; } = "adb.exe";
    public void ConfigurePath(string adbPath) => AdbPath = adbPath;

    public async Task<AdbCommandResult> RunAsync(IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
    {
        ProcessRunResult result = await _runner.RunAsync(new ProcessStartRequest(AdbPath, arguments, Timeout: DefaultTimeout), cancellationToken).ConfigureAwait(false);
        return new AdbCommandResult(result.ExitCode, result.StandardOutput, result.StandardError, result.Duration);
    }

    public Task<AdbCommandResult> StartServerAsync(CancellationToken cancellationToken = default) => RunAsync(_commands.StartServer(), cancellationToken);
    public Task<AdbCommandResult> ListDevicesAsync(CancellationToken cancellationToken = default) => RunAsync(_commands.DevicesLong(), cancellationToken);
    public Task<AdbCommandResult> DisconnectAsync(string serial, CancellationToken cancellationToken = default) => RunAsync(_commands.Disconnect(serial), cancellationToken);
}
