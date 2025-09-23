using System.Text.RegularExpressions;

namespace ClipboardMonitor.PaymentBrands
{
    public class Maestro : PaymentBrandBase, IPaymentBrand
    {
        private const string _brand = "Maestro";

        private readonly Regex _pattern = new Regex(
            @"(?:\D|^)((?:5018|5020|5038|5893|6304|6759|6761|6762|6763)(?:[ \-]?[0-9]){8,15})(?:\D|$)",
            RegexOptions.Compiled);

        public override Regex GetPattern() => _pattern;

        public override string ToString() => _brand;
    }
}
