using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Windows.Forms;

namespace ClipboardMonitor.Helpers
{
    public static class ClipboardHelper
    {
        private static readonly BlockingCollection<Action> ClipboardActions = new();
        private static readonly Thread ClipboardThread;

        static ClipboardHelper()
        {
            ClipboardThread = new Thread(ClipboardWorkerLoop)
            {
                IsBackground = true,
                Name = "Clipboard STA Worker"
            };
            ClipboardThread.SetApartmentState(ApartmentState.STA);
            ClipboardThread.Start();
        }

        public static string GetText() => Invoke(() => Clipboard.GetText());

        public static void SetText(string data)
        {
            const int maxRetries = 8;
            const int delayMs = 100;

            Invoke(() => {
                for (var i = 0; i < maxRetries; i++)
                {
                    try
                    {
                        Clipboard.Clear();
                        Clipboard.SetText(data);
                        return;
                    }
                    catch (System.Runtime.InteropServices.ExternalException)
                    {
                        Thread.Sleep(delayMs * (i + 1));
                    }
                }

                throw new TimeoutException($"Unable to overwrite the clipboard after {maxRetries} retries.");
            });
        }

        public static bool HasDataFormat(string format) => Invoke(() => {
            var data = Clipboard.GetDataObject();
            if (data == null)
            {
                return false;
            }

            var dob = new DataObject(data);
            foreach (var f in dob.GetFormats(true))
            {
                if (format.Equals(f, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        });

        private static T Invoke<T>(Func<T> action)
        {
            using var completed = new ManualResetEventSlim(false);
            Exception? failure = null;
            T? result = default;

            ClipboardActions.Add(() => {
                try
                {
                    result = action();
                }
                catch (Exception ex)
                {
                    failure = ex;
                }
                finally
                {
                    completed.Set();
                }
            });

            completed.Wait();
            if (failure != null)
            {
                throw failure;
            }

            return result!;
        }

        private static void Invoke(Action action) => Invoke(() => {
            action();
            return true;
        });

        private static void ClipboardWorkerLoop()
        {
            foreach (var action in ClipboardActions.GetConsumingEnumerable())
            {
                action();
            }
        }
    }
}
