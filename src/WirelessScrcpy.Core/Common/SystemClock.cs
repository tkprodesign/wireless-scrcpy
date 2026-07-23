using WirelessScrcpy.Core.Abstractions;

namespace WirelessScrcpy.Core.Common;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
