namespace ClipboardMonitor
{
    public struct ProcessInformation : System.IEquatable<ProcessInformation>
    {
        public string WindowTitle { get; set; }

        public string ProcessName { get; set; }

        public string ExecutablePath { get; set; }

#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        public override bool Equals(object obj) => obj is ProcessInformation information && Equals(information);
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).

        public override int GetHashCode() => ProcessName.GetHashCode(System.StringComparison.OrdinalIgnoreCase) ^ ExecutablePath.GetHashCode(System.StringComparison.OrdinalIgnoreCase);

        public static bool operator ==(ProcessInformation left, ProcessInformation right) => left.Equals(right);

        public static bool operator !=(ProcessInformation left, ProcessInformation right) => !(left == right);

        public bool Equals(ProcessInformation other) => other != default && other.ProcessName == ProcessName && other.ExecutablePath == ExecutablePath;
    }
}
