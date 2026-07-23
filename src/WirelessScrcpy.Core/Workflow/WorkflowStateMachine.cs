using WirelessScrcpy.Core.Abstractions;
using WirelessScrcpy.Core.Common;

namespace WirelessScrcpy.Core.Workflow;

public sealed class WorkflowStateMachine
{
    private readonly IClock _clock;
    public WorkflowState CurrentState { get; private set; } = WorkflowState.Idle;
    public event EventHandler<WorkflowSnapshot>? SnapshotChanged;

    public WorkflowStateMachine(IClock clock) => _clock = clock;

    public bool TryApply(WorkflowEvent workflowEvent, string detailMessage, string? deviceIdentity = null, Severity severity = Severity.Info)
    {
        WorkflowState? next = GetNextState(CurrentState, workflowEvent);
        if (next is null) return false;
        CurrentState = next.Value;
        SnapshotChanged?.Invoke(this, new WorkflowSnapshot(CurrentState, detailMessage, _clock.UtcNow, deviceIdentity, severity));
        return true;
    }

    private static WorkflowState? GetNextState(WorkflowState state, WorkflowEvent workflowEvent)
    {
        if (workflowEvent == WorkflowEvent.StopRequested && state is not WorkflowState.Idle and not WorkflowState.Completed and not WorkflowState.Failed) return WorkflowState.Stopping;
        if (workflowEvent == WorkflowEvent.LaunchRequested && state is WorkflowState.Completed or WorkflowState.Failed) return WorkflowState.Starting;
        if (IsFailureEvent(workflowEvent) && state is not WorkflowState.Idle and not WorkflowState.Completed and not WorkflowState.Failed) return WorkflowState.Failed;
        return (state, workflowEvent) switch
        {
            (WorkflowState.Idle, WorkflowEvent.LaunchRequested) => WorkflowState.Starting,
            (WorkflowState.Starting, WorkflowEvent.WorkflowInitialized) => WorkflowState.DetectingAdb,
            (WorkflowState.DetectingAdb, WorkflowEvent.AdbFound) => WorkflowState.DetectingScrcpy,
            (WorkflowState.DetectingAdb, WorkflowEvent.AdbMissing) => WorkflowState.Failed,
            (WorkflowState.DetectingScrcpy, WorkflowEvent.ScrcpyFound) => WorkflowState.StartingAdbServer,
            (WorkflowState.DetectingScrcpy, WorkflowEvent.ScrcpyMissing) => WorkflowState.Failed,
            (WorkflowState.StartingAdbServer, WorkflowEvent.AdbServerReady) => WorkflowState.DetectingUsbDevice,
            (WorkflowState.DetectingUsbDevice, WorkflowEvent.SingleUsbDeviceFound) => WorkflowState.EnablingTcpIp,
            (WorkflowState.DetectingUsbDevice, WorkflowEvent.NoUsbDeviceFound) => WorkflowState.Failed,
            (WorkflowState.DetectingUsbDevice, WorkflowEvent.MultipleUsbDevicesFound) => WorkflowState.Failed,
            (WorkflowState.DetectingUsbDevice, WorkflowEvent.UsbDeviceUnauthorized) => WorkflowState.Failed,
            (WorkflowState.EnablingTcpIp, WorkflowEvent.TcpIpEnabled) => WorkflowState.DiscoveringPhoneIp,
            (WorkflowState.EnablingTcpIp, WorkflowEvent.TcpIpFailed) => WorkflowState.Failed,
            (WorkflowState.DiscoveringPhoneIp, WorkflowEvent.PhoneIpDiscovered) => WorkflowState.PromptingUsbDisconnect,
            (WorkflowState.DiscoveringPhoneIp, WorkflowEvent.PhoneIpMissing) => WorkflowState.Failed,
            (WorkflowState.PromptingUsbDisconnect, WorkflowEvent.UserConfirmedUsbDisconnected) => WorkflowState.ConnectingWirelessAdb,
            (WorkflowState.PromptingUsbDisconnect, WorkflowEvent.UserCancelled) => WorkflowState.Stopping,
            (WorkflowState.ConnectingWirelessAdb, WorkflowEvent.WirelessAdbConnected) => WorkflowState.LaunchingScrcpy,
            (WorkflowState.ConnectingWirelessAdb, WorkflowEvent.WirelessAdbFailed) => WorkflowState.Failed,
            (WorkflowState.LaunchingScrcpy, WorkflowEvent.ScrcpyStarted) => WorkflowState.Running,
            (WorkflowState.LaunchingScrcpy, WorkflowEvent.ScrcpyLaunchFailed) => WorkflowState.Failed,
            (WorkflowState.Running, WorkflowEvent.NetworkInterrupted) => WorkflowState.Reconnecting,
            (WorkflowState.Reconnecting, WorkflowEvent.ReconnectSucceeded) => WorkflowState.Running,
            (WorkflowState.Reconnecting, WorkflowEvent.ReconnectExpired) => WorkflowState.Failed,
            (WorkflowState.Stopping, WorkflowEvent.Stopped) => WorkflowState.Completed,
            _ => null
        };
    }

    private static bool IsFailureEvent(WorkflowEvent workflowEvent) => workflowEvent is
        WorkflowEvent.AdbMissing or
        WorkflowEvent.ScrcpyMissing or
        WorkflowEvent.NoUsbDeviceFound or
        WorkflowEvent.MultipleUsbDevicesFound or
        WorkflowEvent.UsbDeviceUnauthorized or
        WorkflowEvent.TcpIpFailed or
        WorkflowEvent.PhoneIpMissing or
        WorkflowEvent.WirelessAdbFailed or
        WorkflowEvent.ScrcpyLaunchFailed or
        WorkflowEvent.ReconnectExpired;
}

