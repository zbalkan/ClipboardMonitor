using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ClipboardMonitor.PaymentBrands
{
    public abstract class PaymentBrandBase
    {
        public abstract Regex GetPattern();

        public IReadOnlyList<string> Matches(string text)
        {
            var matches = GetPattern().Matches(text);

            if (matches.Count == 0)
            {
                return Array.Empty<string>();
            }

            return matches
                .Cast<Match>()
                .Select(m => m.Groups[1].Value)
                .Where(v => !string.IsNullOrEmpty(v))
                .ToList()
                .AsReadOnly();
        }

        public bool Validate(string cardNumber) => GetPattern().IsMatch(cardNumber);
    }
}