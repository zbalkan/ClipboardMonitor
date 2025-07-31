using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipboardMonitor.PasteGuard
{
    internal static class PasteGuard
    {
        private const int VK_LWIN = 0x5B;
        private const int VK_R = 0x52;
        private const int WH_KEYBOARD_LL = 13;

        private const int WM_KEYDOWN = 0x0100;
        private static readonly LowLevelProc _proc = HookCallback;
        private static readonly TimeSpan Window = TimeSpan.FromSeconds(30);
        private static IntPtr _hook = IntPtr.Zero;
        private static string _riskContent = string.Empty;

        private static DateTime _riskUtc = DateTime.MinValue;

        private delegate IntPtr LowLevelProc(int nCode, IntPtr wParam, IntPtr lParam);

        public static string CurrentRiskContent => _riskContent;

        public static DateTime LastClipboardUpdateUtc { get; set; } = DateTime.UtcNow;

        public static void Install()
        {
            if (_hook == IntPtr.Zero)
            {
                using (var cur = Process.GetCurrentProcess())
                {
                    using (var mod = cur.MainModule)
                    {
                        _hook = SetWindowsHookEx(WH_KEYBOARD_LL, _proc,
                                                 GetModuleHandle(mod.ModuleName), 0);
                    }
                }
            }
        }

        public static void MarkRiskyBrowserCopy(string payload)
        {
            _riskUtc = DateTime.UtcNow;
            _riskContent = payload;          // keep a copy for the alert
        }

        public static void Remove()
        {
            if (_hook != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hook);
                _hook = IntPtr.Zero;
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk,
            int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        // Extra P/Invoke to resolve module handle
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam.ToInt32() == WM_KEYDOWN)
            {
                var kb = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                if (kb.vkCode == VK_R && WinHeld() && IsRecentRisk())
                {
                    var delta = DateTime.UtcNow - LastClipboardUpdateUtc;
                    if (delta.TotalSeconds <= 30)
                    {
                        Task.Run(() =>
                            MessageBox.Show($"Pasting web content into the Run dialog is dangerous. Use extreme caution.\n\n{CurrentRiskContent}",
                                            "Danger!",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Warning,
                                            MessageBoxDefaultButton.Button1,
                                            MessageBoxOptions.ServiceNotification));
                    }
                }
            }
            return CallNextHookEx(_hook, nCode, wParam, lParam);
        }

        private static bool IsRecentRisk() => DateTime.UtcNow - _riskUtc <= Window;

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        private static bool WinHeld() => (GetAsyncKeyState(VK_LWIN) & 0x8000) != 0;

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;   // virtual-key code
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
    }
}