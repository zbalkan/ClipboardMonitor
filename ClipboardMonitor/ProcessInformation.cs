using System;

namespace ClipboardMonitor
{
    public struct ProcessInformation : IEquatable<ProcessInformation>
    {
        public string WindowTitle { get; set; }

        public string ProcessName { get; set; }

        public string ExecutablePath { get; set; }

        public string MainModuleName { get; set; }

        public override int GetHashCode() => (ProcessName,ExecutablePath).GetHashCode();

        public static bool operator ==(ProcessInformation left, ProcessInformation right) => left.Equals(right);

        public static bool operator !=(ProcessInformation left, ProcessInformation right) => !(left == right);

        public bool Equals(ProcessInformation other) => other != default && other.ProcessName == ProcessName && other.ExecutablePath == ExecutablePath;

        public override bool Equals(object obj) => obj is ProcessInformation information && Equals(information);
    }
}
