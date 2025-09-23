using System.Text.RegularExpressions;

namespace ClipboardMonitor.PaymentBrands
{
    public class Visa : PaymentBrandBase, IPaymentBrand
    {
        private const string _brand = "Visa";
        private readonly Regex _pattern = new Regex(
            @"(?:\D|^)(4[0-9]{12}|4[0-9]{3}(?:[ \-]?[0-9]{4}){3}|4[0-9]{3}(?:[ \-]?[0-9]{4}){3}[ \-]?[0-9]{3})(?:\D|$)",
            RegexOptions.Compiled);

        public override Regex GetPattern() => _pattern;

        public override string ToString() => _brand;
    }
}
