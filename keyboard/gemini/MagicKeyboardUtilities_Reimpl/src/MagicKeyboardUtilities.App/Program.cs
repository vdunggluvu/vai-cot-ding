using System;
using System.Threading;
using System.Windows.Forms;

namespace MagicKeyboardUtilities.App
{
    internal static class Program
    {
        private const string MutexName = "MagicKeyboardUtilities.Reimpl";

        [STAThread]
        static void Main()
        {
            // 4.1 STARTUP FLOW: Single instance check
            using var mutex = new Mutex(true, MutexName, out bool createdNew);
            if (!createdNew)
            {
                // Already running
                return;
            }

            ApplicationConfiguration.Initialize();

            // Init logging (simple console/debug for now, later file)
            // In a real app we would build IHost, but here we do manual composition for simplicity/control
            
            try 
            {
                var appHost = new AppHost();
                appHost.Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fatal Error: {ex.Message}", "MagicKeyboardUtilities Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
