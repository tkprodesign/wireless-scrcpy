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
        // Strategy 1: ip -f inet addr show wlan0 — filtered IPv4 output for the wlan0 interface.
        // Output example: "    inet 192.168.1.5/24 brd 192.168.1.255 scope global wlan0"
        AdbCommandResult result = await _client.RunAsync(_commands.Shell(serial, "ip", "-f", "inet", "addr", "show", "wlan0"), cancellationToken).ConfigureAwait(false);
        if (result.ExitCode == 0)
        {
            string? ip = SelectFromInetLines(result.StandardOutput);
            if (ip is not null) return Result<string>.Success(ip);
        }

        // Strategy 2: ip route — extract the src address from wlan* interface lines only.
        // Output example: "192.168.45.0/24 dev wlan0 proto kernel scope link src 192.168.45.153"
        // The naive approach of extracting all IPv4 addresses from this output is unreliable:
        // network prefixes (e.g. 10.0.0.0, 192.168.45.0) appear before the actual src address
        // and would be selected first by a private-address filter.
        result = await _client.RunAsync(_commands.Shell(serial, "ip", "route"), cancellationToken).ConfigureAwait(false);
        if (result.ExitCode == 0)
        {
            string? ip = SelectFromIpRoute(result.StandardOutput);
            if (ip is not null) return Result<string>.Success(ip);
        }

        // Strategy 3: ip addr (all interfaces) — scan for a wlan interface block and extract
        // its inet addresses.
        result = await _client.RunAsync(_commands.Shell(serial, "ip", "addr"), cancellationToken).ConfigureAwait(false);
        if (result.ExitCode == 0)
        {
            string? ip = SelectFromIpAddr(result.StandardOutput);
            if (ip is not null) return Result<string>.Success(ip);
        }

        // Strategy 4: ifconfig wlan0 — last resort for older Android versions that lack iproute2.
        // Output example: "  inet addr:192.168.1.5  Bcast:192.168.1.255  Mask:255.255.255.0"
        result = await _client.RunAsync(_commands.Shell(serial, "ifconfig", "wlan0"), cancellationToken).ConfigureAwait(false);
        if (result.ExitCode == 0)
        {
            string? ip = SelectFromInetLines(result.StandardOutput);
            if (ip is not null) return Result<string>.Success(ip);
        }

        return Result<string>.Failure(new AppError(ErrorCode.PhoneIpDiscoveryFailed, "The phone IP address could not be discovered.", string.Empty, Severity.Error));
    }

    // Extracts the src IP address from ip route lines that belong to a wlan* interface.
    // Example line: "192.168.45.0/24 dev wlan0 proto kernel scope link src 192.168.45.153"
    // Only the "src <IP>" token on lines containing "dev wlan<suffix>" is considered,
    // so network prefixes (192.168.45.0) are never mistaken for the device's address.
    private string? SelectFromIpRoute(string output) =>
        _selector.Select(IpRouteSrcRegex().Matches(output).Select(m => m.Groups[1].Value));

    // Scans ip addr output for a wlan interface block and collects inet addresses from it.
    // Interface heading example: "5: wlan0: <BROADCAST,MULTICAST,UP,LOWER_UP> ..."
    // Address line example:      "    inet 192.168.45.153/24 brd 192.168.45.255 ..."
    private string? SelectFromIpAddr(string output)
    {
        bool inWlanBlock = false;
        var candidates = new List<string>();
        foreach (string line in output.Split('\n'))
        {
            if (InterfaceHeadingRegex().IsMatch(line))
            {
                inWlanBlock = line.Contains("wlan", StringComparison.OrdinalIgnoreCase);
                continue;
            }
            if (!inWlanBlock) continue;
            Match m = InetAddressRegex().Match(line);
            if (m.Success) candidates.Add(m.Groups[1].Value);
        }
        return _selector.Select(candidates);
    }

    // Extracts IP addresses from "inet [addr:]<IP>" lines.
    // Handles both iproute2 style ("inet 192.168.1.5/24") and
    // ifconfig style ("inet addr:192.168.1.5").
    private string? SelectFromInetLines(string output) =>
        _selector.Select(InetAddressRegex().Matches(output).Select(m => m.Groups[1].Value));

    // Matches "inet [addr:]<IP>" and captures just the address (no CIDR, no broadcast).
    [GeneratedRegex(@"\binet\s+(?:addr:)?(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\b")]
    private static partial Regex InetAddressRegex();

    // Matches "dev wlan<suffix> ... src <IP>" on a single line and captures the src IP.
    [GeneratedRegex(@"\bdev\s+wlan\S+\b.*?\bsrc\s+(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\b")]
    private static partial Regex IpRouteSrcRegex();

    // Matches an iproute2 interface heading line such as "5: wlan0: <FLAGS>".
    [GeneratedRegex(@"^\d+:\s+\S")]
    private static partial Regex InterfaceHeadingRegex();
}
