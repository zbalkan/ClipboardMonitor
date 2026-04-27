using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ClipboardMonitor.PaymentBrands;

namespace ClipboardMonitor.PAN
{
    /// <summary>
    /// Discovers and stores all available <see cref="IPaymentBrand"/> implementations
    /// in the current assembly. Acts as a central registry for PAN detection.
    /// </summary>
    public sealed class PaymentBrandRegistry
    {
        private readonly List<IPaymentBrand> _paymentBrands;

        private static readonly Lazy<PaymentBrandRegistry> LazyInstance =
            new Lazy<PaymentBrandRegistry>(() => new PaymentBrandRegistry());

        /// <summary>
        /// Gets the singleton instance of the <see cref="PaymentBrandRegistry"/>.
        /// </summary>
        public static PaymentBrandRegistry Instance => LazyInstance.Value;

        /// <summary>
        /// Gets the list of discovered <see cref="IPaymentBrand"/> instances.
        /// </summary>
        public IReadOnlyList<IPaymentBrand> Brands => _paymentBrands;

        private PaymentBrandRegistry()
        {
            _paymentBrands = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(IPaymentBrand).IsAssignableFrom(t)
                            && !t.IsInterface
                            && !t.IsAbstract)
                .Select(t => (IPaymentBrand)Activator.CreateInstance(t))
                .ToList();
        }
    }
}
