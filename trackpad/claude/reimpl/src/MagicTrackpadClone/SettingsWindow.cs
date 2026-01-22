using System.Windows;
using MagicTrackpadClone.Core;

namespace MagicTrackpadClone;

public partial class SettingsWindow : Window
{
    private readonly AppConfiguration? _config;
    private readonly IConfigStore _configStore;
    private readonly ILogger _logger;

    public SettingsWindow(AppConfiguration? config, IConfigStore configStore, ILogger logger)
    {
        _config = config;
        _configStore = configStore;
        _logger = logger;
        
        InitializeComponent();
        LoadSettings();
    }

    private void InitializeComponent()
    {
        Title = "Magic Trackpad Settings";
        Width = 600;
        Height = 450;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        
        var grid = new System.Windows.Controls.Grid();
        grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = GridLength.Auto });
        
        var scrollViewer = new System.Windows.Controls.ScrollViewer
        {
            VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
            Padding = new Thickness(20)
        };
        
        var stackPanel = new System.Windows.Controls.StackPanel();
        
        // Title
        var titleBlock = new System.Windows.Controls.TextBlock
        {
            Text = "Settings",
            FontSize = 24,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 20)
        };
        stackPanel.Children.Add(titleBlock);
        
        // General Settings
        var generalBlock = new System.Windows.Controls.TextBlock
        {
            Text = "General",
            FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 10)
        };
        stackPanel.Children.Add(generalBlock);
        
        var startWithWindowsCheck = new System.Windows.Controls.CheckBox
        {
            Content = "Start with Windows",
            Margin = new Thickness(0, 0, 0, 10),
            Name = "StartWithWindowsCheckBox"
        };
        stackPanel.Children.Add(startWithWindowsCheck);
        
        var startMinimizedCheck = new System.Windows.Controls.CheckBox
        {
            Content = "Start minimized to tray",
            Margin = new Thickness(0, 0, 0, 10),
            Name = "StartMinimizedCheckBox"
        };
        stackPanel.Children.Add(startMinimizedCheck);
        
        var enableGesturesCheck = new System.Windows.Controls.CheckBox
        {
            Content = "Enable gestures",
            Margin = new Thickness(0, 0, 0, 20),
            Name = "EnableGesturesCheckBox"
        };
        stackPanel.Children.Add(enableGesturesCheck);
        
        // Device Settings
        var deviceBlock = new System.Windows.Controls.TextBlock
        {
            Text = "Device",
            FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 10)
        };
        stackPanel.Children.Add(deviceBlock);
        
        var sensitivityPanel = new System.Windows.Controls.StackPanel
        {
            Orientation = System.Windows.Controls.Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 10)
        };
        
        sensitivityPanel.Children.Add(new System.Windows.Controls.TextBlock
        {
            Text = "Sensitivity: ",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
        });
        
        var sensitivitySlider = new System.Windows.Controls.Slider
        {
            Minimum = 0.5,
            Maximum = 2.0,
            Width = 200,
            TickFrequency = 0.1,
            IsSnapToTickEnabled = true,
            Name = "SensitivitySlider"
        };
        sensitivityPanel.Children.Add(sensitivitySlider);
        
        var sensitivityLabel = new System.Windows.Controls.TextBlock
        {
            Text = "1.0",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 0),
            Name = "SensitivityLabel"
        };
        sensitivityPanel.Children.Add(sensitivityLabel);
        
        stackPanel.Children.Add(sensitivityPanel);
        
        var reverseScrollCheck = new System.Windows.Controls.CheckBox
        {
            Content = "Reverse scroll direction",
            Margin = new Thickness(0, 0, 0, 20),
            Name = "ReverseScrollCheckBox"
        };
        stackPanel.Children.Add(reverseScrollCheck);
        
        scrollViewer.Content = stackPanel;
        System.Windows.Controls.Grid.SetRow(scrollViewer, 0);
        grid.Children.Add(scrollViewer);
        
        // Buttons
        var buttonPanel = new System.Windows.Controls.StackPanel
        {
            Orientation = System.Windows.Controls.Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(20)
        };
        
        var saveButton = new System.Windows.Controls.Button
        {
            Content = "Save",
            Width = 80,
            Height = 30,
            Margin = new Thickness(0, 0, 10, 0)
        };
        saveButton.Click += SaveButton_Click;
        buttonPanel.Children.Add(saveButton);
        
        var cancelButton = new System.Windows.Controls.Button
        {
            Content = "Cancel",
            Width = 80,
            Height = 30
        };
        cancelButton.Click += (s, e) => Close();
        buttonPanel.Children.Add(cancelButton);
        
        System.Windows.Controls.Grid.SetRow(buttonPanel, 1);
        grid.Children.Add(buttonPanel);
        
        Content = grid;
        
        // Wire up slider event
        sensitivitySlider.ValueChanged += (s, e) =>
        {
            sensitivityLabel.Text = e.NewValue.ToString("F1");
        };
        
        // Store references
        RegisterName("StartWithWindowsCheckBox", startWithWindowsCheck);
        RegisterName("StartMinimizedCheckBox", startMinimizedCheck);
        RegisterName("EnableGesturesCheckBox", enableGesturesCheck);
        RegisterName("SensitivitySlider", sensitivitySlider);
        RegisterName("ReverseScrollCheckBox", reverseScrollCheck);
    }

    private void LoadSettings()
    {
        if (_config == null) return;
        
        var startWithWindows = FindName("StartWithWindowsCheckBox") as System.Windows.Controls.CheckBox;
        var startMinimized = FindName("StartMinimizedCheckBox") as System.Windows.Controls.CheckBox;
        var enableGestures = FindName("EnableGesturesCheckBox") as System.Windows.Controls.CheckBox;
        var sensitivity = FindName("SensitivitySlider") as System.Windows.Controls.Slider;
        var reverseScroll = FindName("ReverseScrollCheckBox") as System.Windows.Controls.CheckBox;
        
        if (startWithWindows != null) startWithWindows.IsChecked = _config.General.StartWithWindows;
        if (startMinimized != null) startMinimized.IsChecked = _config.General.StartMinimized;
        if (enableGestures != null) enableGestures.IsChecked = _config.General.EnableGestures;
        if (sensitivity != null) sensitivity.Value = _config.Device.Sensitivity;
        if (reverseScroll != null) reverseScroll.IsChecked = _config.Device.ReverseScrollDirection;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (_config == null) return;
        
        try
        {
            var startWithWindows = FindName("StartWithWindowsCheckBox") as System.Windows.Controls.CheckBox;
            var startMinimized = FindName("StartMinimizedCheckBox") as System.Windows.Controls.CheckBox;
            var enableGestures = FindName("EnableGesturesCheckBox") as System.Windows.Controls.CheckBox;
            var sensitivity = FindName("SensitivitySlider") as System.Windows.Controls.Slider;
            var reverseScroll = FindName("ReverseScrollCheckBox") as System.Windows.Controls.CheckBox;
            
            _config.General.StartWithWindows = startWithWindows?.IsChecked ?? true;
            _config.General.StartMinimized = startMinimized?.IsChecked ?? true;
            _config.General.EnableGestures = enableGestures?.IsChecked ?? true;
            _config.Device.Sensitivity = sensitivity?.Value ?? 1.0;
            _config.Device.ReverseScrollDirection = reverseScroll?.IsChecked ?? false;
            
            await _configStore.SaveConfigurationAsync(_config);
            
            _logger.LogInfo("Settings saved");
            MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to save settings", ex);
            MessageBox.Show($"Failed to save settings:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
