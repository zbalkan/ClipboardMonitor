using System;

namespace System.Threading
{
    /// <summary>
    /// Lightweight lock abstraction for frameworks that do not ship with System.Threading.Lock.
    /// </summary>
    internal sealed class Lock
    {
        private readonly object _gate = new();

        public Scope EnterScope()
        {
            Monitor.Enter(_gate);
            return new Scope(_gate);
        }

        internal readonly struct Scope : IDisposable
        {
            private readonly object _gate;

            internal Scope(object gate) => _gate = gate;

            public void Dispose() => Monitor.Exit(_gate);
        }
    }
}
