using System.Text.RegularExpressions;

namespace ClipboardMonitor.PaymentBrands
{
    public class Amex : PaymentBrandBase, IPaymentBrand
    {
        private Regex? _pattern;

        public override Regex Pattern {
            get {
                if (_pattern == null)
                {
                    _pattern = new(@"(?:\D|^)((?:34|37)[0-9]{2}(?:\ |\-|)[0-9]{6}(?:\ |\-|)[0-9]{5})(?:\D|$)", RegexOptions.Compiled);
                }

                return _pattern;
            }
        }

        public override string ToString() => "Amex";
    }
}
