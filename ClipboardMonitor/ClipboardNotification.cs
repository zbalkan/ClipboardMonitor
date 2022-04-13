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
                        var processInfo = CaptureProcessInfo();

                        Clipboard.SetText(_warningText);

                        foreach (var suspectedPAN in matches)
                        {
                            // Write to event log
                            Logger.Instance.LogWarning($"Incident description: Suspected PAN data detected in clipboard. Clipboard is cleared and overwritten.\nSource application window: {processInfo.WindowTitle}\nSource executable name: {processInfo.ProcessName}\nSource executable path: {processInfo.ProcessPath}\nCaptured data: {PAN.Format(suspectedPAN, PANDisplayMode.Masked)}", 20);
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
            ToastNotificationManager.CreateToastNotifier("NETS EE - ClipboardMonitor").Show(toast);
        }

        private static ProcessInformation CaptureProcessInfo()
        {
            try
            {

                var activeWindow = NativeMethods.GetForegroundWindow();

                var length = NativeMethods.GetWindowTextLength(activeWindow);
                var title = new StringBuilder(length + 1);
                _ = NativeMethods.GetWindowText(activeWindow, title, title.Capacity);

                _ = NativeMethods.GetWindowThreadProcessId(activeWindow, out var processId);
                var process = Process.GetProcessById(processId);
                if (process != null)
                {
                    var name = process.ProcessName;
                    var path = process.MainModule != null ? process.MainModule.FileName : string.Empty;
                    var pi = new ProcessInformation
                    {
                        ProcessName = name,
                        ProcessPath = path ?? string.Empty,
                        WindowTitle = title.ToString()
                    };
                    return pi;
                }
                return default;
            }
            catch
            {
                return default;
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
