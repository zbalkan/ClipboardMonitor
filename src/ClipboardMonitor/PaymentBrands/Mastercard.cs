using System.Text.RegularExpressions;

namespace ClipboardMonitor.PaymentBrands
{
    public class Mastercard : PaymentBrandBase, IPaymentBrand
    {
        private const string _brand = "Mastercard";
        private readonly Regex _pattern = new Regex(@"(?:\D|^)((?:5[1-5][0-9]{2}|2(?:2[2-9][0-9]|[3-6][0-9]{2}|7[01][0-9]|720))(?:[ \-]?)[0-9]{4}(?:[ \-]?)[0-9]{4}(?:[ \-]?)[0-9]{4})(?:\D|$)", RegexOptions.Compiled);

        public override Regex GetPattern() => _pattern;

        public override string ToString() => _brand;
    }
}
