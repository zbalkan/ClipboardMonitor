using System;
using System.Diagnostics;
using System.Windows.Forms;
using Hardcodet.Wpf.TaskbarNotification;

namespace ClipboardMonitor
{
    public sealed partial class ClipboardNotification
    {
        private sealed class NotificationHandlerForm : Form
        {
            private readonly TaskbarIcon _notifyIcon;
            private readonly string _warningText;
            private readonly Scanner _scanner;
            private bool _selfChange;
            public NotificationHandlerForm(string warningText, TaskbarIcon icon)
            {
                _warningText = warningText;
                _notifyIcon = icon;
                _scanner = new Scanner();

                //Turn the child window into a message-only window (refer to Microsoft docs)
                NativeMethods.SetParent(Handle, HWND_MESSAGE);

                //Place window in the system-maintained clipboard format listener list
                NativeMethods.AddClipboardFormatListener(Handle);
            }

            protected override void WndProc(ref Message m)
            {
                //Listen for operating system messages
                if (m.Msg == WM_CLIPBOARDUPDATE)
                {
                    if (_selfChange)
                    {
                        _selfChange = false;
                        return;
                    }

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

                    var alert = _scanner.Scan(content);

                    if (!Scanner.NoAlert.Equals(alert))
                    {
                        Clipboard.SetText(_warningText);
                        var logMessage = $"{alert.Title}\n\n{alert.Detail}";
                        Logger.Instance.LogWarning(logMessage, 20);
                        SendWarning(alert.Title);
                        _selfChange = true;
                    }
                }
                //Called for any unhandled messages
                base.WndProc(ref m);
            }

            protected override void Dispose(bool disposing)
            {
                _scanner.Dispose();
                base.Dispose(disposing);
            }

            private void SendWarning(string message) => _notifyIcon.ShowBalloonTip("Warning", message + "\n\nThe incident is logged.", BalloonIcon.Warning);
        }
    }
}