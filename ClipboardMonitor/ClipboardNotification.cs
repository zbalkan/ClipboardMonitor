// Ignore Spelling: CLIPBOARDUPDATE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Windows.UI.Notifications;
using ClipboardMonitor.PAN;
using System.Text;
using Windows.Win32;

namespace ClipboardMonitor
{
    public sealed class ClipboardNotification : IDisposable
    {
        //Reference https://docs.microsoft.com/en-us/windows/desktop/dataxchg/wm-clipboardupdate
        public const int WM_CLIPBOARDUPDATE = 0x031D;

        private static readonly IntPtr HWND_MESSAGE = new(-3); //Reference https://www.pinvoke.net/default.aspx/Constants.HWND

        private readonly NotificationForm _notificationForm;
        private bool _disposedValue;
        public ClipboardNotification(string warningText)
        {
            _notificationForm = new NotificationForm(warningText);
        }

        private sealed class NotificationForm : Form
        {
            private readonly string _warningText;

            public NotificationForm(string warningText)
            {
                _warningText = warningText;

                //Turn the child window into a message-only window (refer to Microsoft docs)
                PInvoke.SetParent(Handle.AsHwnd(), HWND_MESSAGE.AsHwnd());

                //Place window in the system-maintained clipboard format listener list
                PInvoke.AddClipboardFormatListener(Handle.AsHwnd());
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
                    if (content == null)
                    {
                        return;
                    }

                    // Run the PAN search
                    var searchResult = PANData.Instance.Parse(content);
                    if (IncludesPANData(searchResult))
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
                            .AppendLine();
                        }

                        Logger.Instance.LogWarning(incidents.ToString(), 20);

                        // Display a notification
                        SendToastNotification("Warning", "Suspected PAN data detected in clipboard. Clipboard is cleared and overwritten.\n\nThe incident is logged.");
                    }
                }
                //Called for any unhandled messages
                base.WndProc(ref m);
            }

            private static bool IncludesPANData(IReadOnlyList<SuspectedPANData> searchResult) => searchResult != null && searchResult.Count != 0;

            private static void SendToastNotification(string title, string message)
            {
                var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText04);
                var stringElements = toastXml.GetElementsByTagName("text");
                stringElements[0].AppendChild(toastXml.CreateTextNode(title));
                stringElements[1].AppendChild(toastXml.CreateTextNode(message));
                var toast = new ToastNotification(toastXml);
                ToastNotificationManager.CreateToastNotifier("ClipboardMonitor").Show(toast);
            }
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
    }
}
