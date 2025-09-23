using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ClipboardMonitor.PaymentBrands
{
    public interface IPaymentBrand
    {
        Regex GetPattern();
        IReadOnlyList<string> Matches(string text);
        string ToString();
        bool Validate(string cardNumber);
    }
}