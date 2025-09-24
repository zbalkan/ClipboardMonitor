using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Windows.UI.Notifications;

namespace ClipboardMonitor
{
    public sealed partial class ClipboardNotification
    {
        private sealed class NotificationHandlerForm : Form
        {
            private readonly Scanner _scanner;
            private static string _appId;
            public NotificationHandlerForm(string appId)
            {
                _appId = appId;
                _scanner = new Scanner();


                //Turn the child window into a message-only window (refer to Microsoft docs)
                NativeMethods.SetParent(Handle, HWND_MESSAGE);

                //Place window in the system-maintained clipboard format listener list
                NativeMethods.AddClipboardFormatListener(Handle);
            }

            public static void SendNotification(string message)
            {
                var xml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText01);
                var textNodes = xml.GetElementsByTagName("text");
                textNodes[0].InnerText = message;
                var iconNodes = xml.GetElementsByTagName("image");
                var iconPath = DarkModeHelper.IsDarkModeEnabled() ? Path.GetFullPath("Assets/icon-inverted.png") : Path.GetFullPath("Assets/icon.png");
                iconNodes[0].Attributes.GetNamedItem("src").NodeValue = iconPath;
                var toast = new ToastNotification(xml);
                ToastNotificationManager.CreateToastNotifier(_appId).Show(toast);
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
                    var content = ClipboardHelper.GetText();

                    // The clipboard content can be something else than plain text, e.g. images, binary files, Office shapes and diagrams, etc.
                    if (string.IsNullOrEmpty(content) || AlertHandler.Instance.SubstituteText.Equals(content))
                    {
                        return;
                    }

                    var alert = _scanner.Scan(content);
                    AlertHandler.Instance.InvokeAlert(alert);
                    SendNotification(alert.Title + "\n\nThe incident is logged.");
                }
                //Called for any unhandled messages
                base.WndProc(ref m);
            }

            #region Dispose
            protected override void Dispose(bool disposing)
            {
                NativeMethods.RemoveClipboardFormatListener(Handle);
                _scanner.Dispose();
                base.Dispose(disposing);
            }

            #endregion Dispose
        }
    }
}
