using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace ClipboardMonitor
{
    public static class ClipboardHelper
    {
        public static string GetText()
        {
            var returnValue = string.Empty;
            ExternalException clipboardException = null;
            var staThread = new Thread(
                () =>
                {
                    try
                    {
                        returnValue = Clipboard.GetText();
                    }
                    catch (ExternalException ex)
                    {
                        clipboardException = ex;
                    }
                });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

            if (clipboardException != null)
            {
                return string.Empty;
            }

            return returnValue;
        }

        public static void SetText(string data)
        {

            const int MaxRetries = 3;
            const int DelayMs = 100;
            Exception setFailure = null;

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
                setFailure = new TimeoutException("Unable to overwrite the clipboard after 3 retries (300 ms total).");
            });

            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

            if (setFailure != null)
            {
                throw setFailure;
            }
        }

        public static bool HasDataFormat(string format)
        {
            var returnValue = false;
            ExternalException clipboardException = null;
            var staThread = new Thread(
                () => {
                    try
                    {
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
                    }
                    catch (ExternalException ex)
                    {
                        clipboardException = ex;
                    }
                });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

            if (clipboardException != null)
            {
                return false;
            }

            return returnValue;
        }
    }
}
