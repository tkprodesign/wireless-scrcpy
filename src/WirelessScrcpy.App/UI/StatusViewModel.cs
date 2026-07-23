using WirelessScrcpy.Core.Workflow;

namespace WirelessScrcpy.App.UI;

public sealed class StatusViewModel
{
    public string StateText { get; private set; } = WorkflowState.Idle.ToString();
    public string DetailText { get; private set; } = "Ready.";
    public void Apply(WorkflowSnapshot snapshot)
    {
        StateText = snapshot.State.ToString();
        DetailText = snapshot.DetailMessage;
    }
}
