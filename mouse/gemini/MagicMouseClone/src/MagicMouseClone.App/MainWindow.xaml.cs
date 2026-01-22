using System;
using System.ComponentModel;
using System.Windows;
using MagicMouseClone.Core;

namespace MagicMouseClone.App
{
    public partial class MainWindow : Window
    {
        private readonly FakeDeviceProvider _deviceProvider;

        public MainWindow(FakeDeviceProvider deviceProvider)
        {
            InitializeComponent();
            _deviceProvider = deviceProvider;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Minimize to tray instead of closing
            e.Cancel = true;
            this.Hide();
            base.OnClosing(e);
        }

        private void ConnectFakeDevice_Click(object sender, RoutedEventArgs e)
        {
            _deviceProvider.SimulateConnection();
            Log("Fake Device Connected.");
        }

        public void Log(string message)
        {
            Dispatcher.Invoke(() =>
            {
                LogBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
                LogBox.ScrollToEnd();
            });
        }
    }
}