using Microsoft.Win32.SafeHandles;

namespace ClipboardMonitor.AMSI
{
    internal partial class AmsiSessionSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal AmsiContextSafeHandle? Context { get; set; }

        public AmsiSessionSafeHandle()
            : base(ownsHandle: true)
        {
        }

        public override bool IsInvalid => Context == null || Context.IsInvalid || base.IsInvalid;

        protected override bool ReleaseHandle()
        {
            if (IsInvalid)
            {
                return false;
            }
            NativeMethods.AmsiCloseSession(Context!, handle);
            return true;
        }
    }
}