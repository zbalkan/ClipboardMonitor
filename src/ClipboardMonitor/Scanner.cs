using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClipboardMonitor.AMSI;
using ClipboardMonitor.Helpers;
using ClipboardMonitor.PAN;

namespace ClipboardMonitor
{
    internal sealed class Scanner : IDisposable
    {
        private readonly AmsiContext _amsiContext;
        private readonly AmsiSession _amsiSession;
        private bool _disposed;

        public Scanner()
        {
            _amsiContext = AmsiContext.Create("ClipboardMonitor");
            _amsiSession = _amsiContext.CreateSession();
        }

        public Alert? Scan(string content)
        {
            var processSummary = ProcessHelper.GetClipboardOwnerProcess();
            if (processSummary == null)
            {
                return null;
            }

            if (IsCopiedFromBrowser())
            {
                PasteGuard.NotifySuspiciousClipboard(processSummary, content);
                if (IsSuspicious(content))
                {
                    return CreateAlert("Suspicious content detected in clipboard.", processSummary, content, "Suspicious data", clearClipboard: false);
                }
            }

            if (_amsiSession.IsMalware(content, "Clipboard"))
            {
                return CreateAlert(
                    "AMSI detected malicious content in clipboard. Clipboard is cleared and overwritten.",
                    processSummary,
                    content,
                    "Suspicious content",
                    clearClipboard: true);
            }

            if (PANHelper.TryParse(content, out var searchResult))
            {
                return CreatePanAlert(processSummary, searchResult);
            }

            return null;
        }

        private static Alert CreateAlert(string title, ProcessSummary processSummary, string payload, string contentLabel, bool clearClipboard)
        {
            var incidents = BuildProcessContext(processSummary)
                .Append(contentLabel).Append(": ").AppendLine(payload)
                .AppendLine("----------")
                .AppendLine();

            return new Alert
            {
                Title = title,
                Detail = incidents.ToString(),
                Payload = payload,
                ClearClipboard = clearClipboard
            };
        }

        private static Alert CreatePanAlert(ProcessSummary processSummary, IReadOnlyList<SuspectedPANData> searchResult)
        {
            var incidents = new StringBuilder(500);

            foreach (var suspectedPan in searchResult)
            {
                incidents
                    .Append(BuildProcessContext(processSummary))
                    .Append("Suspicious PAN data: ").AppendLine(suspectedPan.MaskedPAN)
                    .Append("Probable payment brand: ").AppendLine(suspectedPan.PaymentBrand)
                    .AppendLine("----------")
                    .AppendLine();
            }

            return new Alert
            {
                Title = "Suspicious PAN data detected in clipboard. Clipboard is cleared and overwritten.",
                Detail = incidents.ToString(),
                Payload = string.Join(", ", searchResult.Select(sr => sr.ToString())),
                ClearClipboard = true
            };
        }

        private static StringBuilder BuildProcessContext(ProcessSummary processSummary)
        {
            if (processSummary == default)
            {
                return new StringBuilder(200).AppendLine("Failed to get executable information");
            }

            return new StringBuilder(300)
                .Append("Source application window: ").AppendLine(processSummary.WindowTitle)
                .Append("Source process name: ").AppendLine(processSummary.ProcessName)
                .Append("Source executable path: ").AppendLine(processSummary.ExecutablePath);
        }

        private static bool IsCopiedFromBrowser() =>
            ClipboardHelper.HasDataFormat("Chromium internal source URL") ||
            ClipboardHelper.HasDataFormat("text/x-moz-url-priv") ||
            ClipboardHelper.HasDataFormat("msSourceUrl");

        private static bool IsSuspicious(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return false;
            }

            var normalised = content.Normalize(NormalizationForm.FormKD).ToLowerInvariant();
            return SuspiciousContent.HasSuspiciousText(normalised);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _amsiSession.Dispose();
            _amsiContext.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
