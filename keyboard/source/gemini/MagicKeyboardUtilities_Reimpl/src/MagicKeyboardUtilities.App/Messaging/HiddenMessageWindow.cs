using System;
using System.Windows.Forms;

namespace MagicKeyboardUtilities.App.Messaging
{
    public class HiddenMessageWindow : NativeWindow
    {
        private const int WM_DEVICECHANGE = 0x0219;
        private const int WM_HOTKEY = 0x0312;
        private const int CP_NOCLOSE_BUTTON = 0x200;

        private readonly AppHost _appHost;

        public HiddenMessageWindow(AppHost appHost)
        {
            _appHost = appHost;
            CreateHandle(new CreateParams
            {
                Parent = IntPtr.Zero,
                Style = 0, // No specific style
                ExStyle = 0,
                ClassStyle = CP_NOCLOSE_BUTTON,
                Caption = "MagicKeyboardUtilities_MessageWindow"
            });
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_DEVICECHANGE:
                    _appHost.HandleDeviceChange((int)m.WParam, m.LParam);
                    break;
                case WM_HOTKEY:
                    _appHost.HandleHotkey((int)m.WParam);
                    break;
            }
            base.WndProc(ref m);
        }
    }
}
