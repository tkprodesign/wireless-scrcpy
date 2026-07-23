namespace WirelessScrcpy.Core.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
