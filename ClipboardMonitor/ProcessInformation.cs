using System;

namespace ClipboardMonitor
{
    public struct ProcessInformation : IEquatable<ProcessInformation>
    {
        public string WindowTitle { get; set; }

        public string ProcessName { get; set; }

        public string ExecutablePath { get; set; }

        public ProcessInformation()
        {
            WindowTitle = string.Empty;
            ProcessName = string.Empty;
            ExecutablePath = string.Empty;
        }

        public readonly override int GetHashCode() => ProcessName.GetHashCode(StringComparison.OrdinalIgnoreCase) ^ ExecutablePath.GetHashCode(StringComparison.OrdinalIgnoreCase);

        public static bool operator ==(ProcessInformation left, ProcessInformation right) => left.Equals(right);

        public static bool operator !=(ProcessInformation left, ProcessInformation right) => !(left == right);

        public readonly bool Equals(ProcessInformation other) => other != null && other.ProcessName == ProcessName && other.ExecutablePath == ExecutablePath;

#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        public readonly override bool Equals(object obj) => obj is ProcessInformation information && Equals(information);
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
    }
}
