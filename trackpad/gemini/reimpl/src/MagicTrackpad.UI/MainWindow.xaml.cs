using System.ComponentModel;
using System.Windows;
using MagicTrackpad.Core.Models;
using MagicTrackpad.Core.Interfaces;

namespace MagicTrackpad.UI
{
    public partial class MainWindow : Window
    {
        private bool _isRealExit = false;

        public MainWindow()
        {
            InitializeComponent();
            LoadSettings();
            
            // Icon
            // MyNotifyIcon.Icon = ... (Using default for now, or would need an .ico resource)
            // Hardcodet default might be transparent if no icon is set. 
            // In a real app we'd load an .ico from resources.
            
            UpdateStatus("Ready");
        }

        private void LoadSettings()
        {
            if (App.Config == null) return;
            ChkTap.IsChecked = App.Config.IsGestureEnabled(GestureType.Tap);
            ChkTwoFingerTap.IsChecked = App.Config.IsGestureEnabled(GestureType.TwoFingerTap);
            ChkScrollV.IsChecked = App.Config.IsGestureEnabled(GestureType.ScrollVertical);
            ChkPinch.IsChecked = App.Config.IsGestureEnabled(GestureType.Pinch);
        }

        private void SaveSettings()
        {
            App.Config.SetGestureEnabled(GestureType.Tap, ChkTap.IsChecked == true);
            App.Config.SetGestureEnabled(GestureType.TwoFingerTap, ChkTwoFingerTap.IsChecked == true);
            App.Config.SetGestureEnabled(GestureType.ScrollVertical, ChkScrollV.IsChecked == true);
            App.Config.SetGestureEnabled(GestureType.Pinch, ChkPinch.IsChecked == true);
            App.Config.Save();
            MessageBox.Show("Settings saved.", "MagicTrackpad", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!_isRealExit)
            {
                e.Cancel = true;
                this.Hide();
                MyNotifyIcon.ShowBalloonTip("Magic Trackpad", "Minimized to tray.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
            }
        }

        private void ShowSettings_Click(object sender, RoutedEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            _isRealExit = true;
            Application.Current.Shutdown();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Will match Window_Closing -> Hide
        }

        private void SimulateTap_Click(object sender, RoutedEventArgs e)
        {
            // Simulate a gesture detection flow
            App.ActionExecutor.Execute(new Gesture { Type = GestureType.Tap });
            UpdateStatus("Simulated Tap Executed");
        }

        private void UpdateStatus(string status)
        {
            TxtStatus.Text = status;
        }
    }
}