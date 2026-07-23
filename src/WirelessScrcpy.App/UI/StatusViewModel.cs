using WirelessScrcpy.Core.Common;
using WirelessScrcpy.Core.Workflow;

namespace WirelessScrcpy.App.UI;

public sealed class StatusViewModel
{
    public string StateText { get; private set; } = WorkflowState.Idle.ToString();
    public string DetailText { get; private set; } = "Ready to start a wireless scrcpy session.";
    public string DeviceText { get; private set; } = "No device detected yet.";
    public string IpAddressText { get; private set; } = "Not discovered yet.";
    public string LastUpdatedText { get; private set; } = "Waiting for status updates.";
    public Severity Severity { get; private set; } = Severity.Info;
    public bool CanConnect { get; private set; } = true;
    public bool CanDisconnect { get; private set; }

    public void Apply(WorkflowSnapshot snapshot)
    {
        StateText = ToFriendlyState(snapshot.State);
        DetailText = string.IsNullOrWhiteSpace(snapshot.DetailMessage) ? "No details available." : snapshot.DetailMessage;
        Severity = snapshot.Severity;
        LastUpdatedText = $"Last updated {snapshot.TimestampUtc.ToLocalTime():g}";
        CanConnect = snapshot.State is WorkflowState.Idle or WorkflowState.Completed or WorkflowState.Failed;
        CanDisconnect = !CanConnect && snapshot.State is not WorkflowState.Stopping;

        if (!string.IsNullOrWhiteSpace(snapshot.DeviceIdentity))
        {
            if (LooksLikeEndpoint(snapshot.DeviceIdentity))
            {
                IpAddressText = snapshot.DeviceIdentity;
            }
            else
            {
                DeviceText = snapshot.DeviceIdentity;
            }
        }
    }

    private static string ToFriendlyState(WorkflowState state) => state switch
    {
        WorkflowState.Idle => "Idle",
        WorkflowState.Starting => "Starting",
        WorkflowState.DetectingAdb => "Detecting ADB",
        WorkflowState.DetectingScrcpy => "Detecting scrcpy",
        WorkflowState.StartingAdbServer => "Starting ADB server",
        WorkflowState.DetectingUsbDevice => "Detecting USB device",
        WorkflowState.EnablingTcpIp => "Enabling TCP/IP",
        WorkflowState.DiscoveringPhoneIp => "Discovering phone IP",
        WorkflowState.PromptingUsbDisconnect => "Waiting for USB disconnect",
        WorkflowState.ConnectingWirelessAdb => "Connecting wireless ADB",
        WorkflowState.LaunchingScrcpy => "Launching scrcpy",
        WorkflowState.Running => "Running",
        WorkflowState.Reconnecting => "Reconnecting",
        WorkflowState.Stopping => "Stopping",
        WorkflowState.Completed => "Completed",
        WorkflowState.Failed => "Failed",
        _ => state.ToString()
    };

    private static bool LooksLikeEndpoint(string value) => value.Contains(':', StringComparison.Ordinal) || value.Count(static c => c == '.') == 3;
}
