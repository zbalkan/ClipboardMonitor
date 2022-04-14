using System.Text.RegularExpressions;

namespace ClipboardMonitor.PaymentBrands
{
    public class Amex : PaymentBrandBase, IPaymentBrand
    {
        private Regex? pattern;

        public override Regex Pattern {
            get {
                if (pattern == null)
                {
                    pattern = new(@"(?:\D|^)((?:34|37)[0-9]{2}(?:\ |\-|)[0-9]{6}(?:\ |\-|)[0-9]{5})(?:\D|$)", RegexOptions.Compiled);
                }

                return pattern;
            }
        }

        public override string ToString() => "Amex";
    }
}
