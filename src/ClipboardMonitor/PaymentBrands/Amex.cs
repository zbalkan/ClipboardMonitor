using System.Text.RegularExpressions;

namespace ClipboardMonitor.PaymentBrands
{
    public class Amex : PaymentBrandBase, IPaymentBrand
    {
        private const string _brand = "Amex";
        private readonly Regex _pattern = new Regex(@"(?:\D|^)((?:3[47][0-9]{2})(?:[ \-]?)[0-9]{6}(?:[ \-]?)[0-9]{5})(?:\D|$)", RegexOptions.Compiled);

        public override Regex GetPattern() => _pattern;

        public override string ToString() => _brand;
    }
}
