using System;
using System.Diagnostics;

namespace ClipboardMonitor
{
    internal sealed class Logger : IDisposable
    {
        private const string SOURCE = "ClipboardMonitor";
        private const string LOG = "ClipboardMonitor";

        private readonly EventLog? _log;
        private static readonly Lazy<Logger> LazyInstance = new(() => new Logger());

        public static Logger Instance = LazyInstance.Value;
        private bool disposedValue;

        private Logger()
        {
            // Create an EventLog instance and assign its source.
            _log = new EventLog
            {
                Source = SOURCE,
                Log = LOG
            };
        }

        public void Install()
        {
            // Create the source, if it does not already exist.
            if (!Check())
            {
                //An event log source should not be created and immediately used.
                //There is a latency time to enable the source, it should be created
                //prior to executing the application that uses the source.
                //Execute this sample a second time to use the new source.
                EventLog.CreateEventSource(SOURCE, LOG);
                Debug.WriteLine("Created Event Source");
                Debug.WriteLine("Exiting, execute the application a second time to use the source.");
                // The source is created.  Exit the application to allow it to be registered.
            }
        }

        public void Uninstall()
        {
            if (Check())
            {
                Debug.WriteLine("Uninstalling event log source.", 1);

                if (_log != null)
                {
                    _log.Clear(); // Might be unnecessary
                    _log.Close();
                    _log.Dispose();
                }

                EventLog.Delete(LOG);
                Debug.WriteLine("Removed Event Source");
            }
        }

        public bool Check()
        {
#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                var result = EventLog.SourceExists(SOURCE);
                return result;
            }
            catch
            {
                return false;
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        public void LogInfo(string message, int eventId) =>
            _log.WriteEntry(message, EventLogEntryType.Information, eventId);

        public void LogWarning(string message, int eventId) =>
            _log.WriteEntry(message, EventLogEntryType.Warning, eventId);

        public void LogError(string message, int eventId) =>
            _log.WriteEntry(message, EventLogEntryType.Error, eventId);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _log?.Close();
                    _log?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
