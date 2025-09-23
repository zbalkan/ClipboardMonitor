using System;

namespace ClipboardMonitor
{
    public sealed partial class ClipboardNotification : IDisposable
    {
        //Reference https://docs.microsoft.com/en-us/windows/desktop/dataxchg/wm-clipboardupdate
        public const int WM_CLIPBOARDUPDATE = 0x031D;

        private static readonly IntPtr HWND_MESSAGE = new IntPtr(-3); //Reference https://www.pinvoke.net/default.aspx/Constants.HWND

        private readonly NotificationHandlerForm _notificationForm;

        private bool _disposedValue;

        public ClipboardNotification(string warningText)
        {
            _notificationForm = new NotificationHandlerForm(warningText);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposedValue)
            {
                return;
            }

            if (disposing)
            {
                _notificationForm.Dispose();
            }
            _disposedValue = true;
        }
    }
}