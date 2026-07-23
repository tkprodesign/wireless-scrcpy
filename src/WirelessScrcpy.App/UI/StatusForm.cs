using WirelessScrcpy.Core.Workflow;

namespace WirelessScrcpy.App.UI;

public sealed class StatusForm : Form
{
    private readonly StatusViewModel _viewModel;
    private readonly Label _stateLabel = new() { AutoSize = true, Left = 16, Top = 16 };
    private readonly Label _detailLabel = new() { AutoSize = true, Left = 16, Top = 48, MaximumSize = new Size(420, 0) };

    public StatusForm(StatusViewModel viewModel)
    {
        _viewModel = viewModel;
        Text = "Wireless Scrcpy";
        Width = 480;
        Height = 180;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        Controls.Add(_stateLabel);
        Controls.Add(_detailLabel);
        Render();
    }

    public void ApplySnapshot(WorkflowSnapshot snapshot)
    {
        _viewModel.Apply(snapshot);
        Render();
    }

    private void Render()
    {
        _stateLabel.Text = $"State: {_viewModel.StateText}";
        _detailLabel.Text = _viewModel.DetailText;
    }
}
