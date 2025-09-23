using System.Text.RegularExpressions;

namespace ClipboardMonitor.PaymentBrands
{
    public class Maestro : PaymentBrandBase, IPaymentBrand
    {
        private const string _brand = "Maestro";

        private readonly Regex _pattern = new Regex(
            @"(?:\D|^)((?:5[0678]\d\d|6304|67\d\d)(?:[ \-]?\d){8,15})(?:\D|$)",
            RegexOptions.Compiled);

        public override Regex GetPattern() => _pattern;

        public override string ToString() => _brand;
    }
}
