using WirelessScrcpy.Core.Common;
using WirelessScrcpy.Core.Workflow;

namespace WirelessScrcpy.App.UI;

public sealed class StatusForm : Form
{
    private readonly StatusViewModel _viewModel;
    private readonly TableLayoutPanel _root = new();
    private readonly Label _stateValueLabel = CreateValueLabel();
    private readonly Label _detailValueLabel = CreateValueLabel();
    private readonly Label _deviceValueLabel = CreateValueLabel();
    private readonly Label _ipAddressValueLabel = CreateValueLabel();
    private readonly Label _lastUpdatedLabel = new() { AutoSize = true, ForeColor = SystemColors.GrayText, Dock = DockStyle.Fill };
    private readonly Button _connectButton = new() { Text = "Connect", AutoSize = true, MinimumSize = new Size(104, 36) };
    private readonly Button _disconnectButton = new() { Text = "Disconnect", AutoSize = true, MinimumSize = new Size(104, 36), Enabled = false };
    private readonly Button _exitButton = new() { Text = "Exit", AutoSize = true, MinimumSize = new Size(104, 36) };

    public StatusForm(StatusViewModel viewModel)
    {
        _viewModel = viewModel;
        ConfigureWindow();
        BuildLayout();
        Render();
    }

    public event EventHandler? ConnectRequested;
    public event EventHandler? DisconnectRequested;
    public event EventHandler? ExitRequested;

    public void ApplySnapshot(WorkflowSnapshot snapshot)
    {
        if (InvokeRequired)
        {
            BeginInvoke((Action)(() => ApplySnapshot(snapshot)));
            return;
        }

        _viewModel.Apply(snapshot);
        Render();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (WindowState == FormWindowState.Minimized)
        {
            Hide();
        }
    }

    private void ConfigureWindow()
    {
        Text = "Wireless Scrcpy";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(620, 360);
        Size = new Size(680, 400);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        ShowInTaskbar = true;
    }

    private void BuildLayout()
    {
        _root.Dock = DockStyle.Fill;
        _root.Padding = new Padding(24);
        _root.ColumnCount = 2;
        _root.RowCount = 7;
        _root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        _root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var titleLabel = new Label
        {
            Text = "Wireless Scrcpy",
            Font = new Font(Font, FontStyle.Bold),
            AutoSize = true,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 18)
        };
        _root.Controls.Add(titleLabel, 0, 0);
        _root.SetColumnSpan(titleLabel, 2);

        AddRow(1, "Status", _stateValueLabel);
        AddRow(2, "Device", _deviceValueLabel);
        AddRow(3, "IP address", _ipAddressValueLabel);
        AddRow(4, "Details", _detailValueLabel);

        _root.Controls.Add(_lastUpdatedLabel, 0, 5);
        _root.SetColumnSpan(_lastUpdatedLabel, 2);

        var buttons = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Fill,
            AutoSize = true,
            Margin = new Padding(0, 18, 0, 0)
        };
        buttons.Controls.Add(_exitButton);
        buttons.Controls.Add(_disconnectButton);
        buttons.Controls.Add(_connectButton);
        _root.Controls.Add(buttons, 0, 6);
        _root.SetColumnSpan(buttons, 2);

        _connectButton.Click += (_, _) => ConnectRequested?.Invoke(this, EventArgs.Empty);
        _disconnectButton.Click += (_, _) => DisconnectRequested?.Invoke(this, EventArgs.Empty);
        _exitButton.Click += (_, _) => ExitRequested?.Invoke(this, EventArgs.Empty);

        Controls.Add(_root);
    }

    private void AddRow(int row, string labelText, Control valueControl)
    {
        var label = new Label
        {
            Text = labelText,
            AutoSize = true,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.TopLeft,
            ForeColor = SystemColors.GrayText,
            Margin = new Padding(0, 0, 16, 14)
        };
        valueControl.Margin = new Padding(0, 0, 0, 14);
        _root.Controls.Add(label, 0, row);
        _root.Controls.Add(valueControl, 1, row);
    }

    private void Render()
    {
        _stateValueLabel.Text = _viewModel.StateText;
        _stateValueLabel.ForeColor = _viewModel.Severity switch
        {
            Severity.Error => Color.Firebrick,
            Severity.Warning => Color.DarkGoldenrod,
            _ => SystemColors.ControlText
        };
        _detailValueLabel.Text = _viewModel.DetailText;
        _deviceValueLabel.Text = _viewModel.DeviceText;
        _ipAddressValueLabel.Text = _viewModel.IpAddressText;
        _lastUpdatedLabel.Text = _viewModel.LastUpdatedText;
        _connectButton.Enabled = _viewModel.CanConnect;
        _disconnectButton.Enabled = _viewModel.CanDisconnect;
    }

    private static Label CreateValueLabel() => new()
    {
        AutoSize = true,
        Dock = DockStyle.Fill,
        MaximumSize = new Size(430, 0)
    };
}
