using WirelessScrcpy.Core.Common;
using WirelessScrcpy.Core.Devices;

namespace WirelessScrcpy.Core.Adb;

public sealed class AdbManager
{
    private readonly AdbClient _client;
    private readonly AdbOutputParser _parser;
    private readonly AdbDeviceSelector _selector;
    private readonly AdbServerManager _serverManager;
    private readonly AdbTcpIpService _tcpIpService;
    private readonly AdbWirelessConnector _wirelessConnector;

    public AdbManager(
        AdbClient client,
        AdbOutputParser parser,
        AdbDeviceSelector selector,
        AdbServerManager serverManager,
        AdbTcpIpService tcpIpService,
        AdbWirelessConnector wirelessConnector)
    {
        _client = client;
        _parser = parser;
        _selector = selector;
        _serverManager = serverManager;
        _tcpIpService = tcpIpService;
        _wirelessConnector = wirelessConnector;
    }

    public void ConfigurePath(string adbPath) => _client.ConfigurePath(adbPath);

    public async Task<Result<bool>> EnsureServerStartedAsync(CancellationToken cancellationToken = default)
    {
        AdbCommandResult result = await _serverManager.EnsureStartedAsync(cancellationToken).ConfigureAwait(false);
        return result.ExitCode == 0
            ? Result<bool>.Success(true)
            : Result<bool>.Failure(new AppError(ErrorCode.ProcessStartFailed, "ADB server could not be started.", result.StandardError, Severity.Error));
    }

    public async Task<Result<AndroidDevice>> DetectSingleUsbDeviceAsync(CancellationToken cancellationToken = default)
    {
        Result<IReadOnlyList<AndroidDevice>> devices = await ListDevicesAsync(cancellationToken).ConfigureAwait(false);
        return devices.IsSuccess ? _selector.SelectSingleUsbDevice(devices.Value) : Result<AndroidDevice>.Failure(devices.Error!);
    }

    public async Task<Result<IReadOnlyList<AndroidDevice>>> ListDevicesAsync(CancellationToken cancellationToken = default)
    {
        AdbCommandResult result = await _client.ListDevicesAsync(cancellationToken).ConfigureAwait(false);
        if (result.ExitCode != 0)
        {
            return Result<IReadOnlyList<AndroidDevice>>.Failure(new AppError(ErrorCode.NoUsbDevice, "Unable to list ADB devices.", result.StandardError, Severity.Error));
        }
        return Result<IReadOnlyList<AndroidDevice>>.Success(_parser.ParseDevices(result.StandardOutput));
    }

    public async Task<Result<bool>> EnableTcpIpAsync(string serial, int port, CancellationToken cancellationToken = default)
    {
        AdbCommandResult result = await _tcpIpService.EnableAsync(serial, port, cancellationToken).ConfigureAwait(false);
        return _parser.IsTcpIpEnabled(result) || result.ExitCode == 0
            ? Result<bool>.Success(true)
            : Result<bool>.Failure(new AppError(ErrorCode.TcpIpEnablementFailed, "ADB TCP/IP mode could not be enabled.", result.StandardError + result.StandardOutput, Severity.Error));
    }

    public async Task<Result<bool>> ConnectWirelessAsync(NetworkEndpoint endpoint, CancellationToken cancellationToken = default)
    {
        AdbCommandResult result = await _wirelessConnector.ConnectAsync(endpoint, cancellationToken).ConfigureAwait(false);
        return _parser.IsConnected(result)
            ? Result<bool>.Success(true)
            : Result<bool>.Failure(new AppError(ErrorCode.WirelessConnectionFailed, "Wireless ADB connection failed.", result.StandardError + result.StandardOutput, Severity.Error));
    }

    public async Task<Result<AndroidDevice>> VerifyWirelessDeviceAsync(NetworkEndpoint endpoint, CancellationToken cancellationToken = default)
    {
        Result<IReadOnlyList<AndroidDevice>> devices = await ListDevicesAsync(cancellationToken).ConfigureAwait(false);
        if (!devices.IsSuccess) return Result<AndroidDevice>.Failure(devices.Error!);
        AndroidDevice? match = devices.Value.FirstOrDefault(device =>
            device.ConnectionType == DeviceConnectionType.Wireless &&
            device.State == DeviceState.Authorized &&
            string.Equals(device.Serial, endpoint.ToString(), StringComparison.OrdinalIgnoreCase));
        return match is null
            ? Result<AndroidDevice>.Failure(new AppError(ErrorCode.WirelessConnectionFailed, "Wireless ADB connection could not be verified.", $"{endpoint} was not present as an authorized ADB device.", Severity.Error))
            : Result<AndroidDevice>.Success(match);
    }

    public Task<AdbCommandResult> DisconnectAsync(string serial, CancellationToken cancellationToken = default) => _client.DisconnectAsync(serial, cancellationToken);
}
