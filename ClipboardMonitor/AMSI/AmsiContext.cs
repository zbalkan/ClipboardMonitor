using System;
using System.ComponentModel;

namespace ClipboardMonitor.AMSI
{
    public sealed class AmsiContext : IDisposable
    {
        private readonly AmsiContextSafeHandle _context;

        private AmsiContext(AmsiContextSafeHandle context)
        {
            _context = context;
        }

        public static AmsiContext Create(string applicationName)
        {
            var result = NativeMethods.AmsiInitialize(applicationName, out var context);
            if (result != 0)
                throw new Win32Exception(result);

            return new AmsiContext(context);
        }

        public AmsiSession CreateSession()
        {
            var result = NativeMethods.AmsiOpenSession(_context, out var session);
            session.Context = _context;
            if (result != 0)
                throw new Win32Exception(result);

            return new AmsiSession(_context, session);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}