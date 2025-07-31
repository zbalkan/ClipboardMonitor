using System.Threading;

namespace ClipboardMonitor
{
    public static class Clipboard
    {
        public static string GetText()
        {
            var returnValue = string.Empty;
            var staThread = new Thread(
                () => {
                    // Use a fully qualified name for Clipboard otherwise it
                    // will end up calling itself.
                    returnValue = System.Windows.Forms.Clipboard.GetText();
                });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();

            return returnValue;
        }

        public static void SetText(string data)
        {
            var staThread = new Thread(
                () => {
                    // Use a fully qualified name for Clipboard otherwise it
                    // will end up calling itself.
                    System.Windows.Forms.Clipboard.Clear();
                    System.Windows.Forms.Clipboard.SetText(data);
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
                    // Use a fully qualified name for Clipboard otherwise it
                    // will end up calling itself.
                    var data = System.Windows.Forms.Clipboard.GetDataObject();
                    if (data != null)
                    {
                        var dob = new System.Windows.Forms.DataObject(data);
                        var formats = dob.GetFormats(true);

                        foreach (var f in formats)
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
