using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ClipboardMonitor
{
    //https://stackoverflow.com/questions/621577/clipboard-event-c-sharp
    //https://stackoverflow.com/questions/17762037/error-while-trying-to-copy-string-to-clipboard
    //https://gist.github.com/glombard/7986317

    internal static class NativeMethods
    {
        //Reference https://docs.microsoft.com/en-us/windows/desktop/dataxchg/wm-clipboardupdate
        public const int WM_CLIPBOARDUPDATE = 0x031D;
        //Reference https://www.pinvoke.net/default.aspx/Constants.HWND
#pragma warning disable IDE0090 // Use 'new(...)'
        public static IntPtr HWND_MESSAGE = new IntPtr(-3);
#pragma warning restore IDE0090 // Use 'new(...)'

        //Reference https://www.pinvoke.net/default.aspx/user32/AddClipboardFormatListener.html
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AddClipboardFormatListener(IntPtr hwnd);

        //Reference https://www.pinvoke.net/default.aspx/user32.setparent
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        //Reference https://www.pinvoke.net/default.aspx/user32/getwindowtext.html
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
#pragma warning disable CA1838 // Avoid 'StringBuilder' parameters for P/Invokes
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
#pragma warning restore CA1838 // Avoid 'StringBuilder' parameters for P/Invokes

        //Reference https://www.pinvoke.net/default.aspx/user32.getwindowtextlength
        [DllImport("user32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        //Reference https://www.pinvoke.net/default.aspx/user32.getforegroundwindow
        [DllImport("user32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        public static extern IntPtr GetForegroundWindow();

        //Reference http://pinvoke.net/default.aspx/user32/GetWindowThreadProcessId.html
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.SafeDirectories)]
#pragma warning disable CA1838 // Avoid 'StringBuilder' parameters for P/Invokes
        public static extern int GetModuleFileName(IntPtr hModule, StringBuilder lpFilename, int nSize);
#pragma warning restore CA1838 // Avoid 'StringBuilder' parameters for P/Invokes
    }
}
