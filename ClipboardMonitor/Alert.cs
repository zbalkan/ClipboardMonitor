using System;

namespace ClipboardMonitor
{
    public struct Alert : IEquatable<Alert>
    {
        public string Title { get; set; }
        public string Detail { get; set; }
        public string Payload { get; set; }

        public bool Equals(Alert other) => Title.Equals(other.Title) &&
                Detail.Equals(other.Detail) &&
                Payload.Equals(other.Payload);
    }
}