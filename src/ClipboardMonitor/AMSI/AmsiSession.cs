using System;
using System.ComponentModel;

namespace ClipboardMonitor.AMSI
{
    public sealed class AmsiSession : IDisposable
    {
        private readonly AmsiContextSafeHandle _context;
        private readonly AmsiSessionSafeHandle _session;

        internal AmsiSession(AmsiContextSafeHandle context, AmsiSessionSafeHandle session)
        {
            _context = context;
            _session = session;
        }

        public bool IsMalware(string payload, string contentName)
        {
            var returnValue = NativeMethods.AmsiScanString(_context, payload, contentName, _session, out var result);
            if (returnValue != 0)
            {
                throw new Win32Exception(returnValue);
            }

            return AmsiResultIsMalware(result);
        }

        private static bool AmsiResultIsMalware(AmsiResult result) => result >= AmsiResult.AMSI_RESULT_DETECTED;

        public void Dispose()
        {
            _session.Dispose();
        }
    }
}