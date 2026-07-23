using WirelessScrcpy.Core.Common;

namespace WirelessScrcpy.Core.Diagnostics;

public sealed record ToolDiscoveryResult(ToolLocation? Adb, ToolLocation? Scrcpy, AppError? Error)
{
    public bool IsSuccess => Error is null && Adb is not null && Scrcpy is not null;
}
