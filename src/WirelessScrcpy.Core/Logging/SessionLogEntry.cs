using WirelessScrcpy.Core.Common;

namespace WirelessScrcpy.Core.Logging;

public sealed record SessionLogEntry(DateTimeOffset TimestampUtc, Severity Level, string EventName, string Message);
