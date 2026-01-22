using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using MagicKeyboardUtilities.App.Core;

namespace MagicKeyboardUtilities.App.Tray;

/// <summary>
/// Tray icon controller with context menu
/// Traceability: Section 4.3 TRAY ICON FLOW, Section 8.1 Phase 5
/// Evidence: "System Tray Application" (inferred from background behavior)
/// </summary>
public class TrayIconController : IDisposable
{
    private readonly ILogger<TrayIconController> _logger;
    private readonly ActionDispatcher _actionDispatcher;
    private NotifyIcon? _notifyIcon;
    private ContextMenuStrip? _contextMenu;
    private bool _isEnabled;

    public TrayIconController(ILogger<TrayIconController> logger, ActionDispatcher actionDispatcher)
    {
        _logger = logger;
        _actionDispatcher = actionDispatcher;
        _isEnabled = false;
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled != value)
            {
                _isEnabled = value;
                UpdateMenuState();
            }
        }
    }

    /// <summary>
    /// Create and show tray icon
    /// Traceability: Section 4.3 Step 1 "Tray Icon Creation"
    /// </summary>
    public void Create()
    {
        try
        {
            // Create context menu
            _contextMenu = new ContextMenuStrip();
            _contextMenu.Items.Add("Enable", null, OnEnable);
            _contextMenu.Items.Add("Disable", null, OnDisable);
            _contextMenu.Items.Add(new ToolStripSeparator());
            _contextMenu.Items.Add("Settings...", null, OnSettings);
            _contextMenu.Items.Add(new ToolStripSeparator());
            _contextMenu.Items.Add("Exit", null, OnExit);

            // Create tray icon
            _notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application, // TODO: Use custom icon
                Text = "Magic Keyboard Utilities (Reimpl)",
                ContextMenuStrip = _contextMenu,
                Visible = true
            };

            // Handle double-click
            _notifyIcon.DoubleClick += OnTrayDoubleClick;

            _logger.LogInformation("Tray icon created successfully");
            UpdateMenuState();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create tray icon");
        }
    }

    /// <summary>
    /// Update menu items based on enabled state
    /// </summary>
    private void UpdateMenuState()
    {
        if (_contextMenu == null) return;

        try
        {
            _contextMenu.Items[0].Enabled = !_isEnabled; // Enable
            _contextMenu.Items[1].Enabled = _isEnabled;  // Disable

            if (_notifyIcon != null)
            {
                _notifyIcon.Text = _isEnabled
                    ? "Magic Keyboard Utilities (Reimpl) - Enabled"
                    : "Magic Keyboard Utilities (Reimpl) - Disabled";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating menu state");
        }
    }

    /// <summary>
    /// Handle Enable menu item
    /// Traceability: Section 4.3 Step 4 "Menu Command Execution"
    /// </summary>
    private void OnEnable(object? sender, EventArgs e)
    {
        _logger.LogInformation("Enable menu clicked");
        _actionDispatcher.Execute("Enable");
    }

    /// <summary>
    /// Handle Disable menu item
    /// </summary>
    private void OnDisable(object? sender, EventArgs e)
    {
        _logger.LogInformation("Disable menu clicked");
        _actionDispatcher.Execute("Disable");
    }

    /// <summary>
    /// Handle Settings menu item
    /// Traceability: Section 4.3 "Settings… stub"
    /// </summary>
    private void OnSettings(object? sender, EventArgs e)
    {
        _logger.LogInformation("Settings menu clicked");
        
        try
        {
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            
            if (File.Exists(configPath))
            {
                var result = MessageBox.Show(
                    $"Configuration file location:\n{configPath}\n\nOpen in default editor?",
                    "Settings",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = configPath,
                        UseShellExecute = true
                    });
                }
            }
            else
            {
                MessageBox.Show(
                    $"Configuration file not found at:\n{configPath}",
                    "Settings",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening settings");
            MessageBox.Show(
                $"Error opening settings: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Handle Exit menu item
    /// Traceability: Section 4.6 Step 1 "Exit Signal Reception"
    /// </summary>
    private void OnExit(object? sender, EventArgs e)
    {
        _logger.LogInformation("Exit menu clicked");
        _actionDispatcher.Execute("Exit");
    }

    /// <summary>
    /// Handle tray icon double-click
    /// Traceability: Section 4.3 Step 2 "Tray Message Handling"
    /// </summary>
    private void OnTrayDoubleClick(object? sender, EventArgs e)
    {
        _logger.LogInformation("Tray icon double-clicked");
        
        MessageBox.Show(
            $"Magic Keyboard Utilities (Reimplementation)\n" +
            $"Version: 3.1.5.6\n" +
            $"Status: {(_isEnabled ? "Enabled" : "Disabled")}\n\n" +
            $"Traceability: Flow Report Section 4.3",
            "About",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    /// <summary>
    /// Show notification balloon
    /// </summary>
    public void ShowNotification(string title, string message, ToolTipIcon icon = ToolTipIcon.Info)
    {
        try
        {
            _notifyIcon?.ShowBalloonTip(3000, title, message, icon);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing notification");
        }
    }

    public void Dispose()
    {
        try
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _logger.LogInformation("Tray icon disposed");
            }

            _contextMenu?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing tray icon");
        }
    }
}
