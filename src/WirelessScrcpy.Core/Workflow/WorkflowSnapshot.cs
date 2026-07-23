using WirelessScrcpy.Core.Common;

namespace WirelessScrcpy.Core.Workflow;

public sealed record WorkflowSnapshot(WorkflowState State, string DetailMessage, DateTimeOffset TimestampUtc, string? DeviceIdentity, Severity Severity);
