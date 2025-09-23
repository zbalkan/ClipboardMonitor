using System.Text.RegularExpressions;

namespace ClipboardMonitor.PaymentBrands
{
    public class UnionPay : PaymentBrandBase, IPaymentBrand
    {
        private const string _brand = "UnionPay";

        // UnionPay: 62 + 14–17 digits (16–19 total), allow spaces/dashes
        private readonly Regex _pattern = new Regex(
            @"(?:\D|^)(62(?:[ \-]?[0-9]){14,17})(?=\D|$)",
            RegexOptions.Compiled);

        public override Regex GetPattern() => _pattern;

        public override string ToString() => _brand;
    }
}
