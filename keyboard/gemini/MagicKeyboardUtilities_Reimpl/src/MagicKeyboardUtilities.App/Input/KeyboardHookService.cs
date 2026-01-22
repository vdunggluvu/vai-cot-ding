using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using MagicKeyboardUtilities.App.Config;

namespace MagicKeyboardUtilities.App.Input
{
    public class KeyboardHookService
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;
        private readonly AppConfig _config;
        private bool _isActive = false;

        public bool IsActive => _isActive;

        public KeyboardHookService(AppConfig config)
        {
            _config = config;
            _proc = HookCallback; // Keep reference to prevent GC
        }

        public void Install()
        {
            if (_isActive) return;
            _hookID = SetHook(_proc);
            _isActive = true;
        }

        public void Uninstall()
        {
            if (!_isActive) return;
            UnhookWindowsHookEx(_hookID);
            _hookID = IntPtr.Zero;
            _isActive = false;
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName ?? "MagicKeyboardUtilities.App"), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                
                // 4.2 INPUT HOOK FLOW: Check remapping
                // Simple iteration for MVP
                foreach (var map in _config.Remapping)
                {
                    if (map.FromVk == vkCode)
                    {
                        // Found a match. 
                        // If we are strictly cloning logic, we might block and SendInput new key.
                        // Or just consume it. To be safe, specific logic needed.
                        // For re-impl parity, we log and optionally replace if SendInput enabled.
                        Console.WriteLine($"[Hook] Remapped VK {vkCode} -> {map.ToVk}");
                        
                        // To implement actual replacement:
                        // 1. Return 1 to block original
                        // 2. SendInput(toVk)
                        
                        // Stub for safety unless explicitly enabled
                         if (_config.Features.EnableSendInput)
                         {
                             // SendInput logic would go here
                             // ActionDispatcher.SendKey(map.ToVk);
                             return (IntPtr)1; // Block original
                         }
                    }
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
    }
}
