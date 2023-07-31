using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ClipboardMonitor.PaymentBrands
{
    public abstract class PaymentBrandBase
    {
        public abstract Regex GetPattern();

        public IReadOnlyList<string>? Parse(string text)
        {
            var matches = GetPattern().Matches(text);

            if (matches is not {Count: > 0})
            {
                return null;
            }

            return matches.Select(m => m.Value).ToList().AsReadOnly();
        }

        public bool Validate(string cardNumber) => GetPattern().IsMatch(cardNumber);
    }
}