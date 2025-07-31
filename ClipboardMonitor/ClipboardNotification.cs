// Ignore Spelling: CLIPBOARDUPDATE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using ClipboardMonitor.AMSI;
using ClipboardMonitor.PAN;
using Hardcodet.Wpf.TaskbarNotification;

namespace ClipboardMonitor
{
    public sealed class ClipboardNotification : IDisposable
    {
        //Reference https://docs.microsoft.com/en-us/windows/desktop/dataxchg/wm-clipboardupdate
        public const int WM_CLIPBOARDUPDATE = 0x031D;

        private static readonly IntPtr HWND_MESSAGE = new IntPtr(-3); //Reference https://www.pinvoke.net/default.aspx/Constants.HWND

        private readonly NotificationForm _notificationForm;

        private bool _disposedValue;

        public ClipboardNotification(string warningText, TaskbarIcon icon)
        {
            _notificationForm = new NotificationForm(warningText, icon);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposedValue)
            {
                return;
            }

            if (disposing)
            {
                _notificationForm.Dispose();
            }
            _disposedValue = true;
        }

        private sealed class NotificationForm : Form
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
            private readonly TaskbarIcon _notifyIcon;
            private readonly string _warningText;

            public NotificationForm(string warningText, TaskbarIcon icon)
            {
                _warningText = warningText;
                _notifyIcon = icon;
                _amsiContext = AmsiContext.Create("ClipboardMonitor");
                _amsiSession = _amsiContext.CreateSession();

                //Turn the child window into a message-only window (refer to Microsoft docs)
                NativeMethods.SetParent(Handle, HWND_MESSAGE);

                //Place window in the system-maintained clipboard format listener list
                NativeMethods.AddClipboardFormatListener(Handle);
            }

            protected override void Dispose(bool disposing)
            {
                _amsiContext.Dispose();
                _amsiSession.Dispose();
                base.Dispose(disposing);
            }

            protected override void WndProc(ref Message m)
            {
                //Listen for operating system messages
                if (m.Msg == WM_CLIPBOARDUPDATE)
                {
                    //Get the date and time for the current moment expressed as coordinated universal time (UTC).
                    var saveUtcNow = DateTime.UtcNow;
                    Debug.WriteLine("Copy event detected at {0} (UTC)!", saveUtcNow);

                    //Write to stdout clipboard contents
                    var content = Clipboard.GetText();

                    // The clipboard content can be something else than plain text, e.g. images, binary files, Office shapes and diagrams, etc.
                    if (string.IsNullOrEmpty(content))
                    {
                        return;
                    }

                    // The logic comes from Eric Lawrence's article: https://textslashplain.com/2024/06/04/attack-techniques-trojaned-clipboard/
                    if (IsCopiedFromBrowser() && IsSuspicous(content) && _amsiSession.IsMalware(content, "Clipboard"))
                    {
                        SendAmsiAlert(content);
                    }

                    // Run the PAN search
                    var searchResult = PANData.Instance.Parse(content);
                    if (IncludesPANData(searchResult))
                    {
                        SendPanAlert(searchResult);
                    }
                }
                //Called for any unhandled messages
                base.WndProc(ref m);
            }

            private static bool IncludesPANData(IReadOnlyList<SuspectedPANData> searchResult) => searchResult != null && searchResult.Count != 0;

            private static bool IsCopiedFromBrowser()
            {
                var fromChromium = Clipboard.HasDataFormat("Chromium internal source URL");
                var fromFirefox = Clipboard.HasDataFormat("text/x-moz-url-priv");
                var fromIE = Clipboard.HasDataFormat("msSourceUrl");

                return fromChromium || fromFirefox || fromIE;
            }

            private static bool IsSuspicous(string content)
            {
                if (string.IsNullOrEmpty(content)) return false;

                // Fast, allocation-free search with early exit
                foreach (var t in SuspiciousText)
                {
                    if (content.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;
                }
                return false;
            }

            private void SendAmsiAlert(string content)
            {
                var processInfo = ProcessHelper.CaptureProcessInfo();

                Clipboard.SetText(_warningText);

                var incidents = new StringBuilder(500);
                incidents.Append("Incident number: 1")
                .AppendLine("Incident description: AMSI detected malicious content in clipboard. Clipboard is cleared and overwritten.")
                .Append("Source application window: ").AppendLine(processInfo.WindowTitle)
                .Append("Source executable name: ").AppendLine(processInfo.ProcessName)
                .Append("Source executable path: ").AppendLine(processInfo.ExecutablePath)
                .Append("Suspected data: ").AppendLine(content)
                .AppendLine("----------") // Used as delimiter
                .AppendLine();

                Logger.Instance.LogWarning(incidents.ToString(), 20);

                // Display a notification
                SendToastNotification("Warning", "AMSI detected malicious content in clipboard. Clipboard is cleared and overwritten.\n\nThe incident is logged.");
            }

            private void SendPanAlert(IReadOnlyList<SuspectedPANData> searchResult)
            {
                var processInfo = ProcessHelper.CaptureProcessInfo();

                Clipboard.SetText(_warningText);

                var incidents = new StringBuilder(500);
                for (var i = 0; i < searchResult.Count; i++)
                {
                    var suspectedPan = searchResult[i];
                    incidents.Append("Incident number: ").AppendLine((i + 1).ToString())
                    .AppendLine("Incident description: Suspected PAN data detected in clipboard. Clipboard is cleared and overwritten.")
                    .Append("Source application window: ").AppendLine(processInfo.WindowTitle)
                    .Append("Source executable name: ").AppendLine(processInfo.ProcessName)
                    .Append("Source executable path: ").AppendLine(processInfo.ExecutablePath)
                    .Append("Suspected PAN data: ").AppendLine(suspectedPan.MaskedPAN)
                    .Append("Probable payment brand: ").AppendLine(suspectedPan.PaymentBrand)
                    .AppendLine("----------") // Used as delimiter
                    .AppendLine();
                }

                Logger.Instance.LogWarning(incidents.ToString(), 20);

                // Display a notification
                SendToastNotification("Warning", "Suspected PAN data detected in clipboard. Clipboard is cleared and overwritten.\n\nThe incident is logged.");
            }

            private void SendToastNotification(string title, string message) => _notifyIcon.ShowBalloonTip(title, message, BalloonIcon.Warning);
        }
    }
}