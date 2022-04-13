using System;
using System.Windows.Forms;
using System.Text;
using System.Diagnostics;
using Windows.UI.Notifications;
using System.IO;
using System.Reflection;
using System.Drawing;

namespace ClipboardMonitor
{
    public sealed class ClipboardNotification : IDisposable
    {
        private readonly NotificationForm notificationForm;
        private bool disposedValue;

        public ClipboardNotification(string warningText)
        {
            notificationForm = new NotificationForm(warningText);
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
                    var matches = PAN.ParseLine(content);
                    if (matches != null && matches.Count != 0)
                    {
                        var processInfo = ProcessHelper.CaptureProcessInfo();

                        Clipboard.SetText(_warningText);

                        foreach (var suspectedPAN in matches)
                        {
                            if (PAN.Validate(suspectedPAN, out var cardType))
                            {
                                Logger.Instance.LogWarning($"Incident description: Suspected PAN data detected in clipboard. Clipboard is cleared and overwritten.\nSource application window: {processInfo.WindowTitle}\nSource executable name: {processInfo.ProcessName}\nSource executable path: {processInfo.ExecutablePath}\nCaptured data: {PAN.Format(suspectedPAN, PANDisplayMode.Masked)}\nProbably card type: {Enum.GetName(cardType)}", 20);
                            }
                        }

                        // Display a notification
                        SendToastNotification("Warning", "Suspected PAN data detected in clipboard. Clipboard is cleared and overwritten.\n\nThe incident is logged.");
                    }
                }
                //Called for any unhandled messages
                base.WndProc(ref m);
            }
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
            if (!disposedValue)
            {
                if (disposing)
                {
                    notificationForm.Dispose();
                }
                disposedValue = true;
            }
        }
    }
}
