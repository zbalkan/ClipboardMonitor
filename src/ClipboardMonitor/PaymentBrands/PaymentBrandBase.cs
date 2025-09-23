using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ClipboardMonitor.PaymentBrands
{
    public abstract class PaymentBrandBase
    {
        public abstract Regex GetPattern();

        public IReadOnlyList<string> Parse(string text)
        {
            var matches = GetPattern().Matches(text);

            if (matches.Count == 0)
            {
                return (IReadOnlyList<string>)Enumerable.Empty<string>();
            }

            return matches.Cast<Match>().Select(m => m.Value).ToList().AsReadOnly();
        }

        public bool Validate(string cardNumber) => GetPattern().IsMatch(cardNumber);
    }
}