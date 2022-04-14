using System;

namespace ClipboardMonitor
{
    public struct PANData : IEquatable<PANData>
    {
        public string MaskedPAN { get; set; }

        public string PaymentBrand { get; set; }

        public override string ToString() => $"{MaskedPAN} [{PaymentBrand}]";
        public override bool Equals(object? obj) => obj is PANData data && Equals(data);
        public bool Equals(PANData other) => MaskedPAN == other.MaskedPAN && PaymentBrand == other.PaymentBrand;
        public override int GetHashCode() => HashCode.Combine(MaskedPAN, PaymentBrand);

        public static bool operator ==(PANData left, PANData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PANData left, PANData right)
        {
            return !(left == right);
        }
    }
}