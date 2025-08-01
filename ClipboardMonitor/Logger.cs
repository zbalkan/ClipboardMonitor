using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardMonitor
{
    internal sealed class Logger : IDisposable
    {
        private const string Log = "ClipboardMonitor";
        private const string Source = "ClipboardMonitor";
        private static readonly Lazy<Logger> LazyInstance = new Lazy<Logger>(() => new Logger());
        public static readonly Logger Instance = LazyInstance.Value;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly EventLog _log;

        private readonly BlockingCollection<LogItem> _queue =
            new BlockingCollection<LogItem>(boundedCapacity: 1024);

        private readonly StringBuilder _sb = new StringBuilder(256);
        private readonly string _username;
        private readonly Task _worker;
        private volatile bool _disposed;

        private Logger()
        {
            _log = new EventLog { Source = Source, Log = Log };
            _username = WindowsIdentity.GetCurrent().Name;

            _worker = Task.Factory.StartNew(Consume, _cts.Token,
                                            TaskCreationOptions.LongRunning,
                                            TaskScheduler.Default);
        }

        /// <summary>
        ///     Checks the existence the event source for ClipboardMonitor.
        /// </summary>
        public bool Check()
        {
            try
            {
                var result = EventLog.SourceExists(Source);
                return result;
            }
            catch (SecurityException ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        ///     Creates the event source for ClipboardMonitor. Requires elevated privileges.
        /// </summary>
        public void Install()
        {
            // Create the source, if it does not already exist.
            if (Check())
            {
                return;
            }

            //An event log source should not be created and immediately used.
            //There is a latency time to enable the source, it should be created
            //prior to executing the application that uses the source.
            //Execute this sample a second time to use the new source.
            EventLog.CreateEventSource(Source, Log);
            Debug.WriteLine("Created Event Source");
            Debug.WriteLine("Exiting, execute the application a second time to use the source.");
            // The source is created.  Exit the application to allow it to be registered.
        }

        public void LogError(string message, int id) => Enqueue(EventLogEntryType.Error, message, id);

        public void LogInfo(string message, int id) => Enqueue(EventLogEntryType.Information, message, id);

        public void LogWarning(string message, int id) => Enqueue(EventLogEntryType.Warning, message, id);

        /// <summary>
        ///     Removes the event source for ClipboardMonitor. Requires elevated privileges.
        /// </summary>
        public void Uninstall()
        {
            if (!Check())
            {
                return;
            }

            Debug.WriteLine("Uninstalling event log source.");

            if (_log != null)
            {
                _log.Clear(); // Might be unnecessary
                _log.Close();
                _log.Dispose();
            }

            EventLog.Delete(Log);
            Debug.WriteLine("Removed Event Source");
        }

        private void Consume()
        {
            foreach (var item in _queue.GetConsumingEnumerable())
            {
                TryWrite(item);
            }
        }

        private void Enqueue(EventLogEntryType type, string message, int eventId)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Logger));
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var item = new LogItem(type, Enrich(message), eventId);

            while (!_queue.TryAdd(item))
            {
                _queue.TryTake(out _);
            }
        }

        private string Enrich(string msg)
        {
            _sb.Clear();
            _sb.Append("Local Time: ").AppendLine(DateTime.Now.ToString("O"))
                .Append("UTC Time: ").AppendLine(DateTime.UtcNow.ToString("O"))
                .Append("User: ").AppendLine(_username)
                .AppendLine("Details: ")
                .AppendLine(msg)
                .AppendLine();
            return _sb.ToString();
        }

        private void TryWrite(LogItem item)
        {
            try
            {
                _log.WriteEntry(item.Message, item.Type, item.EventId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Logger write failed (1st try): {ex}");
                Thread.Sleep(250);
                try { _log.WriteEntry(item.Message, item.Type, item.EventId); }
                catch (Exception ex2)
                {
                    Debug.WriteLine($"Logger write failed (2nd try): {ex2}");
                }
            }
        }

        private readonly struct LogItem
        {
            public LogItem(EventLogEntryType type, string message, int eventId)
            { Type = type; Message = message; EventId = eventId; }

            public int EventId { get; }
            public string Message { get; }
            public EventLogEntryType Type { get; }
        }

        #region Dispose

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _queue.CompleteAdding();
                if (!_worker.Wait(2000))
                {
                    _cts.Cancel();
                }

                _cts.Dispose();
                _queue.Dispose();

                _log?.Close();
                _log?.Dispose();
            }

            _disposed = true;
        }

        #endregion Dispose
    }
}