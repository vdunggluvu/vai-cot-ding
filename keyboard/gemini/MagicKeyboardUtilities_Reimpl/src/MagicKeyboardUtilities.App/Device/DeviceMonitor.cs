using System;
using System.Runtime.InteropServices;
using MagicKeyboardUtilities.App.Config;

namespace MagicKeyboardUtilities.App.Device
{
    public class DeviceMonitor
    {
        private const int DBT_DEVICEARRIVAL = 0x8000;
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
        private const int DBT_DEVTYP_DEVICEINTERFACE = 0x00000005;
        private const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;

        private static readonly Guid GUID_DEVCLASS_KEYBOARD = new Guid("4D36E96B-E325-11CE-BFC1-08002BE10318"); // Generic Keyboard Class
        // Alternatively use HID Class: 4D1E55B2-F16F-11CF-88CB-001111000030

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr NotificationFilter, int Flags);

        [DllImport("user32.dll")]
        private static extern bool UnregisterDeviceNotification(IntPtr Handle);

        [StructLayout(LayoutKind.Sequential)]
        private struct DEV_BROADCAST_DEVICEINTERFACE
        {
            public int dbcc_size;
            public int dbcc_devicetype;
            public int dbcc_reserved;
            public Guid dbcc_classguid;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
            public string dbcc_name;
        }
        
        // Simplified struct header to read type first
        [StructLayout(LayoutKind.Sequential)]
        private struct DEV_BROADCAST_HDR
        {
            public int dbch_size;
            public int dbch_devicetype;
            public int dbch_reserved;
        }

        private IntPtr _hwnd;
        private IntPtr _hDevNotify;
        private AppConfig _config;

        public DeviceMonitor(IntPtr hwnd, AppConfig config)
        {
            _hwnd = hwnd;
            _config = config;
        }

        public void StartMonitoring()
        {
            var dbi = new DEV_BROADCAST_DEVICEINTERFACE
            {
                dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE,
                dbcc_reserved = 0,
                dbcc_classguid = GUID_DEVCLASS_KEYBOARD,
                dbcc_name = ""
            };
            dbi.dbcc_size = Marshal.SizeOf(dbi);
            
            IntPtr buffer = Marshal.AllocHGlobal(dbi.dbcc_size);
            Marshal.StructureToPtr(dbi, buffer, true);

            _hDevNotify = RegisterDeviceNotification(_hwnd, buffer, DEVICE_NOTIFY_WINDOW_HANDLE);
            
            Marshal.FreeHGlobal(buffer);
        }

        public void StopMonitoring()
        {
            if (_hDevNotify != IntPtr.Zero)
            {
                UnregisterDeviceNotification(_hDevNotify);
                _hDevNotify = IntPtr.Zero;
            }
        }

        public void ProcessMessage(int eventType, IntPtr data)
        {
            if (data == IntPtr.Zero) return;

            if (eventType == DBT_DEVICEARRIVAL || eventType == DBT_DEVICEREMOVECOMPLETE)
            {
                DEV_BROADCAST_HDR hdr = Marshal.PtrToStructure<DEV_BROADCAST_HDR>(data);
                if (hdr.dbch_devicetype == DBT_DEVTYP_DEVICEINTERFACE)
                {
                    // For robust reading of variable length string, need manual marshalling
                    // But for now, let's assume standard structure mapping works for fixed buffer
                    // Note: dbcc_name is TCHAR array usually 1 char, followed by variable length.
                    
                    // Simple check stub
                    // Real implementation would parse VID/PID from dbcc_name
                    
                    Console.WriteLine($"Device change detected: Event {eventType:X}");
                }
            }
        }
    }
}
