using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClipboardMonitor.PAN;

namespace ClipboardMonitor
{
    public sealed class PasteGuardWrapper : IDisposable
    {
        private const int QueueCapacity = 16;

        private readonly ConcurrentQueue<(ProcessSummary processSummary, string content)> _warningQueue = new();
        private readonly SemaphoreSlim _queueSignal = new(0);
        private readonly CancellationTokenSource _warningCts = new();
        private readonly Task _warningWorker;
        private int _queueCount;
        private bool _disposed;

        public PasteGuardWrapper()
        {
            PasteGuard.Install();
            PasteGuard.RegisterAction(EnqueueWarning);
            _warningWorker = Task.Run(() => ProcessWarningsAsync(_warningCts.Token));
        }

        public static void NotifyPasteGuard(ProcessSummary processSummary, string content)
        {
            var candidate = content.Length > 200 ? content.Substring(0, 200) : content;

            try
            {
                PasteGuard.SetSuspiciousActivityContent(processSummary, PANHelper.Mask(candidate));
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError($"PAN masking failed while preparing paste-guard correlation content.\n{ex}", 31);
                PasteGuard.SetSuspiciousActivityContent(processSummary, candidate);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            PasteGuard.Remove();
            _warningCts.Cancel();
            _queueSignal.Release();

            try
            {
                _warningWorker.Wait(2000);
            }
            catch (AggregateException)
            {
                // expected during shutdown
            }

            _queueSignal.Dispose();
            _warningCts.Dispose();
            _disposed = true;
        }

        private void EnqueueWarning(ProcessSummary processSummary, string content)
        {
            if (_disposed)
            {
                return;
            }

            _warningQueue.Enqueue((processSummary, content));
            var size = Interlocked.Increment(ref _queueCount);

            while (size > QueueCapacity && _warningQueue.TryDequeue(out _))
            {
                Interlocked.Decrement(ref _queueCount);
                size--;
                Logger.Instance.LogInfo("PasteGuard warning queue full; dropping oldest warning.", 38);
            }

            _queueSignal.Release();
        }

        private async Task ProcessWarningsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await _queueSignal.WaitAsync(cancellationToken).ConfigureAwait(false);

                if (_warningQueue.TryDequeue(out var warning))
                {
                    Interlocked.Decrement(ref _queueCount);
                    ProcessWarning(warning.processSummary, warning.content);
                }
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

            var incidents = new StringBuilder(500)
                .AppendLine("Detected Run dialog following suspicious text copied from browser.");

            if (processSummary == default)
            {
                incidents
                    .Append("Suspicious content: ").AppendLine(content)
                    .AppendLine("Failed to get executable information");
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
    }
}
