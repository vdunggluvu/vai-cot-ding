using MagicMouseClone.Core.Services;
using MagicMouseClone.Core.Models;
using Microsoft.Extensions.Logging;

namespace MagicMouseClone.App;

/// <summary>
/// Main form with system tray integration
/// Evidence: High - Based on section 7.7 Tray Controller
/// </summary>
public partial class MainForm : Form
{
    private readonly ILogger<MainForm> _logger;
    private readonly AppHost _appHost;
    private NotifyIcon? _notifyIcon;
    private ContextMenuStrip? _contextMenu;

    public MainForm(ILogger<MainForm> logger, AppHost appHost)
    {
        _logger = logger;
        _appHost = appHost;

        InitializeComponent();
        InitializeTrayIcon();
        WireUpEvents();
    }

    private void InitializeComponent()
    {
        this.Text = "MagicMouseClone";
        this.Size = new Size(600, 400);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = true;
        this.ShowInTaskbar = false;

        // Status label
        var statusLabel = new Label
        {
            Text = "Initializing...",
            Location = new Point(20, 20),
            AutoSize = true,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold)
        };
        this.Controls.Add(statusLabel);

        // Device info label
        var deviceLabel = new Label
        {
            Text = "Device: Not connected",
            Location = new Point(20, 60),
            AutoSize = true,
            Font = new Font("Segoe UI", 10F)
        };
        this.Controls.Add(deviceLabel);

        // Gesture log
        var gestureLog = new TextBox
        {
            Location = new Point(20, 100),
            Size = new Size(540, 250),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            ReadOnly = true,
            Font = new Font("Consolas", 9F)
        };
        this.Controls.Add(gestureLog);

        // Store references
        this.Tag = new
        {
            StatusLabel = statusLabel,
            DeviceLabel = deviceLabel,
            GestureLog = gestureLog
        };
    }

    private void InitializeTrayIcon()
    {
        _contextMenu = new ContextMenuStrip();
        _contextMenu.Items.Add("Show Window", null, OnShowWindow);
        _contextMenu.Items.Add("-");
        _contextMenu.Items.Add("Device Status", null, OnShowDeviceStatus);
        _contextMenu.Items.Add("-");
        _contextMenu.Items.Add("Exit", null, OnExit);

        _notifyIcon = new NotifyIcon
        {
            Text = "MagicMouseClone",
            ContextMenuStrip = _contextMenu,
            Visible = true
        };

        // Create simple icon
        using var bitmap = new Bitmap(16, 16);
        using var g = Graphics.FromImage(bitmap);
        g.Clear(Color.Blue);
        g.FillEllipse(Brushes.White, 4, 4, 8, 8);

        _notifyIcon.Icon = Icon.FromHandle(bitmap.GetHicon());
        _notifyIcon.DoubleClick += OnShowWindow;
    }

    private void WireUpEvents()
    {
        this.Load += OnFormLoad;
        this.FormClosing += OnFormClosing;
        this.Resize += OnResize;

        _appHost.DeviceConnected += OnDeviceConnected;
        _appHost.DeviceDisconnected += OnDeviceDisconnected;
        _appHost.GestureDetected += OnGestureDetected;
    }

    private async void OnFormLoad(object? sender, EventArgs e)
    {
        _logger.LogInformation("MainForm loaded");

        // Initialize and start app host
        var initialized = await _appHost.InitializeAsync();
        if (initialized)
        {
            await _appHost.StartAsync();
            UpdateStatus("Running");
            _notifyIcon!.ShowBalloonTip(2000, "MagicMouseClone",
                "Application started. Running in system tray.", ToolTipIcon.Info);

            // Minimize to tray on startup
            this.WindowState = FormWindowState.Minimized;
            this.Hide();
        }
        else
        {
            UpdateStatus("Failed to initialize");
            MessageBox.Show("Failed to initialize application.", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            // Minimize to tray instead of closing
            e.Cancel = true;
            this.Hide();
            return;
        }

        _logger.LogInformation("Application closing");
        await _appHost.StopAsync();
        _appHost.Dispose();
        _notifyIcon?.Dispose();
    }

    private void OnResize(object? sender, EventArgs e)
    {
        if (this.WindowState == FormWindowState.Minimized)
        {
            this.Hide();
        }
    }

    private void OnShowWindow(object? sender, EventArgs e)
    {
        this.Show();
        this.WindowState = FormWindowState.Normal;
        this.BringToFront();
    }

    private void OnShowDeviceStatus(object? sender, EventArgs e)
    {
        var device = _appHost.GetCurrentDevice();
        var connected = _appHost.IsDeviceConnected();

        var message = connected && device != null
            ? $"Device: {device.DeviceName}\nState: {device.State}\nBattery: {device.BatteryLevel}%"
            : "No device connected";

        MessageBox.Show(message, "Device Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void OnExit(object? sender, EventArgs e)
    {
        _logger.LogInformation("Exit requested");
        Application.Exit();
    }

    private void OnDeviceConnected(object? sender, DeviceInfo device)
    {
        this.Invoke(() =>
        {
            UpdateDeviceInfo($"Device: {device.DeviceName} (Connected)");
            _notifyIcon?.ShowBalloonTip(2000, "Device Connected",
                $"{device.DeviceName} is now connected.", ToolTipIcon.Info);
        });
    }

    private void OnDeviceDisconnected(object? sender, EventArgs e)
    {
        this.Invoke(() =>
        {
            UpdateDeviceInfo("Device: Not connected");
            _notifyIcon?.ShowBalloonTip(2000, "Device Disconnected",
                "Device has been disconnected.", ToolTipIcon.Warning);
        });
    }

    private void OnGestureDetected(object? sender, GestureEvent gestureEvent)
    {
        this.Invoke(() =>
        {
            LogGesture($"[{gestureEvent.Timestamp:HH:mm:ss}] {gestureEvent.Type}");
        });
    }

    private void UpdateStatus(string status)
    {
        var controls = (dynamic?)this.Tag;
        if (controls?.StatusLabel is Label label)
        {
            label.Text = $"Status: {status}";
        }
    }

    private void UpdateDeviceInfo(string info)
    {
        var controls = (dynamic?)this.Tag;
        if (controls?.DeviceLabel is Label label)
        {
            label.Text = info;
        }
    }

    private void LogGesture(string message)
    {
        var controls = (dynamic?)this.Tag;
        if (controls?.GestureLog is TextBox textBox)
        {
            textBox.AppendText(message + Environment.NewLine);
        }
    }
}
