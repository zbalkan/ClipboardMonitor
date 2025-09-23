using System.Text.RegularExpressions;

namespace ClipboardMonitor.PaymentBrands
{
    public class Jcb : PaymentBrandBase, IPaymentBrand
    {
        private const string _brand = "JCB";

        // JCB: 3528–3589 (16–19 digits), plus 2131 & 1800 (15 digits)
        private readonly Regex _pattern = new Regex(
            @"(?:\D|^)((?:2131|1800)(?:[ \-]?[0-9]){11}|35(?:2[89]|[3-8][0-9])(?:[ \-]?[0-9]){12,15})(?:\D|$)",
            RegexOptions.Compiled);

        public override Regex GetPattern() => _pattern;

        public override string ToString() => _brand;
    }
}
