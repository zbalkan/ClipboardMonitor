using System.Text.RegularExpressions;

namespace ClipboardMonitor.PaymentBrands
{
    public class Mastercard : PaymentBrandBase, IPaymentBrand
    {
        private const string _brand = "Mastercard";
        private readonly Regex _pattern = new Regex(@"(?:\D|^)(5[1-5][0-9]{2}(?:\ |\-|)[0-9]{4}(?:\ |\-|)[0-9]{4}(?:\ |\-|)[0-9]{4})(?:\D|$)", RegexOptions.Compiled);

        public override Regex GetPattern() => _pattern;

        public override string ToString() => _brand;
    }
}
