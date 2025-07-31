using System;
using System.Collections.Generic;
using System.Linq;
using ClipboardMonitor.PaymentBrands;

namespace ClipboardMonitor.PAN
{
    public sealed class PANData
    {
        private readonly List<IPaymentBrand> _paymentBrands;

        private const int MinimumPANLength = 15;

        private static readonly Lazy<PANData> LazyInstance = new Lazy<PANData>(() => new PANData());

        public static PANData Instance => LazyInstance.Value;

        private PANData()
        {
            _paymentBrands = new List<IPaymentBrand>();
        }

        public PANData AddPaymentBrand(IPaymentBrand paymentBrand)
        {
            _paymentBrands.Add(paymentBrand);
            return this;
        }

        public IReadOnlyList<SuspectedPANData> Parse(string text)
        {
            if (_paymentBrands == null || _paymentBrands.Count == 0)
            {
                throw new PANException("No payment brand is defined. Define at least one payment brand.");
            }

            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException($"'{nameof(text)}' cannot be null or empty.", nameof(text));
            }

            var maxPossibleNumberOfPANs = text.Length / MinimumPANLength;
            var list = new List<SuspectedPANData>(maxPossibleNumberOfPANs);

            foreach (var brand in _paymentBrands)
            {
                var result = brand.Parse(text);
                if (result.Count == 0)
                {
                    continue;
                }

                var brandName = brand.ToString();

                list.AddRange(result.Select(p => new SuspectedPANData { MaskedPAN = Mask(GetOnlyNumbers(p)), PaymentBrand = brandName }));
            }

            return list.AsReadOnly();
        }

        /// <summary>
        ///     By default all PANs are masked
        /// </summary>
        /// <para>
        ///     PCI DSS 4.0.1 Req. 3.4.1: PAN is masked when displayed (the BIN and last four digits are the maximum number of digits to be displayed), 
        ///     such that only personnel with a legitimate business need can see more than the BIN and last four digits of the PAN.
        /// </para>
        /// <see href="https://www.pcisecuritystandards.org/"/>
        /// <param name="cardNumber">PAN</param>
        /// <returns>Masked string</returns>
        private string Mask(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber))
            {
                throw new ArgumentException($"'{nameof(cardNumber)}' cannot be null or empty.", nameof(cardNumber));
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

        private string GetOnlyNumbers(string input) => new string(input.Where(char.IsDigit).ToArray());
    }
}
