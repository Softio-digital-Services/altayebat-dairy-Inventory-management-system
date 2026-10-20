using System;
using System.Drawing;
using System.IO.Ports;
using System.Windows.Forms;
using InventorySystem.Controls;
using InventorySystem.Helpers;

namespace InventorySystem.Forms
{
    public class ScaleSettingsForm : BaseModalForm
    {
        private ModernComboBox cmbPort;
        private ModernComboBox cmbBaud;
        private ModernComboBox cmbParity;
        private ModernComboBox cmbDataBits;
        private ModernComboBox cmbStopBits;
        private ModernComboBox cmbUnit;
        private ModernButton btnRefreshPorts;
        private ModernButton btnConnectToggle;
        private ModernButton btnSimulate;
        private Label lblLiveWeight;
        private Label lblStatus;
        private ModernTextBox txtSimulateWeight;
        private GroupBox gbPort;
        private GroupBox gbTest;

        public ScaleSettingsForm()
        {
            this.ClientSize = new Size(620, 680);
            this.TitleText = "TM-A17 Scale Settings & Calibration";

            InitializeUI();
            LoadSettings();

            SetFooterButtons(
                "Save Settings",
                "Close",
                btnSave_Click,
                btnCancel_Click
            );

            ScaleService.Instance.WeightReceived += Instance_WeightReceived;
            ScaleService.Instance.StatusChanged += Instance_StatusChanged;

            LocalizationManager.LanguageChanged += (s, e) => ApplyLocalization();
            ApplyLocalization();

            this.FormClosing += (s, e) =>
            {
                ScaleService.Instance.WeightReceived -= Instance_WeightReceived;
                ScaleService.Instance.StatusChanged -= Instance_StatusChanged;
            };
        }

        private void ApplyLocalization()
        {
            LocalizationManager.ApplyRTL(this);
            LocalizationManager.TranslateControl(this);
            Func<string, string> L = LocalizationManager.GetString;

            this.TitleText = L("Scale_FormTitle");
            if (gbPort != null) gbPort.Text = L("gbPort");
            if (gbTest != null) gbTest.Text = L("gbTest");
            if (PrimaryButton != null) PrimaryButton.Text = L("Scale_SaveSettings");
            if (SecondaryButton != null) SecondaryButton.Text = L("Scale_Close");
            if (btnRefreshPorts != null) btnRefreshPorts.Text = L("btnRefreshPorts");
            if (btnSimulate != null) btnSimulate.Text = L("btnSimulate");
            if (btnConnectToggle != null)
                btnConnectToggle.Text = ScaleService.Instance.IsConnected ? L("Scale_Disconnect") : L("btnConnectToggle");
            UpdateStatusLabel(ScaleService.Instance.IsConnected);

            SetFooterButtons(L("Scale_SaveSettings"), L("Scale_Close"), btnSave_Click, btnCancel_Click);
        }

        private void InitializeUI()
        {
            FlowLayoutPanel flpMain = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(25, 10, 25, 10),
                AutoScroll = true
            };

            // Settings Group Box
            gbPort = new GroupBox
            {
                Name = "gbPort",
                Text = "TM-A17 Serial COM Port Settings",
                Width = 550,
                Height = 275,
                Margin = new Padding(0, 0, 0, 15)
            };

