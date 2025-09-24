using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClipboardMonitor.AMSI;
using ClipboardMonitor.PAN;

namespace ClipboardMonitor
{
    internal class Scanner : IDisposable
    {
        private readonly AmsiContext _amsiContext;
        private readonly AmsiSession _amsiSession;
        private bool disposedValue;

        public Scanner()
        {
            _amsiContext = AmsiContext.Create("ClipboardMonitor");
            _amsiSession = _amsiContext.CreateSession();
            PasteGuard.PasteGuard.RegisterAction(WarningAction);
        }

        public Alert Scan(string content)
        {
            // The logic comes from Eric Lawrence's article: https://textslashplain.com/2024/06/04/attack-techniques-trojaned-clipboard/
            var processSummary = ProcessHelper.GetClipboardOwnerProcess();

            if (IsCopiedFromBrowser())
            {
                PasteGuard.PasteGuard.SetSuspiciousActivityContent(processSummary, content);
                if (IsSuspicious(content))
                {
                    return CreateSuspiciousActivityAlert(processSummary, content);
                }
                if (_amsiSession.IsMalware(content, "Clipboard"))
                {
                    return CreateMalwareAlert(processSummary, content);
                }

            }

            var searchResult = PANHelper.Parse(content);
            return searchResult == null || searchResult.Count == 0 ? default : CreatePanAlert(processSummary, searchResult);
        }

        private static Alert CreateMalwareAlert(ProcessSummary processSummary, string content)
        {
            var incidents = new StringBuilder(500);

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
                incidents = new StringBuilder(500);
                incidents
                .Append("Source application window: ").AppendLine(processSummary.WindowTitle)
                .Append("Source process name: ").AppendLine(processSummary.ProcessName)
                .Append("Source executable path: ").AppendLine(processSummary.ExecutablePath)
                .Append("Suspicious content: ").AppendLine(content)
                .AppendLine("----------") // Used as delimiter
                .AppendLine();
            }

            return new Alert
            {
                Title = "AMSI detected malicious content in clipboard. Clipboard is cleared and overwritten.",
                Detail = incidents.ToString(),
                Payload = content,
                ClearClipboard = true
            };
        }

        private static Alert CreatePanAlert(ProcessSummary processSummary, IReadOnlyList<SuspectedPANData> searchResult)
        {
            var incidents = new StringBuilder(500);
            if (processSummary == default)
            {
                for (var i = 0; i < searchResult.Count; i++)
                {
                    var suspectedPan = searchResult[i];
                    incidents
                        .Append("Incident number: ").AppendLine((i + 1).ToString())
                        .Append("Suspicious PAN data: ").AppendLine(suspectedPan.MaskedPAN)
                        .Append("Probable payment brand: ").AppendLine(suspectedPan.PaymentBrand)
                        .AppendLine("Failed to get executable information")
                        .AppendLine("----------") // Used as delimiter
                        .AppendLine();
                }
            }
            else
            {
                for (var i = 0; i < searchResult.Count; i++)
                {
                    var SuspiciousPan = searchResult[i];
                    incidents
                        .Append("Source application window: ").AppendLine(processSummary.WindowTitle)
                        .Append("Source process name: ").AppendLine(processSummary.ProcessName)
                        .Append("Source executable path: ").AppendLine(processSummary.ExecutablePath)
                        .Append("Suspicious PAN data: ").AppendLine(SuspiciousPan.MaskedPAN)
                        .Append("Probable payment brand: ").AppendLine(SuspiciousPan.PaymentBrand)
                        .AppendLine("----------") // Used as delimiter
                        .AppendLine();
                }
            }

            return new Alert
            {
                Title = "Suspicious PAN data detected in clipboard. Clipboard is cleared and overwritten.",
                Detail = incidents.ToString(),
                Payload = string.Join(", ", searchResult.Select(sr => sr.ToString()).ToArray()),
                ClearClipboard = true
            };
        }

        private static Alert CreateSuspiciousActivityAlert(ProcessSummary processSummary, string content)
        {
            var incidents = new StringBuilder(500);

            if (processSummary == default)
            {
                incidents
                .Append("Suspicious data: ").AppendLine(content)
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
                .Append("Suspicious data: ").AppendLine(content)
                .AppendLine("----------") // Used as delimiter
                .AppendLine();
            }

            return new Alert
            {
                Title = "Suspicious content detected in clipboard.",
                Detail = incidents.ToString(),
                Payload = content,
                ClearClipboard = false
            };
        }

        private static bool IsCopiedFromBrowser()
        {
            var fromChromium = ClipboardHelper.HasDataFormat("Chromium internal source URL");
            var fromFirefox = ClipboardHelper.HasDataFormat("text/x-moz-url-priv");
            var fromIE = ClipboardHelper.HasDataFormat("msSourceUrl");

            return fromChromium || fromFirefox || fromIE;
        }

        private static bool IsSuspicious(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return false;
            }

            var normalised = content
                    .Normalize(NormalizationForm.FormKD)
                    .ToLowerInvariant();

            return SuspiciousContent.HasSuspiciousText(normalised);
        }

        private static void WarningAction(ProcessSummary processSummary, string content)
        {
            Task.Run(() =>
                MessageBox.Show($"Pasting web content into the Run dialog is dangerous. Use extreme caution.\n\n{content}",
                                "Danger!",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning,
                                MessageBoxDefaultButton.Button1,
                                MessageBoxOptions.ServiceNotification)
            );

            var incidents = new StringBuilder(500);
            incidents.AppendLine("Win+R action detected after suspicious text copy from browser.");
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
                .Append("Suspicious content: ").AppendLine(content)
                .AppendLine("----------") // Used as delimiter
                .AppendLine();
            }
            Logger.Instance.LogWarning(incidents.ToString(), 21);
        }
        #region Dispose

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _amsiSession.Dispose();
                    _amsiContext.Dispose();
                }

                disposedValue = true;
            }
        }

        #endregion Dispose
    }
}