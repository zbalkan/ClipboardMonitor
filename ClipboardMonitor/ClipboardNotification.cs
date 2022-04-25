using ClipboardMonitor.PAN;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Windows.UI.Notifications;

namespace ClipboardMonitor
{
    public sealed class ClipboardNotification : IDisposable
    {
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
                NativeMethods.SetParent(Handle, NativeMethods.HWND_MESSAGE);
                //Place window in the system-maintained clipboard format listener list
                NativeMethods.AddClipboardFormatListener(Handle);
            }

            protected override void WndProc(ref Message m)
            {
                //Listen for operating system messages
                if (m.Msg == NativeMethods.WM_CLIPBOARDUPDATE)
                {
                    //Get the date and time for the current moment expressed as coordinated universal time (UTC).
                    var saveUtcNow = DateTime.UtcNow;
                    Debug.WriteLine("Copy event detected at {0} (UTC)!", saveUtcNow);


                    //Write to stdout clipboard contents
                    var content = Clipboard.GetText();
                    //Debug.WriteLine($"Clipboard Content: {content}");

                    // Run the PAN search
                    var searchResult = PANData.Instance.Parse(content);
                    if (IncludesPANData(searchResult))
                    {
                        var processInfo = ProcessHelper.CaptureProcessInfo();

                        Clipboard.SetText(_warningText);

                        foreach (var suspectedPan in searchResult)
                        {
                            Logger.Instance.LogWarning($"Incident description: Suspected PAN data detected in clipboard. Clipboard is cleared and overwritten.\nSource application window: {processInfo.WindowTitle}\nSource executable name: {processInfo.ProcessName}\nSource executable path: {processInfo.ExecutablePath}\nSuspected PAN data: {suspectedPan.MaskedPAN}\nProbable payment brand: {suspectedPan.PaymentBrand}", 20);
                        }

                        // Display a notification
                        SendToastNotification("Warning", "Suspected PAN data detected in clipboard. Clipboard is cleared and overwritten.\n\nThe incident is logged.");
                    }
                }
                //Called for any unhandled messages
                base.WndProc(ref m);
            }

            private static bool IncludesPANData(IReadOnlyList<SuspectedPANData> searchResult) => searchResult != null && searchResult.Count != 0;
        }

        private static void SendToastNotification(string title, string message)
        {
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText04);
            var stringElements = toastXml.GetElementsByTagName("text");
            stringElements[0].AppendChild(toastXml.CreateTextNode(title));
            stringElements[1].AppendChild(toastXml.CreateTextNode(message));
            var toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier("ClipboardMonitor").Show(toast);
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
