using System;

namespace ClipboardMonitor
{
    public class AlertHandler
    {
        private static readonly Lazy<AlertHandler> LazyInstance = new Lazy<AlertHandler>(() => new AlertHandler());
        public static readonly AlertHandler Instance = LazyInstance.Value;

        public string SubstituteText { get; set; }

        internal void InvokeAlert(Alert alert)
        {
            if (alert != null)
            {
                if (alert.ClearClipboard)
                {
                    ClipboardHelper.SetText(SubstituteText);
                }

                var logMessage = $"{alert.Title}\n\n{alert.Detail}";
                Logger.Instance.LogWarning(logMessage, 20);
            }
        }
    }
}
