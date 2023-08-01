using System;
using System.Runtime.InteropServices;

namespace ClipboardMonitor
{
    // These methods cannot be provided by CsWin32 properly.
    // References:
    //https://stackoverflow.com/questions/621577/clipboard-event-c-sharp
    //https://stackoverflow.com/questions/17762037/error-while-trying-to-copy-string-to-clipboard
    //https://gist.github.com/glombard/7986317

    internal static class NativeMethods
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern bool GetKernelObjectSecurity(IntPtr handle, int securityInformation, [Out] byte[] pSecurityDescriptor, uint nLength, out uint lpnLengthNeeded);

        [DllImport("advapi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern bool SetKernelObjectSecurity(IntPtr handle, int securityInformation, [In] byte[] pSecurityDescriptor);

        [DllImport("ntdll.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern int NtSetInformationProcess(IntPtr hProcess, int processInformationClass, ref int processInformation, int processInformationLength);
    }
}
