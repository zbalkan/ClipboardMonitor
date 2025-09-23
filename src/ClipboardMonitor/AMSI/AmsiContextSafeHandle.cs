using Microsoft.Win32.SafeHandles;

namespace ClipboardMonitor.AMSI
{
    internal class AmsiContextSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public AmsiContextSafeHandle()
            : base(ownsHandle: true)
        {
        }

        protected override bool ReleaseHandle()
        {
            NativeMethods.AmsiUninitialize(handle);
            return true;
        }
    }
}