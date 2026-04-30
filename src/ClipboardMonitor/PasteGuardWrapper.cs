using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClipboardMonitor.PAN;

namespace ClipboardMonitor
{
    public class PasteGuardWrapper : IDisposable
    {
        private readonly BlockingCollection<(ProcessSummary processSummary, string content)> _warningQueue = new(16);
        private readonly CancellationTokenSource _warningCts = new();
        private readonly Task _warningWorker;
        private bool _disposed;

        public PasteGuardWrapper()
        {
            PasteGuard.Install();
            PasteGuard.RegisterAction(WarningAction);
            _warningWorker = Task.Factory.StartNew(ProcessWarnings, _warningCts.Token,
                TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public static void NotifyPasteGuard(ProcessSummary processSummary, string content)
        {
            var candidate = content.Length > 200 ? content.Substring(0, 200) : content;
            var masked = candidate;

            try
            {
                masked = PANHelper.Mask(candidate);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError($"PAN masking failed while preparing paste-guard correlation content.\n{ex}", 31);
            }

            PasteGuard.SetSuspiciousActivityContent(processSummary, masked);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            PasteGuard.Remove();
            _warningQueue.CompleteAdding();
            _warningCts.Cancel();
            try
            {
                _warningWorker.Wait(2000);
            }
            catch
            {
                // no-op
            }
            _warningCts.Dispose();
            _warningQueue.Dispose();
            _disposed = true;
        }

        private void WarningAction(ProcessSummary processSummary, string content)
        {
            if (_disposed || _warningQueue.IsAddingCompleted)
            {
                return;
            }

            if (!_warningQueue.TryAdd((processSummary, content)))
            {
                Logger.Instance.LogInfo("PasteGuard warning queue full; dropping oldest warning.", 38);
                _warningQueue.TryTake(out _);
                _ = _warningQueue.TryAdd((processSummary, content));
            }
        }

        private void ProcessWarnings()
        {
            try
            {
                foreach (var item in _warningQueue.GetConsumingEnumerable(_warningCts.Token))
                {
                    ProcessWarning(item.processSummary, item.content);
                }
            }
            catch (OperationCanceledException)
            {
                // expected during shutdown
            }
        }

        private static void ProcessWarning(ProcessSummary processSummary, string content)
        {
            MessageBox.Show($"Do not paste web content into the Run dialog unless you fully trust the source.\nCopied content:\n\n{content}",
                            "Danger!",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning,
                            MessageBoxDefaultButton.Button1,
                            MessageBoxOptions.ServiceNotification);

            var incidents = new StringBuilder(500);
            incidents.AppendLine("Detected Run dialog following suspicious text copied from browser.");
            if (processSummary == default)
            {
                incidents
                .Append("Suspicious content: ").AppendLine(content)
                .AppendLine("Failed to get executable information")
                .AppendLine("----------")
                .AppendLine();
            }
            else
            {
                incidents
                .Append("Source application window: ").AppendLine(processSummary.WindowTitle)
                .Append("Source process name: ").AppendLine(processSummary.ProcessName)
                .Append("Source executable path: ").AppendLine(processSummary.ExecutablePath)
                .AppendLine("Suspicious content: ")
                .AppendLine("----------")
                .AppendLine(content)
                .AppendLine("----------")
                .AppendLine();
            }
            var alert = new Alert
            {
                Title = "Detected Run dialog following suspicious text copied from browser. Clipboard is cleared and overwritten.",
                Detail = incidents.ToString(),
                Payload = content,
                ClearClipboard = true
            };
            AlertHandler.Instance.InvokeAlert(alert);
        }
    }
}
