using System.Text.RegularExpressions;

namespace ClipboardMonitor.PaymentBrands
{
    public class Discover : PaymentBrandBase, IPaymentBrand
    {
        private const string _brand = "Discover";

        private readonly Regex _pattern = new Regex(
            @"(?<!\d)((?:6011|65[0-9]{2}|64[4-9][0-9])(?:[ \-]?[0-9]){12,15}|622(?:12[6-9]|1[3-9][0-9]|[2-8][0-9]{2}|9[01][0-9]|92[0-5])(?:[ \-]?[0-9]){10,13})(?!\d)",
            RegexOptions.Compiled);

        public override Regex GetPattern() => _pattern;

        public override string ToString() => _brand;
    }
}
