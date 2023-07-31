using System.Text.RegularExpressions;

namespace ClipboardMonitor.PaymentBrands
{
    public class Visa : PaymentBrandBase, IPaymentBrand
    {
        private Regex? _pattern;

        public override Regex GetPattern()
        {
            _pattern ??= new(@"(?:\D|^)(4[0-9]{3}(?:\ |\-|)[0-9]{4}(?:\ |\-|)[0-9]{4}(?:\ |\-|)[0-9]{4})(?:\D|$)", RegexOptions.Compiled);

            return _pattern;
        }

        public override string ToString() => "Visa";
    }
}
