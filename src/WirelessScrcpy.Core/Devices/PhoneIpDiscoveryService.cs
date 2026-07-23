using System.Text.RegularExpressions;
using WirelessScrcpy.Core.Adb;
using WirelessScrcpy.Core.Common;

namespace WirelessScrcpy.Core.Devices;

public sealed partial class PhoneIpDiscoveryService
{
    private readonly AdbClient _client;
    private readonly AdbCommandBuilder _commands;
    private readonly IpAddressCandidateSelector _selector;

    public PhoneIpDiscoveryService(AdbClient client, AdbCommandBuilder commands, IpAddressCandidateSelector selector)
    {
        _client = client;
        _commands = commands;
        _selector = selector;
    }

    public async Task<Result<string>> DiscoverAsync(string serial, CancellationToken cancellationToken = default)
    {
        AdbCommandResult result = await _client.RunAsync(_commands.Shell(serial, "ip", "-f", "inet", "addr", "show", "wlan0"), cancellationToken).ConfigureAwait(false);
        if (result.ExitCode != 0)
        {
            result = await _client.RunAsync(_commands.Shell(serial, "ip", "route"), cancellationToken).ConfigureAwait(false);
        }
        if (result.ExitCode != 0)
        {
            return Result<string>.Failure(new AppError(ErrorCode.PhoneIpDiscoveryFailed, "The phone IP address could not be discovered.", result.StandardError, Severity.Error));
        }
        string? selected = _selector.Select(AddressRegex().Matches(result.StandardOutput).Select(match => match.Value));
        return selected is null
            ? Result<string>.Failure(new AppError(ErrorCode.PhoneIpDiscoveryFailed, "The phone IP address could not be discovered.", result.StandardOutput, Severity.Error))
            : Result<string>.Success(selected);
    }

    [GeneratedRegex(@"\b(?:\d{1,3}\.){3}\d{1,3}\b")]
    private static partial Regex AddressRegex();
}
