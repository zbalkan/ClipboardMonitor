using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClipboardMonitor.PAN;

namespace ClipboardMonitor
{
    internal static class PasteGuard
    {
        private const int LAST_N_SECONDS = 30;
        private static readonly NativeMethods.WinEventDelegate _winEventProc = WinEventCallback;
        private static readonly NativeMethods.LowLevelKeyboardProc _keyboardProc = KeyboardHookCallback;
        private static readonly TimeSpan Window = TimeSpan.FromSeconds(LAST_N_SECONDS);
        private static readonly TimeSpan AlertCooldown = TimeSpan.FromSeconds(5);

        private static IntPtr _lastRunDialog = IntPtr.Zero;
        private static ProcessSummary? _processSummary;
        private static string _riskContent = string.Empty;
        private static long _riskUtcTicks;
        private static long _lastWinXUtcTicks;
        private static long _lastAlertUtcTicks;
        private static readonly ConcurrentQueue<(ProcessSummary processSummary, string content)> _alertQueue = new ConcurrentQueue<(ProcessSummary processSummary, string content)>();
        private static readonly SemaphoreSlim _alertSignal = new SemaphoreSlim(0);
        private static CancellationTokenSource? _alertCts;
        private static Task? _alertWorker;
        private static IntPtr _winEventHookForeground = IntPtr.Zero;
        private static IntPtr _keyboardHook = IntPtr.Zero;

        private static bool _initialized;

        public static void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            if (_winEventHookForeground == IntPtr.Zero)
            {
                _winEventHookForeground = NativeMethods.SetWinEventHook(
                    NativeMethods.EVENT_SYSTEM_FOREGROUND,
                    NativeMethods.EVENT_SYSTEM_FOREGROUND,
                    IntPtr.Zero,
                    _winEventProc,
                    0,
                    0,
                    NativeMethods.WINEVENT_OUTOFCONTEXT);
            }

            if (_keyboardHook == IntPtr.Zero)
            {
                _keyboardHook = NativeMethods.SetWindowsHookEx(NativeMethods.WH_KEYBOARD_LL, _keyboardProc, IntPtr.Zero, 0);
            }

            _alertCts = new CancellationTokenSource();
            _alertWorker = Task.Run(() => ProcessAlertsAsync(_alertCts.Token));
            _initialized = true;
        }

        public static void Dispose()
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

            _alertCts?.Cancel();
            _alertSignal.Release();

            try
            {
                _alertWorker?.Wait(2000);
            }
            catch (AggregateException)
            {
                // expected during shutdown
            }

            _alertWorker = null;
            _alertCts?.Dispose();
            _alertCts = null;

            while (_alertQueue.TryDequeue(out _)) { }
            _initialized = false;
        }

        public static void NotifySuspiciousClipboard(ProcessSummary processSummary, string content)
        {
            var candidate = content.Length > 200 ? content.Substring(0, 200) : content;
            try
            {
                candidate = PANHelper.Mask(candidate);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError($"PAN masking failed while preparing paste-guard correlation content.\n{ex}", 31);
            }

            Volatile.Write(ref _riskUtcTicks, DateTime.UtcNow.Ticks);
            Volatile.Write(ref _processSummary, processSummary);
            Volatile.Write(ref _riskContent, candidate);
        }

        private static string GetClassName(IntPtr hWnd)
        {
            var sb = new StringBuilder(256);
            NativeMethods.GetClassName(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        private static bool IsRecentRisk() => DateTime.UtcNow.Ticks - Volatile.Read(ref _riskUtcTicks) <= Window.Ticks;

        private static bool CanDispatchAlert() =>
            DateTime.UtcNow.Ticks - Volatile.Read(ref _lastAlertUtcTicks) >= AlertCooldown.Ticks;

        private static void DispatchAlertAsync()
        {
            if (!_initialized || !CanDispatchAlert())
            {
                return;
            }

            var snapshotProcessSummary = Volatile.Read(ref _processSummary);
            var snapshotContent = Volatile.Read(ref _riskContent) ?? string.Empty;

            if (snapshotProcessSummary == default || string.IsNullOrWhiteSpace(snapshotContent))
            {
                return;
            }

            _alertQueue.Enqueue((snapshotProcessSummary, snapshotContent));
            _alertSignal.Release();
        }

        private static async Task ProcessAlertsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await _alertSignal.WaitAsync(cancellationToken).ConfigureAwait(false);

                    if (!_alertQueue.TryDequeue(out var alert))
                    {
                        continue;
                    }

                    if (!CanDispatchAlert())
                    {
                        continue;
                    }

                    ShowWarningAndAlert(alert.processSummary, alert.content);
                    Volatile.Write(ref _lastAlertUtcTicks, DateTime.UtcNow.Ticks);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Instance.LogError($"PasteGuard alert action failed.\n{ex}", 34);
                }
            }
        }

        private static void ShowWarningAndAlert(ProcessSummary processSummary, string content)
        {
            MessageBox.Show(
                $"Do not paste web content into the Run dialog unless you fully trust the source.\nCopied content:\n\n{content}",
                "Danger!",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.ServiceNotification);

            var incidents = new StringBuilder(500).AppendLine("Detected Run dialog following suspicious text copied from browser.");
            if (processSummary == default)
            {
                incidents.Append("Suspicious content: ").AppendLine(content).AppendLine("Failed to get executable information");
            }
            else
            {
                incidents
                    .Append("Source application window: ").AppendLine(processSummary.WindowTitle)
                    .Append("Source process name: ").AppendLine(processSummary.ProcessName)
                    .Append("Source executable path: ").AppendLine(processSummary.ExecutablePath)
                    .AppendLine("Suspicious content: ")
                    .AppendLine(content);
            }

            incidents.AppendLine("----------").AppendLine();

            AlertHandler.Instance.InvokeAlert(new Alert
            {
                Title = "Detected Run dialog following suspicious text copied from browser. Clipboard is cleared and overwritten.",
                Detail = incidents.ToString(),
                Payload = content,
                ClearClipboard = true
            });
        }

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
                        DispatchAlertAsync();
                    }
                }
            }

            return NativeMethods.CallNextHookEx(_keyboardHook, nCode, wParam, lParam);
        }

        private static void WinEventCallback(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (hwnd == IntPtr.Zero || idObject != 0) return;

            if (GetClassName(hwnd) == "#32770")
            {
                NativeMethods.GetWindowThreadProcessId(hwnd, out var pid);
                try
                {
                    using var proc = Process.GetProcessById((int)pid);
                    if (!proc.ProcessName.Equals("explorer", StringComparison.OrdinalIgnoreCase)) return;
                    if (hwnd == _lastRunDialog) return;
                    _lastRunDialog = hwnd;
                    if (IsRecentRisk()) DispatchAlertAsync();
                }
                catch (Exception ex)
                {
                    Logger.Instance.LogInfo($"PasteGuard window correlation skipped due to process lookup failure.\n{ex.Message}", 35);
                }
            }
        }
    }
}
