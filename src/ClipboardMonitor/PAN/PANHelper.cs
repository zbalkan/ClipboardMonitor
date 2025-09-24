using System;
using System.Collections.Generic;
using System.Linq;

namespace ClipboardMonitor.PAN
{
    /// <summary>
    /// Provides helper methods for detecting and sanitizing
    /// Primary Account Numbers (PANs) in text.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="PANHelper"/> is a stateless utility class that depends on the
    /// <see cref="PaymentBrandRegistry"/> for knowledge of available
    /// <see cref="IPaymentBrand"/> implementations.
    /// </para>
    /// <para>
    /// It offers two main functions:
    /// <list type="bullet">
    /// <item><description>
    /// <see cref="TryParse"/> – extract suspected PANs, mask them, and return them
    /// with associated payment brand information.
    /// </description></item>
    /// <item><description>
    /// <see cref="Mask"/> – mask all detected PANs in free text, leaving
    /// non-PAN content untouched.
    /// </description></item>
    /// </list>
    /// All masking follows PCI DSS 4.0.1 requirement 3.4.1 (first six and last
    /// four digits visible).
    /// </para>
    /// </remarks>
    public static class PANHelper
    {
        private const int MinimumPANLength = 13;

        /// <summary>
        /// Parses the specified text and extracts all suspected PANs detected by
        /// registered payment brands. Each detected PAN is masked (first 6 and
        /// last 4 digits shown, middle digits replaced with '*') before being
        /// returned in the result set.
        /// </summary>
        /// <param name="text">Input text that may contain PANs.</param>
        /// <returns>
        /// A read-only list of <see cref="SuspectedPANData"/> containing the
        /// masked PAN and the name of the payment brand that matched it.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="text"/> is null or empty.
        /// </exception>
        /// <exception cref="PANException">
        /// Thrown when no payment brands are registered in
        /// <see cref="PaymentBrandRegistry"/>.
        /// </exception>
        public static bool TryParse(string text, out IReadOnlyList<SuspectedPANData> suspectedPANs)
        {
            var result = new List<SuspectedPANData>();
            suspectedPANs = result.AsReadOnly();

            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            var brands = PaymentBrandRegistry.Instance.Brands;
            if (brands.Count == 0)
            {
                return false;

            }

            foreach (var brand in brands)
            {
                var brandMatches = brand.Matches(text);

                foreach (var match in brandMatches)
                {
                    if (Luhn.Validate(match))
                    {
                        var suspected = new SuspectedPANData
                        {
                            MaskedPAN = MaskPan(match),
                            PaymentBrand = brand.ToString()
                        };

                        result.Add(suspected);
                    }
                }
            }

            suspectedPANs = result.AsReadOnly();
            return result.Count > 0;
        }

        /// <summary>
        /// Replaces all detected PANs in the given text with their masked equivalents,
        /// leaving non-PAN content unchanged.
        /// </summary>
        /// <param name="text">Input text that may contain PANs.</param>
        /// <returns>
        /// A sanitized version of the input text where all detected PANs
        /// are masked in compliance with PCI DSS (first 6 and last 4 digits visible).
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="text"/> is null or empty.
        /// </exception>
        /// <exception cref="PANException">
        /// Thrown when no payment brands are registered in
        /// <see cref="PaymentBrandRegistry"/>.
        /// </exception>
        public static string Mask(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Text cannot be null or empty.", nameof(text));
            }

            var brands = PaymentBrandRegistry.Instance.Brands;
            if (brands.Count == 0)
            {
                throw new PANException("No payment brand is defined.");
            }

            var matches = brands.SelectMany(b => b.Matches(text)).Distinct();
            var sanitized = text;

            foreach (var match in matches)
            {
                sanitized = sanitized.Replace(match, MaskPan(match));
            }

            return sanitized;
        }

        /// <summary>
        /// Masks a PAN according to PCI DSS requirement 3.4.1,
        /// showing only the first six and last four digits.
        /// </summary>
        /// <param name="pan">The PAN to be masked.</param>
        /// <returns>
        /// A masked string in which all digits between the BIN (first six digits)
        /// and the last four digits are replaced with '*'.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="pan"/> is null or empty.
        /// </exception>
        /// <exception cref="PANException">
        /// Thrown when <paramref name="pan"/> is shorter than
        /// the minimum allowed PAN length (13 digits).
        /// </exception>
        internal static string MaskPan(string pan)
        {
            if (string.IsNullOrEmpty(pan))
            {
                throw new ArgumentException("Card number cannot be null or empty.", nameof(pan));
            }

            if (pan.Length < MinimumPANLength)
            {
                throw new PANException($"PAN too short to mask: {pan}");
            }

            var stripped = new string(pan.Where(char.IsDigit).ToArray());
            var first = stripped.Substring(0, 6);
            var last = stripped.Substring(stripped.Length - 4, 4);
            var middle = new string('*', stripped.Length - 10);

            return string.Concat(first, middle, last);
        }
    }
}
