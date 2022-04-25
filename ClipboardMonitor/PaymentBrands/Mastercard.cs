using System.Text.RegularExpressions;

namespace ClipboardMonitor.PaymentBrands
{
    public class Mastercard : PaymentBrandBase, IPaymentBrand
    {
        private Regex? _pattern;
        public override Regex Pattern {
            get {
                if (_pattern == null)
                {
                    _pattern = new(@"(?:\D|^)(5[1-5][0-9]{2}(?:\ |\-|)[0-9]{4}(?:\ |\-|)[0-9]{4}(?:\ |\-|)[0-9]{4})(?:\D|$)", RegexOptions.Compiled);
                }

                return _pattern;
            }
        }

        public override string ToString() => "Mastercard";
    }
}
