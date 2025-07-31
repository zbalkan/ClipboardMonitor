using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardMonitor.PasteGuard
{
    internal static class PasteGuard
    {
        private const int VK_LWIN = 0x5B;

        private static string CurrentRiskContent => Volatile.Read(ref _riskContent) ?? string.Empty;

        private const int LAST_N_SECONDS = 30;

        private const int VK_R = 0x52;

        private const int WH_KEYBOARD_LL = 13;

        private const int WM_KEYDOWN = 0x0100;

        private static readonly NativeMethods.LowLevelProc _proc = HookCallback;

        private static readonly TimeSpan Window = TimeSpan.FromSeconds(LAST_N_SECONDS);

        private static IntPtr _hook = IntPtr.Zero;

        private static string _riskContent = string.Empty;

        private static long _riskUtcTicks;

        private static Action<string> _registeredAction;

        public static void RegisterAction(Action<string> action)
        {
            if (_registeredAction == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            _registeredAction = action;
        }

        public static void Install()
        {
            if (_hook == IntPtr.Zero)
            {
                _hook = NativeMethods
                    .SetWindowsHookEx(
                    WH_KEYBOARD_LL,
                    _proc,
                    NativeMethods.GetModuleHandle(ProcessHelper.CaptureProcessInfo().MainModuleName),
                    0);
            }
        }

        public static void MarkRiskyBrowserCopy(string payload)
        {
            Volatile.Write(ref _riskUtcTicks, DateTime.UtcNow.Ticks);
            Volatile.Write(ref _riskContent,
                           payload.Length > 200 ? payload.Substring(0, 200) : payload); // truncate / mask here
        }

        public static void Remove()
        {
            if (_hook != IntPtr.Zero)
            {
                NativeMethods.UnhookWindowsHookEx(_hook);
                _hook = IntPtr.Zero;
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam.ToInt32() == WM_KEYDOWN)
            {
                var kb = Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);
                if (kb.vkCode == VK_R && WinHeld() // User hits Windows + R
                    && IsRecentRisk() // Within 30 secs after a browser copy-paste
                    && _registeredAction != null) // And there is an action registered.
                {
                    Task.Run(() => _registeredAction.Invoke(CurrentRiskContent));
                }
            }
            return NativeMethods.CallNextHookEx(_hook, nCode, wParam, lParam);
        }

        private static bool IsRecentRisk() => DateTime.UtcNow.Ticks - Volatile.Read(ref _riskUtcTicks) <= Window.Ticks;

        private static bool WinHeld() => (NativeMethods.GetAsyncKeyState(VK_LWIN) & 0x8000) != 0;
    }
}