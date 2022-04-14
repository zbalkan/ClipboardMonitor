using System.Collections.Generic;

namespace ClipboardMonitor.PaymentBrands
{
    public interface IPaymentBrand
    {
        IReadOnlyList<string>? Parse(string text);
        string ToString();
        bool Validate(string cardNumber);
    }
}