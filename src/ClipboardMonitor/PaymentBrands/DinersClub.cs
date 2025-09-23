using System.Text.RegularExpressions;

namespace ClipboardMonitor.PaymentBrands
{
    public class DinersClub : PaymentBrandBase, IPaymentBrand
    {
        private const string _brand = "Diners Club";

        // Diners Club: 300–305, 3095, 36, 38–39 (14 digits, allow spaces/dashes)
        private readonly Regex _pattern = new Regex(
            @"(?:\D|^)((?:30[0-5][0-9]|3095|36[0-9]{2}|3[89][0-9]{2})(?:[ \-]?[0-9]){10})(?:\D|$)",
            RegexOptions.Compiled);

        public override Regex GetPattern() => _pattern;

        public override string ToString() => _brand;
    }
}
