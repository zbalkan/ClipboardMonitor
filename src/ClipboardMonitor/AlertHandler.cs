using System;
using ClipboardMonitor.Helpers;

namespace ClipboardMonitor
{
    public class AlertHandler
    {
        private static readonly Lazy<AlertHandler> LazyInstance = new(() => new AlertHandler());
        public static AlertHandler Instance => LazyInstance.Value;

        public string SubstituteText { get; set; } = "This content has been removed for your safety.";

        internal void InvokeAlert(Alert alert)
        {
            if (alert != null)
            {
                if (alert.ClearClipboard)
                {
                    try
                    {
                        ClipboardHelper.SetText(SubstituteText);
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.LogError($"Failed to overwrite clipboard during alert handling.\n{ex}", 33);
                    }
                }

                var logMessage = $"{alert.Title}\n\n{alert.Detail}";
                Logger.Instance.LogWarning(logMessage, 20);
            }
        }
    }
}
