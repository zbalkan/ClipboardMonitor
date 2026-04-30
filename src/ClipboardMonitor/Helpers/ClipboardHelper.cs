using System;
using System.Threading;
using System.Windows.Forms;

namespace ClipboardMonitor.Helpers
{
    public static class ClipboardHelper
    {
        public static string GetText()
        {
            var returnValue = string.Empty;
            var staThread = new Thread(
                () => returnValue = Clipboard.GetText());
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

            return returnValue;
        }

        public static void SetText(string data)
        {
            const int maxRetries = 8;
            const int delayMs = 100;
            Exception? failure = null;

            var staThread = new Thread(() => {
                for (var i = 0; i < maxRetries; i++)
                {
                    try
                    {
                        Clipboard.Clear();
                        Clipboard.SetText(data);
                        return;
                    }
                    catch (System.Runtime.InteropServices.ExternalException)
                    {
                        Thread.Sleep(delayMs * (i + 1));
                    }
                    catch (Exception ex)
                    {
                        failure = ex;
                        return;
                    }
                }
                failure = new TimeoutException($"Unable to overwrite the clipboard after {maxRetries} retries.");
            });

            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

            if (failure != null)
            {
                throw failure;
            }
        }

        public static bool HasDataFormat(string format)
        {
            var returnValue = false;
            var staThread = new Thread(
                () => {
                    var data = Clipboard.GetDataObject();
                    if (data != null)
                    {
                        var dob = new DataObject(data);
                        foreach (var f in dob.GetFormats(true))
                        {
                            if (format.Equals(f, StringComparison.Ordinal))
                            {
                                returnValue = true;
                                break;
                            }
                        }
                    }
                });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

            return returnValue;
        }
    }
}