            cmbPort = new ModernComboBox { Name = "cmbPort", LabelText = "COM Port:", Location = new Point(20, 25), Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            btnRefreshPorts = new ModernButton { Name = "btnRefreshPorts", Text = "Refresh", Location = new Point(275, 48), Width = 90, Height = 35 };
            ThemeConfig.ApplySecondaryButton(btnRefreshPorts);
            btnRefreshPorts.Click += (s, e) => PopulatePorts();

            btnConnectToggle = new ModernButton { Name = "btnConnectToggle", Text = ScaleService.Instance.IsConnected ? "Disconnect" : "Connect", Location = new Point(375, 48), Width = 145, Height = 35 };
            ThemeConfig.ApplyPrimaryButton(btnConnectToggle);
            btnConnectToggle.Click += btnConnectToggle_Click;

            cmbBaud = new ModernComboBox { Name = "cmbBaud", LabelText = "Baud Rate:", Location = new Point(20, 105), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbBaud.Items.AddRange(new object[] { 4800, 9600, 19200, 38400, 57600, 115200 });

            cmbParity = new ModernComboBox { Name = "cmbParity", LabelText = "Parity:", Location = new Point(190, 105), Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbParity.Items.AddRange(new object[] { "None", "Odd", "Even", "Mark", "Space" });

            cmbDataBits = new ModernComboBox { Name = "cmbDataBits", LabelText = "Data Bits:", Location = new Point(370, 105), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbDataBits.Items.AddRange(new object[] { 7, 8 });

            cmbStopBits = new ModernComboBox { Name = "cmbStopBits", LabelText = "Stop Bits:", Location = new Point(20, 185), Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStopBits.Items.AddRange(new object[] { "One", "Two", "OnePointFive" });

            cmbUnit = new ModernComboBox { Name = "cmbUnit", LabelText = "Default Weight Unit:", Location = new Point(280, 185), Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbUnit.Items.AddRange(new object[] { "kg", "g", "lb", "oz" });

            gbPort.Controls.AddRange(new Control[] { cmbPort, btnRefreshPorts, btnConnectToggle, cmbBaud, cmbParity, cmbDataBits, cmbStopBits, cmbUnit });
            flpMain.Controls.Add(gbPort);

            // Real-time scale readout panel
            gbTest = new GroupBox
            {
                Name = "gbTest",
                Text = "Live Scale Readout & Test Simulator",
                Width = 550,
                Height = 150,
                Margin = new Padding(0, 0, 0, 15)
            };

            lblLiveWeight = new Label
            {
                Name = "lblLiveWeight",
                Text = $"{ScaleService.Instance.LastWeight:N3} {ScaleService.Instance.LastUnit}",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Location = new Point(20, 25),
                AutoSize = true
            };

            lblStatus = new Label
            {
                Name = "lblStatus",
                ForeColor = ScaleService.Instance.IsConnected ? Color.Green : Color.Red,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(280, 34),
                AutoSize = true
            };

            txtSimulateWeight = new ModernTextBox { Name = "txtSimulateWeight", LabelText = "Test Weight:", Location = new Point(20, 75), Width = 230 };
            txtSimulateWeight.Text = "0.500";

            btnSimulate = new ModernButton { Name = "btnSimulate", Text = "Simulate Scale Reading", Location = new Point(275, 98), Width = 245, Height = 35 };
            ThemeConfig.ApplySecondaryButton(btnSimulate);
            btnSimulate.Click += (s, e) =>
            {
                if (decimal.TryParse(txtSimulateWeight.Text, out decimal w))
                {
                    string unit = cmbUnit.SelectedItem != null ? cmbUnit.SelectedItem.ToString() : "kg";
                    ScaleService.Instance.SimulateWeight(w, unit);
                }
            };

            gbTest.Controls.AddRange(new Control[] { lblLiveWeight, lblStatus, txtSimulateWeight, btnSimulate });
            flpMain.Controls.Add(gbTest);

            this.ContentPanel.Controls.Add(flpMain);
        }

        private void PopulatePorts()
        {
            cmbPort.Items.Clear();
            string[] ports = ScaleService.GetAvailablePorts();
            if (ports.Length > 0)
            {
                cmbPort.Items.AddRange(ports);
                cmbPort.SelectedIndex = 0;
            }
            else
            {
                cmbPort.Items.Add("COM1 (No ports detected)");
                cmbPort.SelectedIndex = 0;
            }
        }

        private void LoadSettings()
        {
            PopulatePorts();
            var cfg = ScaleService.Instance.Config;

            if (cmbPort.Items.Contains(cfg.PortName))
                cmbPort.SelectedItem = cfg.PortName;

            cmbBaud.SelectedItem = cfg.BaudRate;
            if (cmbBaud.SelectedIndex < 0) cmbBaud.SelectedItem = 9600;

            cmbParity.SelectedItem = cfg.Parity;
            if (cmbParity.SelectedIndex < 0) cmbParity.SelectedItem = "None";

            cmbDataBits.SelectedItem = cfg.DataBits;
            if (cmbDataBits.SelectedIndex < 0) cmbDataBits.SelectedItem = 8;

            cmbStopBits.SelectedItem = cfg.StopBits;
            if (cmbStopBits.SelectedIndex < 0) cmbStopBits.SelectedItem = "One";

            cmbUnit.SelectedItem = cfg.DefaultUnit;
            if (cmbUnit.SelectedIndex < 0) cmbUnit.SelectedItem = "kg";
        }

        private void btnConnectToggle_Click(object sender, EventArgs e)
        {
            if (ScaleService.Instance.IsConnected)
            {
                ScaleService.Instance.Disconnect();
                btnConnectToggle.Text = "Connect";
            }
            else
            {
                string port = cmbPort.SelectedItem?.ToString();
                if (port != null && port.Contains(" ")) port = port.Split(' ')[0];
                int baud = Convert.ToInt32(cmbBaud.SelectedItem ?? 9600);

                bool success = ScaleService.Instance.Connect(port, baud);
                btnConnectToggle.Text = success ? "Disconnect" : "Connect";
            }
        }

        private void Instance_WeightReceived(decimal weight, string unit, bool isStable)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => Instance_WeightReceived(weight, unit, isStable)));
                return;
            }
            lblLiveWeight.Text = $"{weight:N3} {unit} {(isStable ? "(Stable)" : "(Unstable)")}";
        }

        private void Instance_StatusChanged(bool isConnected, string statusText)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => Instance_StatusChanged(isConnected, statusText)));
                return;
            }
            UpdateStatusLabel(isConnected);
            btnConnectToggle.Text = isConnected
                ? LocalizationManager.GetString("Scale_Disconnect")
                : LocalizationManager.GetString("btnConnectToggle");
        }

        private void UpdateStatusLabel(bool isConnected)
        {
            if (lblStatus == null) return;
            string state = isConnected
                ? LocalizationManager.GetString("Scale_StateConnected", "Connected")
                : LocalizationManager.GetString("Scale_StateDisconnected", "Disconnected");
            lblStatus.Text = "● " + state;
            lblStatus.ForeColor = isConnected ? Color.Green : Color.Red;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var cfg = ScaleService.Instance.Config;
            string p = cmbPort.SelectedItem?.ToString();
            if (p != null && p.Contains(" ")) p = p.Split(' ')[0];

            cfg.PortName = p ?? "COM1";
            cfg.BaudRate = Convert.ToInt32(cmbBaud.SelectedItem ?? 9600);
            cfg.Parity = cmbParity.SelectedItem?.ToString() ?? "None";
            cfg.DataBits = Convert.ToInt32(cmbDataBits.SelectedItem ?? 8);
            cfg.StopBits = cmbStopBits.SelectedItem?.ToString() ?? "One";
            cfg.DefaultUnit = cmbUnit.SelectedItem?.ToString() ?? "kg";

            ScaleService.Instance.SaveConfig();
            MessageHelper.ShowInfo("TM-A17 Scale configuration saved successfully.");
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
