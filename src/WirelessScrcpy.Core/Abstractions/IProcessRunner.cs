using WirelessScrcpy.Core.Diagnostics;

namespace WirelessScrcpy.Core.Abstractions;

public interface IProcessRunner
{
    Task<ProcessRunResult> RunAsync(ProcessStartRequest request, CancellationToken cancellationToken = default);
    ProcessHandle Start(ProcessStartRequest request);
}
