namespace WirelessScrcpy.Core.Workflow;

public sealed record WorkflowOptions(int TcpPort, TimeSpan ReconnectDelay, TimeSpan ReconnectWindow, TimeSpan AdbMonitorInterval)
{
    public static WorkflowOptions Default { get; } = new(5555, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(3));
}
