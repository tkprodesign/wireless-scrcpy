namespace WirelessScrcpy.Core.Workflow;

public enum WorkflowState
{
    Idle,
    Starting,
    DetectingAdb,
    DetectingScrcpy,
    StartingAdbServer,
    DetectingUsbDevice,
    EnablingTcpIp,
    DiscoveringPhoneIp,
    PromptingUsbDisconnect,
    ConnectingWirelessAdb,
    LaunchingScrcpy,
    Running,
    Reconnecting,
    Stopping,
    Completed,
    Failed
}
