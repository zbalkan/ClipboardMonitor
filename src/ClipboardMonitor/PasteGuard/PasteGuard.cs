using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ClipboardMonitor.PAN;

namespace ClipboardMonitor.PasteGuard
{
    internal static class PasteGuard
    {
        private const int VK_LWIN = 0x5B;

        private static string CurrentRiskContent => Volatile.Read(ref _riskContent) ?? string.Empty;

        private const int LAST_N_SECONDS = 30;

        private const int VK_R = 0x52;

        private const int VK_X = 0x58;

        private const int VK_I = 0x49;

        private const int WH_KEYBOARD_LL = 13;

        private const int WM_KEYDOWN = 0x0100;

        private static readonly NativeMethods.LowLevelProc _proc = HookCallback;

        private static readonly TimeSpan Window = TimeSpan.FromSeconds(LAST_N_SECONDS);

        private static IntPtr _hook = IntPtr.Zero;

        private static string _riskContent = string.Empty;

        private static long _riskUtcTicks;

        private static long _lastWinXUtcTicks;

        private static Action<string> _registeredAction;

        public static void RegisterAction(Action<string> action) => _registeredAction = action ?? throw new ArgumentNullException(nameof(action));

        public static void Install()
        {
            if (_hook == IntPtr.Zero)
            {
                var hMod = NativeMethods.GetModuleHandle(null);
                _hook = NativeMethods.SetWindowsHookEx(
                            WH_KEYBOARD_LL,
                            _proc,
                            hMod,
                            0);
                if (_hook == IntPtr.Zero)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(),
                        "Failed to install keyboard hook.");
                }
            }
        }

        public static void MarkRiskyBrowserCopy(string payload)
        {
            var safePayload = payload ?? string.Empty;
            Volatile.Write(ref _riskUtcTicks, DateTime.UtcNow.Ticks);
            if (safePayload.Length == 0)
            {
                Volatile.Write(ref _riskContent, string.Empty);
                return;
            }

            Volatile.Write(ref _riskContent,
                           safePayload.Length > 200 ? MaskPayload(safePayload.Substring(0, 200)) : MaskPayload(safePayload));
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
                var key = kb.vkCode;
                if (key == VK_X && WinHeld()) // User hits Windows + X
                {
                    Volatile.Write(ref _lastWinXUtcTicks, DateTime.UtcNow.Ticks);
                }

                var shortcutTriggered = (key == VK_R && WinHeld()) // User hits Windows + R
                                        || (key == VK_I && TryConsumeRecentWinX()); // User follows Win + X with I

                if (shortcutTriggered
                    && IsRecentRisk() // Within 30 secs after a browser copy-paste
                    && _registeredAction != null) // And there is an action registered.
                {
                    Task.Run(() => _registeredAction.Invoke(CurrentRiskContent));
                }
            }
            return NativeMethods.CallNextHookEx(_hook, nCode, wParam, lParam);
        }

        private static bool IsRecentRisk() => DateTime.UtcNow.Ticks - Volatile.Read(ref _riskUtcTicks) <= Window.Ticks;

        private static bool TryConsumeRecentWinX()
        {
            while (true)
            {
                var lastWinXUtcTicks = Volatile.Read(ref _lastWinXUtcTicks);
                if (DateTime.UtcNow.Ticks - lastWinXUtcTicks > Window.Ticks)
                {
                    return false;
                }

                if (Interlocked.CompareExchange(ref _lastWinXUtcTicks, 0, lastWinXUtcTicks) == lastWinXUtcTicks)
                {
                    return true;
                }
            }
        }

        private static bool WinHeld() => (NativeMethods.GetAsyncKeyState(VK_LWIN) & 0x8000) != 0;

        private static string MaskPayload(string payload) => PANData.Instance.Sanitize(payload);
    }
}