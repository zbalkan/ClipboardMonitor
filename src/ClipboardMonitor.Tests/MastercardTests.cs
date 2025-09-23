using ClipboardMonitor.PAN;
using ClipboardMonitor.PaymentBrands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardMonitor.Tests
{
    [TestClass]
    public class MastercardTests
    {
        [TestInitialize]
        public void Init()
        {
            PANData.Instance.AddPaymentBrand(new Mastercard());
        }

        [TestMethod]
        public void Test_PAN_Valid_Mastercard()
        {
            const string cardNumber = "5105105105105100"; // Classic 51 prefix
            var pan = PANData.Instance.Parse(cardNumber)[0];

            Assert.AreEqual("Mastercard", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Mastercard_WithDashes()
        {
            const string cardNumber = "5105-1051-0510-5100";
            var pan = PANData.Instance.Parse(cardNumber)[0];

            Assert.AreEqual("Mastercard", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Mastercard_WithSpaces()
        {
            const string cardNumber = "5105 1051 0510 5100";
            var pan = PANData.Instance.Parse(cardNumber)[0];

            Assert.AreEqual("Mastercard", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Mastercard_EmbeddedInText()
        {
            const string text = "Payment OK: 5105105105105100 approved.";
            var pan = PANData.Instance.Parse(text)[0];

            Assert.AreEqual("Mastercard", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Mastercard_2Series()
        {
            const string cardNumber = "2221000000000009"; // Lower bound of 2-series
            var pan = PANData.Instance.Parse(cardNumber)[0];

            Assert.AreEqual("Mastercard", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Mastercard_2Series_Upper()
        {
            const string cardNumber = "2720990000000005"; // Upper bound of 2-series
            var pan = PANData.Instance.Parse(cardNumber)[0];

            Assert.AreEqual("Mastercard", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Invalid_Mastercard_WrongLength_Short()
        {
            const string cardNumber = "510510510510510"; // 15 digits
            var result = PANData.Instance.Parse(cardNumber);

            Assert.IsEmpty(result);
        }

        [TestMethod]
        public void Test_PAN_Invalid_Mastercard_WrongLength_Long()
        {
            const string cardNumber = "51051051051051000"; // 17 digits
            var result = PANData.Instance.Parse(cardNumber);

            Assert.IsEmpty(result);
        }

        [TestMethod]
        public void Test_PAN_Invalid_Mastercard_WrongPrefix()
        {
            const string cardNumber = "6011111111111117"; // Discover
            var result = PANData.Instance.Parse(cardNumber);

            Assert.IsEmpty(result);
        }

        [TestMethod]
        public void Test_PAN_Invalid_Mastercard_WithLetters()
        {
            const string cardNumber = "5105a105105105100";
            var result = PANData.Instance.Parse(cardNumber);

            Assert.IsEmpty(result);
        }
    }
}
