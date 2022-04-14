using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ClipboardMonitor.PaymentBrands
{
    public abstract class PaymentBrandBase
    {
        public abstract Regex Pattern { get; }

        public IReadOnlyList<string>? Parse(string text)
        {
            var matches = Pattern.Matches(text);

            if (matches != null && matches.Count > 0)
            {
                var readOnlyCollection = matches.Select(m => m.Value).ToList().AsReadOnly();
                return readOnlyCollection;
            }
            return null;
        }

        public bool Validate(string cardNumber) => Pattern.IsMatch(cardNumber);
    }
}