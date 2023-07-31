using System;

namespace ClipboardMonitor.PAN
{
    public struct SuspectedPANData : IEquatable<SuspectedPANData>
    {
        public string MaskedPAN { get; set; }

        public string PaymentBrand { get; set; }

        public readonly override string ToString() => $"{MaskedPAN} [{PaymentBrand}]";
        public readonly override bool Equals(object? obj) => obj is SuspectedPANData data && Equals(data);
        public readonly bool Equals(SuspectedPANData other) => MaskedPAN == other.MaskedPAN && PaymentBrand == other.PaymentBrand;
        public readonly override int GetHashCode() => HashCode.Combine(MaskedPAN, PaymentBrand);

        public static bool operator ==(SuspectedPANData left, SuspectedPANData right) => left.Equals(right);

        public static bool operator !=(SuspectedPANData left, SuspectedPANData right) => !(left == right);
    }
}