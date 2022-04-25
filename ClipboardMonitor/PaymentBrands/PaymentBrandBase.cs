using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ClipboardMonitor.PaymentBrands
{
    abstract public class PaymentBrandBase
    {
        abstract public Regex Pattern { get; }

        public IReadOnlyList<string>? Parse(string text)
        {
            var matches = Pattern.Matches(text);

            if (matches is not {Count: > 0})
            {
                return null;
            }

            var readOnlyCollection = matches.Select(m => m.Value).ToList().AsReadOnly();
            return readOnlyCollection;
        }

        public bool Validate(string cardNumber) => Pattern.IsMatch(cardNumber);
    }
}