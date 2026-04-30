using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using ClipboardMonitor.Helpers;
using Windows.UI.Notifications;

namespace ClipboardMonitor
{
    public sealed partial class ClipboardNotification
    {
        private sealed class NotificationHandlerForm : Form
        {
            private readonly Scanner _scanner;
            private static string _appId;

            private static readonly string IconPath = ExtractEmbeddedAsset("icon.png");
            private static readonly string IconInvertedPath = ExtractEmbeddedAsset("icon-inverted.png");

            public NotificationHandlerForm(string appId)
            {
                _appId = appId ?? throw new ArgumentNullException(nameof(appId));
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
                var iconPath = DarkModeHelper.IsDarkModeEnabled() ? IconInvertedPath : IconPath;
                iconNodes[0].Attributes.GetNamedItem("src").NodeValue = new Uri(iconPath).AbsoluteUri;
                var toast = new ToastNotification(xml);
                ToastNotificationManager.CreateToastNotifier(_appId).Show(toast);
            }


            private static string ExtractEmbeddedAsset(string fileName)
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = $"{assembly.GetName().Name}.Assets.{fileName}";

                using var stream = assembly.GetManifestResourceStream(resourceName)
                    ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' was not found.");

                var outputDir = Path.Combine(Path.GetTempPath(), assembly.GetName().Name ?? "ClipboardMonitor", "Assets");
                Directory.CreateDirectory(outputDir);
                var outputPath = Path.Combine(outputDir, fileName);

                using var file = File.Create(outputPath);
                stream.CopyTo(file);

                return outputPath;
            }

            protected override void WndProc(ref Message m)
            {
                try
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
                        if (alert == default)
                        {
                            return;
                        }
                        AlertHandler.Instance.InvokeAlert(alert);
                        SendNotification(alert.Title + "\n\nThe incident is logged.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.LogError($"Exception while processing clipboard update.\n{ex}", 32);
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