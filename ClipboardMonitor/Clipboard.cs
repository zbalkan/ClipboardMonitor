using System.Threading;

namespace ClipboardMonitor
{
    public static class Clipboard
    {
        public static string GetText()
        {
            var ReturnValue = string.Empty;
            var STAThread = new Thread(
                delegate () {
                    // Use a fully qualified name for Clipboard otherwise it
                    // will end up calling itself.
                    ReturnValue = System.Windows.Forms.Clipboard.GetText();
                });
            STAThread.SetApartmentState(ApartmentState.STA);
            STAThread.Start();
            STAThread.Join();

            return ReturnValue;
        }


        public static void SetText(string data)
        {
            var STAThread = new Thread(
                delegate () {
                    // Use a fully qualified name for Clipboard otherwise it
                    // will end up calling itself.
                    System.Windows.Forms.Clipboard.Clear();
                    System.Windows.Forms.Clipboard.SetText(data);
                });
            STAThread.SetApartmentState(ApartmentState.STA);
            STAThread.Start();
            STAThread.Join();
        }
    }
}
