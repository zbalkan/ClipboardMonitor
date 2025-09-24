using System;
using System.Text;
using ClipboardMonitor.PAN;

namespace ClipboardMonitor
{
    public class PasteGuardWrapper : IDisposable
    {
        public PasteGuardWrapper()
        {
            PasteGuard.Install();
            PasteGuard.RegisterAction(WarningAction);
        }

        public static void NotifyPasteGuard(ProcessSummary processSummary, string content)
        {
            var masked =
                content.Length > 200 ? MaskPayload(content.Substring(0, 200)) : MaskPayload(content);
            PasteGuard.SetSuspiciousActivityContent(processSummary, masked);
        }

        public void Dispose() => PasteGuard.Remove();

        private static string MaskPayload(string payload) => PANHelper.Mask(payload);

        private void WarningAction(ProcessSummary processSummary, string content)
        {
            var incidents = new StringBuilder(500);
            incidents.AppendLine("Detected Run dialog following suspicious text copied from browser.");
            if (processSummary == default)
            {
                incidents
                .Append("Suspicious content: ").AppendLine(content)
                .AppendLine("Failed to get executable information")
                .AppendLine("----------") // Used as delimiter
                .AppendLine();
            }
            else
            {
                incidents
                .Append("Source application window: ").AppendLine(processSummary.WindowTitle)
                .Append("Source process name: ").AppendLine(processSummary.ProcessName)
                .Append("Source executable path: ").AppendLine(processSummary.ExecutablePath)
                .AppendLine("Suspicious content: ")
                .AppendLine("----------") // Used as delimiter
                .AppendLine(content)
                .AppendLine("----------") // Used as delimiter
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