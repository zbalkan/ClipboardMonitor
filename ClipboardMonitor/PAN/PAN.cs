using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ClipboardMonitor
{
    public static class PAN
    {
        private static readonly Regex Mastercard = new(@"(?:\D|^)(5[1-5][0-9]{2}(?:\ |\-|)[0-9]{4}(?:\ |\-|)[0-9]{4}(?:\ |\-|)[0-9]{4})(?:\D|$)", RegexOptions.Compiled);
        private static readonly Regex Visa = new(@"(?:\D|^)(4[0-9]{3}(?:\ |\-|)[0-9]{4}(?:\ |\-|)[0-9]{4}(?:\ |\-|)[0-9]{4})(?:\D|$)", RegexOptions.Compiled);
        private static readonly Regex Amex = new(@"(?:\D|^)((?:34|37)[0-9]{2}(?:\ |\-|)[0-9]{6}(?:\ |\-|)[0-9]{5})(?:\D|$)", RegexOptions.Compiled);

        public static IReadOnlyList<PANData> Parse(string text)
        {
            var matches = ParseLine(text);

            if (matches != null && matches.Count != 0)
            {
                var list = new List<PANData>(matches.Count);

                foreach (var suspectedPAN in matches)
                {
                    if (PAN.Validate(suspectedPAN, out var cardType))
                    {
#pragma warning disable CS8601 // Possible null reference assignment.
                        list.Add(new PANData() { MaskedPAN = Mask(suspectedPAN), PaymentBrand = Enum.GetName(cardType) });
#pragma warning restore CS8601 // Possible null reference assignment.
                    }
                }
                return list.AsReadOnly();
            }
            return new List<PANData>().AsReadOnly();
        }

        /// <summary>
        ///     By default all PANs are masked
        /// </summary>
        /// <para>
        ///     PCI-DSS v3.2 - Req.3.3: Mask PAN when displayed (the first six and last four digits 
        ///     are the maximum number of digits to be displayed), such that only personnel with a legitimate
        ///     business need can see more than the first six/last four digits of the PAN.
        /// </para>
        /// <see href="https://www.pcisecuritystandards.org/"/>
        /// <param name="cardNumber">PAN</param>
        /// <returns></returns>
        public static string Mask(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber))
            {
                throw new System.ArgumentException($"'{nameof(cardNumber)}' cannot be null or empty.", nameof(cardNumber));
            }

            var first = cardNumber.Substring(0, 6);
            var middle = cardNumber.Substring(6, cardNumber.Length - 10);
            var last = cardNumber.Substring(cardNumber.Length - 5, 4);

            var maskedArray = new char[middle.Length];

            for (var i = 0; i < middle.Length; i++)
            {
                if (char.IsDigit(middle[i]))
                {
                    maskedArray[i] = '*';
                }
                else
                {
                    maskedArray[i] = middle[i];
                }
            }

            return string.Concat(first, new string(maskedArray), last);
        }

        private static string GetNumbers(string input) => new(input.Where(c => char.IsDigit(c)).ToArray());

        private static bool Validate(string cardNumber, out CardType cardType)
        {
            cardType = GetCardType(cardNumber);
            return cardType != CardType.Invalid && Luhn.Validate(cardNumber);
        }

        private static CardType GetCardType(string cardNumber)
        {
            if (Mastercard.IsMatch(cardNumber))
            {
                return CardType.Mastercard;
            }

            if (Visa.IsMatch(cardNumber))
            {
                return CardType.Visa;
            }

            if (Amex.IsMatch(cardNumber))
            {
                return CardType.Amex;
            }
            return CardType.Invalid;
        }

        private static System.Collections.ObjectModel.ReadOnlyCollection<string> ParseLine(string line)
        {
            var mastercard = PAN.Mastercard.Matches(line);
            var visa = PAN.Visa.Matches(line);
            var amex = PAN.Amex.Matches(line);

            var list = new List<string>(capacity: mastercard.Count + visa.Count + amex.Count);

            list.AddRange(mastercard.Select(item => GetNumbers(item.Value)));
            list.AddRange(visa.Select(item => GetNumbers(item.Value)));
            list.AddRange(amex.Select(item => GetNumbers(item.Value)));

            return list.AsReadOnly();
        }
    }
}
