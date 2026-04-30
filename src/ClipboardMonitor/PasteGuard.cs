using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardMonitor
{
    internal static class PasteGuard
    {
        private const int LAST_N_SECONDS = 30;
        private static readonly NativeMethods.WinEventDelegate _winEventProc = WinEventCallback;
        private static readonly NativeMethods.LowLevelKeyboardProc _keyboardProc = KeyboardHookCallback;
        private static readonly TimeSpan Window = TimeSpan.FromSeconds(LAST_N_SECONDS);

        private static IntPtr _lastRunDialog = IntPtr.Zero;
        private static ProcessSummary? _processSummary;
        private static Action<ProcessSummary, string>? _registeredAction;
        private static string _riskContent = string.Empty;
        private static long _riskUtcTicks;
        private static long _lastWinXUtcTicks;
        private static IntPtr _winEventHookForeground = IntPtr.Zero;
        private static IntPtr _keyboardHook = IntPtr.Zero;
        private static ProcessSummary? BrowserProcessSummary => Volatile.Read(ref _processSummary) ?? default;
        private static string CurrentRiskContent => Volatile.Read(ref _riskContent) ?? string.Empty;

        public static void Install()
        {
            if (_winEventHookForeground == IntPtr.Zero)
            {
                _winEventHookForeground = NativeMethods.SetWinEventHook(NativeMethods.EVENT_SYSTEM_FOREGROUND, NativeMethods.EVENT_SYSTEM_FOREGROUND,
                    IntPtr.Zero, _winEventProc, 0, 0, NativeMethods.WINEVENT_OUTOFCONTEXT);
            }

            if (_keyboardHook == IntPtr.Zero)
            {
                _keyboardHook = NativeMethods.SetWindowsHookEx(NativeMethods.WH_KEYBOARD_LL, _keyboardProc, IntPtr.Zero, 0);
            }
        }

        public static void RegisterAction(Action<ProcessSummary, string> action) =>
                    _registeredAction = action ?? throw new ArgumentNullException(nameof(action));

        public static void Remove()
        {
            if (_winEventHookForeground != IntPtr.Zero)
            {
                NativeMethods.UnhookWinEvent(_winEventHookForeground);
                _winEventHookForeground = IntPtr.Zero;
            }

            if (_keyboardHook != IntPtr.Zero)
            {
                NativeMethods.UnhookWindowsHookEx(_keyboardHook);
                _keyboardHook = IntPtr.Zero;
            }
        }

        public static void SetSuspiciousActivityContent(ProcessSummary processSummary, string content)
        {
            Volatile.Write(ref _riskUtcTicks, DateTime.UtcNow.Ticks);
            Volatile.Write(ref _processSummary, processSummary);
            Volatile.Write(ref _riskContent, content);
        }

        private static string GetClassName(IntPtr hWnd)
        {
            var sb = new StringBuilder(256);
            NativeMethods.GetClassName(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        private static bool IsRecentRisk() =>
            DateTime.UtcNow.Ticks - Volatile.Read(ref _riskUtcTicks) <= Window.Ticks;

        private static bool IsWindowsKeyPressed() =>
            (NativeMethods.GetKeyState(NativeMethods.VK_LWIN) & 0x8000) != 0 ||
            (NativeMethods.GetKeyState(NativeMethods.VK_RWIN) & 0x8000) != 0;

        private static IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)NativeMethods.WM_KEYDOWN)
            {
                var vkCode = Marshal.ReadInt32(lParam);
                if (vkCode == NativeMethods.VK_X && IsWindowsKeyPressed())
                {
                    Volatile.Write(ref _lastWinXUtcTicks, DateTime.UtcNow.Ticks);
                }
                else if (vkCode == NativeMethods.VK_I)
                {
                    var ticksSinceWinX = DateTime.UtcNow.Ticks - Volatile.Read(ref _lastWinXUtcTicks);
                    if (ticksSinceWinX <= Window.Ticks && IsRecentRisk())
                    {
                        _ = Task.Run(() => {
                            if (BrowserProcessSummary != null)
                            {
                                _registeredAction?.Invoke(BrowserProcessSummary, CurrentRiskContent);
                            }
                        });
                    }
                }
            }

            return NativeMethods.CallNextHookEx(_keyboardHook, nCode, wParam, lParam);
        }

        private static void WinEventCallback(
             IntPtr hWinEventHook,
             uint eventType,
             IntPtr hwnd,
             int idObject,
             int idChild,
             uint dwEventThread,
             uint dwmsEventTime)
        {
            if (hwnd == IntPtr.Zero || idObject != 0) // OBJID_WINDOW only
            {
                return;
            }

            var cls = GetClassName(hwnd);
            if (cls == "#32770")
            {
                NativeMethods.GetWindowThreadProcessId(hwnd, out var pid);
                try
                {
                    using var proc = Process.GetProcessById((int)pid);
                    var exeName = proc.ProcessName;

                    if (exeName.Equals("explorer", StringComparison.OrdinalIgnoreCase))
                    {
                        if (hwnd == _lastRunDialog)
                        {
                            return;
                        }

                        _lastRunDialog = hwnd;

                        if (!IsRecentRisk())
                        {
                            return;
                        }

                        _ = Task.Run(() => {
                            if (BrowserProcessSummary != null)
                            {
                                _registeredAction?.Invoke(BrowserProcessSummary, CurrentRiskContent);
                            }
                        });
                    }
                }
                catch
                {
                    // process may have exited, ignore
                }
            }
        }
    }
}