using System.Text.RegularExpressions;

namespace ClipboardMonitor.PaymentBrands
{
    public class Visa : PaymentBrandBase, IPaymentBrand
    {
        private Regex? pattern;

        public override Regex Pattern {
            get {
                if (pattern == null)
                {
                    pattern = new(@"(?:\D|^)(4[0-9]{3}(?:\ |\-|)[0-9]{4}(?:\ |\-|)[0-9]{4}(?:\ |\-|)[0-9]{4})(?:\D|$)", RegexOptions.Compiled);
                }

                return pattern;
            }
        }

        public override string ToString() => "Visa";
    }
}
