// Ignore Spelling: Pwstr

using System;
using Windows.Win32.Foundation;

namespace ClipboardMonitor
{
    internal static class NativeExtensions
    {
        public static HWND AsHwnd(this IntPtr intPtr) => new(intPtr);
    }
}
