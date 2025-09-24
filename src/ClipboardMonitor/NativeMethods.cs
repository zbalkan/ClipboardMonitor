using System;
using System.Runtime.InteropServices;
using System.Text;
using ClipboardMonitor.AMSI;

namespace ClipboardMonitor
{
    internal static class NativeMethods
    {

        internal const uint EVENT_SYSTEM_FOREGROUND = 0x0003;

        internal const uint WINEVENT_OUTOFCONTEXT = 0;

        internal delegate void WinEventDelegate(
                                            IntPtr hWinEventHook,
                                            uint eventType,
                                            IntPtr hwnd,
                                            int idObject,
                                            int idChild,
                                            uint dwEventThread,
                                            uint dwmsEventTime);

        #region advapi32.dll

        [DllImport("advapi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern bool GetKernelObjectSecurity(IntPtr handle, int securityInformation, [Out] byte[] pSecurityDescriptor, uint nLength, out uint lpnLengthNeeded);

        [DllImport("advapi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern bool SetKernelObjectSecurity(IntPtr handle, int securityInformation, [In] byte[] pSecurityDescriptor);

        #endregion advapi32.dll

        #region ntdll.dll

        [DllImport("ntdll.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int NtSetInformationProcess(IntPtr hProcess, int processInformationClass, ref int processInformation, int processInformationLength);

        #endregion ntdll.dll

        #region user32.dll

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr GetClipboardOwner();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out IntPtr lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern IntPtr SetWinEventHook(
            uint eventMin,
            uint eventMax,
            IntPtr hmodWinEventProc,
            WinEventDelegate lpfnWinEventProc,
            uint idProcess,
            uint idThread,
            uint dwFlags);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern bool UnhookWinEvent(IntPtr hWinEventHook);
        #endregion user32.dll

        #region shell32.dll
        [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int SetCurrentProcessExplicitAppUserModelID(string appID);
        #endregion shell32.dll

        #region Amsi.dll
        // Based on Meziantou's samples at <see href="https://www.meziantou.net/using-windows-antimalware-scan-interface-in-dotnet.htm"/>.
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [DllImport("Amsi.dll", EntryPoint = "AmsiInitialize", CallingConvention = CallingConvention.StdCall)]
        internal static extern int AmsiInitialize([MarshalAs(UnmanagedType.LPWStr)] string appName, out AmsiContextSafeHandle amsiContext);

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [DllImport("Amsi.dll", EntryPoint = "AmsiUninitialize", CallingConvention = CallingConvention.StdCall)]
        internal static extern void AmsiUninitialize(IntPtr amsiContext);

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [DllImport("Amsi.dll", EntryPoint = "AmsiOpenSession", CallingConvention = CallingConvention.StdCall)]
        internal static extern int AmsiOpenSession(AmsiContextSafeHandle amsiContext, out AmsiSessionSafeHandle session);

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [DllImport("Amsi.dll", EntryPoint = "AmsiCloseSession", CallingConvention = CallingConvention.StdCall)]
        internal static extern void AmsiCloseSession(AmsiContextSafeHandle amsiContext, IntPtr session);

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [DllImport("Amsi.dll", EntryPoint = "AmsiScanString", CallingConvention = CallingConvention.StdCall)]
        internal static extern int AmsiScanString(AmsiContextSafeHandle amsiContext, [In, MarshalAs(UnmanagedType.LPWStr)] string payload, [In, MarshalAs(UnmanagedType.LPWStr)] string contentName, AmsiSessionSafeHandle session, out AmsiResult result);

        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [DllImport("Amsi.dll", EntryPoint = "AmsiScanBuffer", CallingConvention = CallingConvention.StdCall)]
        internal static extern int AmsiScanBuffer(AmsiContextSafeHandle amsiContext, byte[] buffer, uint length, string contentName, AmsiSessionSafeHandle session, out AmsiResult result);
        #endregion Amsi.dll
    }
}