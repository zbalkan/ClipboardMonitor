using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ClipboardMonitor.AMSI;
using ClipboardMonitor.PAN;

namespace ClipboardMonitor
{
    internal class Scanner : IDisposable
    {
        private static readonly string[] SuspiciousText =
                {
                "pwsh",
                "powershell",
                "mshta",
                "cmd",
                "msiexec"
            };

        private readonly AmsiContext _amsiContext;
        private readonly AmsiSession _amsiSession;
        private bool disposedValue;

        public Scanner()
        {
            _amsiContext = AmsiContext.Create("ClipboardMonitor");
            _amsiSession = _amsiContext.CreateSession();
            void warningAction(string riskyContent)
            {
                MessageBox.Show($"Pasting web content into the Run dialog is dangerous. Use extreme caution.\n\n{riskyContent}",
                                "Danger!",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning,
                                MessageBoxDefaultButton.Button1,
                                MessageBoxOptions.ServiceNotification);
            }
            PasteGuard.PasteGuard.RegisterAction(warningAction);
        }

        public Alert Scan(string content)
        {
            // The logic comes from Eric Lawrence's article: https://textslashplain.com/2024/06/04/attack-techniques-trojaned-clipboard/
            if (IsCopiedFromBrowser())
            {
                PasteGuard.PasteGuard.MarkRiskyBrowserCopy(content);
                if (IsSuspicious(content) || _amsiSession.IsMalware(content, "Clipboard"))
                {
                    return CreateAmsiAlert(content);
                }
            }

            var searchResult = PANHelper.Parse(content);
            return searchResult == null || searchResult.Count == 0 ? default : CreatePanAlert(searchResult);
        }

        private static Alert CreateAmsiAlert(string content)
        {
            var processInfo = ProcessHelper.GetClipboardOwnerProcess();
            var incidents = new StringBuilder(500);

            if (processInfo == default)
            {
                incidents
                .Append("Suspected data: ").AppendLine(content)
                .AppendLine("Failed to get executable information")
                .AppendLine("----------") // Used as delimiter
                .AppendLine();
            }
            else
            {
                incidents = new StringBuilder(500);
                incidents
                .Append("Source application window: ").AppendLine(processInfo.WindowTitle)
                .Append("Source process name: ").AppendLine(processInfo.ProcessName)
                .Append("Source executable path: ").AppendLine(processInfo.ExecutablePath)
                .Append("Suspected data: ").AppendLine(content)
                .AppendLine("----------") // Used as delimiter
                .AppendLine();
            }
            return new Alert
            {
                Title = "AMSI detected malicious content in clipboard. Clipboard is cleared and overwritten.",
                Detail = incidents.ToString(),
                Payload = content
            };
        }

        private static Alert CreatePanAlert(IReadOnlyList<SuspectedPANData> searchResult)
        {

            var processInfo = ProcessHelper.GetClipboardOwnerProcess();
            var incidents = new StringBuilder(500);
            if (processInfo == default)
            {
                for (var i = 0; i < searchResult.Count; i++)
                {
                    var suspectedPan = searchResult[i];
                    incidents
                        .Append("Incident number: ").AppendLine((i + 1).ToString())
                        .Append("Suspected PAN data: ").AppendLine(suspectedPan.MaskedPAN)
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
                    var suspectedPan = searchResult[i];
                    incidents
                        .Append("Source application window: ").AppendLine(processInfo.WindowTitle)
                        .Append("Source process name: ").AppendLine(processInfo.ProcessName)
                        .Append("Source executable path: ").AppendLine(processInfo.ExecutablePath)
                        .Append("Suspected PAN data: ").AppendLine(suspectedPan.MaskedPAN)
                        .Append("Probable payment brand: ").AppendLine(suspectedPan.PaymentBrand)
                        .AppendLine("----------") // Used as delimiter
                        .AppendLine();
                }
            }
            return new Alert
            {
                Title = "Suspected PAN data detected in clipboard. Clipboard is cleared and overwritten.",
                Detail = incidents.ToString(),
                Payload = string.Join(", ", searchResult.Select(sr => sr.ToString()).ToArray())
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
                    .ToUpperInvariant();

            foreach (var token in SuspiciousText)
            {
                if (normalised.IndexOf(token, StringComparison.Ordinal) >= 0)
                {
                    return true;
                }
            }
            return false;
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