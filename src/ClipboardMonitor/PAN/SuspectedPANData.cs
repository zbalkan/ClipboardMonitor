using System;

namespace ClipboardMonitor.PAN
{
    public struct SuspectedPANData : IEquatable<SuspectedPANData>
    {
        public string MaskedPAN { get; set; }

        public string PaymentBrand { get; set; }

        public override string ToString() => $"{MaskedPAN} [{PaymentBrand}]";
        public override bool Equals(object obj) => obj is SuspectedPANData data && Equals(data);
        public bool Equals(SuspectedPANData other) => MaskedPAN == other.MaskedPAN && PaymentBrand == other.PaymentBrand;
        public override int GetHashCode() => (MaskedPAN, PaymentBrand).GetHashCode();

        public static bool operator ==(SuspectedPANData left, SuspectedPANData right) => left.Equals(right);

        public static bool operator !=(SuspectedPANData left, SuspectedPANData right) => !(left == right);
    }
}