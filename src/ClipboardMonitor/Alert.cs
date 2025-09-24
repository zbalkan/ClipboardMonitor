namespace ClipboardMonitor
{
    public class Alert
    {
        public string Title { get; set; }
        public string Detail { get; set; }
        public string Payload { get; set; }
        public bool ClearClipboard {  get; set; } = true;
    }
}