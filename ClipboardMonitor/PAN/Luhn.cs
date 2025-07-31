using System;
using System.Globalization;
using System.Linq;

namespace ClipboardMonitor.PAN
{
    public static class Luhn
    {
        /// <summary>
        /// Validate number against Luhn Algorithm
        /// </summary>
        /// <param name="cardNumber">Card number</param>
        /// <returns>bool</returns>
        public static bool Validate(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber))
            {
                return false;
            }

            var sum = 0;
            var alternate = false;
            var nx = cardNumber.Replace("-", "").Replace(" ", "").ToArray();

            for (var i = nx.Length - 1; i >= 0; i--)
            {
                var n = int.Parse(nx[i].ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture);

                if (alternate)
                {
                    n *= 2;

                    if (n > 9)
                    {
                        n = n % 10 + 1;
                    }
                }
                sum += n;
                alternate = !alternate;
            }
            return sum % 10 == 0;
        }
    }
}
