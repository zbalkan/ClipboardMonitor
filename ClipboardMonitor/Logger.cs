using System;
using System.Diagnostics;
using System.Security.Principal;

namespace ClipboardMonitor
{
    internal sealed class Logger : IDisposable
    {
        private const string Source = "ClipboardMonitor";
        private const string Log = "ClipboardMonitor";

        private readonly string _username;
        private readonly EventLog? _log;
        private static readonly Lazy<Logger> LazyInstance = new(() => new Logger());

        public static Logger Instance = LazyInstance.Value;
        private bool _disposedValue;

        private Logger()
        {
            // Create an EventLog instance and assign its source.
            _log = new EventLog
            {
                Source = Source,
                Log = Log
            };

            _username = WindowsIdentity.GetCurrent().Name;
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
                EventLog.CreateEventSource(Source, Log);
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

                EventLog.Delete(Log);
                Debug.WriteLine("Removed Event Source");
            }
        }

        public bool Check()
        {
            try
            {
                var result = EventLog.SourceExists(Source);
                return result;
            }
            catch (System.Security.SecurityException ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        public void LogInfo(string message, int eventId) =>
            _log.WriteEntry(EnrichLog(message), EventLogEntryType.Information, eventId);

        public void LogWarning(string message, int eventId) =>
            _log.WriteEntry(EnrichLog(message), EventLogEntryType.Warning, eventId);

        public void LogError(string message, int eventId) =>
            _log.WriteEntry(EnrichLog(message), EventLogEntryType.Error, eventId);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

        private string EnrichLog(string message) => $"Local Time: {DateTime.Now:O}\nUTC Time: {DateTime.UtcNow:O}\nUser: {_username}\nDetails: \n{message}";

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _log?.Close();
                    _log?.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
