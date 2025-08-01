using System;
using System.Threading;
using System.Windows.Forms;

namespace ClipboardMonitor
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

            const int MaxRetries = 3;
            const int DelayMs = 100;

            var staThread = new Thread(() =>
            {
                for (var i = 0; i < MaxRetries; i++)
                {
                    try
                    {
                        Clipboard.Clear();
                        Clipboard.SetText(data);
                        return;
                    }
                    catch (System.Runtime.InteropServices.ExternalException)
                    {
                        Thread.Sleep(DelayMs);
                    }
                }
                throw new TimeoutException("Unable to overwrite the clipboard after 3 retries (300 ms total).");
            });

            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();
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
                            if (format.Equals(f))
                            {
                                returnValue = true;
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
