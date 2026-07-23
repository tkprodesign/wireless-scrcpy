namespace WirelessScrcpy.Core.Devices;

public sealed record NetworkEndpoint(string IpAddress, int Port)
{
    public override string ToString() => $"{IpAddress}:{Port}";
}
