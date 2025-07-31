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
        public static Alert NoAlert = new Alert { Title = string.Empty, Detail = string.Empty, Payload = string.Empty };

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
            PasteGuard.PasteGuard.RegisterAction((riskyContent) => {
                MessageBox.Show($"Pasting web content into the Run dialog is dangerous. Use extreme caution.\n\n{riskyContent}",
                                "Danger!",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning,
                                MessageBoxDefaultButton.Button1,
                                MessageBoxOptions.ServiceNotification);
            });
        }

        public Alert Scan(string content)
        {
            // The logic comes from Eric Lawrence's article: https://textslashplain.com/2024/06/04/attack-techniques-trojaned-clipboard/
            if (IsCopiedFromBrowser())
            {
                PasteGuard.PasteGuard.MarkRiskyBrowserCopy(content);
                if (IsSuspicious(content) && _amsiSession.IsMalware(content, "Clipboard"))
                {
                    return CreateAmsiAlert(content);
                }
            }

            var searchResult = PANData.Instance.Parse(content);
            if (IncludesPANData(searchResult))
            {
                return CreatePanAlert(searchResult);
            }
            return NoAlert;
        }

        private static Alert CreateAmsiAlert(string content)
        {
            var processInfo = ProcessHelper.CaptureProcessInfo();
            var incidents = new StringBuilder(500);

            if (processInfo == default)
            {
                incidents
                .Append("Source application window: ").AppendLine(processInfo.WindowTitle)
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

            var processInfo = ProcessHelper.CaptureProcessInfo();
            var incidents = new StringBuilder(500);
            if (processInfo == default)
            {
                for (var i = 0; i < searchResult.Count; i++)
                {
                    var suspectedPan = searchResult[i];
                    incidents
                        .Append("Incident number: ").AppendLine((i + 1).ToString())
                        .Append("Source application window: ").AppendLine(processInfo.WindowTitle)
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

        private static bool IncludesPANData(IReadOnlyList<SuspectedPANData> searchResult) => searchResult != null && searchResult.Count != 0;

        private static bool IsCopiedFromBrowser()
        {
            var fromChromium = Clipboard.HasDataFormat("Chromium internal source URL");
            var fromFirefox = Clipboard.HasDataFormat("text/x-moz-url-priv");
            var fromIE = Clipboard.HasDataFormat("msSourceUrl");

            return fromChromium || fromFirefox || fromIE;
        }

        private static bool IsSuspicious(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return false;
            }

            // Fast, allocation-free search with early exit
            foreach (var t in SuspiciousText)
            {
                if (content.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0)
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