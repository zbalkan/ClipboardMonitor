using ClipboardMonitor.PAN;
using ClipboardMonitor.PaymentBrands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardMonitor.Tests
{
    [TestClass]
    public class MastercardTests
    {
        [TestMethod]
        public void Test_PAN_Valid_Mastercard()
        {
            const string cardNumber = "5105105105105100"; // Classic 51 prefix
            _  = PANHelper.TryParse(cardNumber, out var pans);

            Assert.AreEqual("Mastercard", pans[0].PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Mastercard_WithDashes()
        {
            const string cardNumber = "5105-1051-0510-5100";
            _  = PANHelper.TryParse(cardNumber, out var pans);

            Assert.AreEqual("Mastercard", pans[0].PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Mastercard_WithSpaces()
        {
            const string cardNumber = "5105 1051 0510 5100";
            _  = PANHelper.TryParse(cardNumber, out var pans);

            Assert.AreEqual("Mastercard", pans[0].PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Mastercard_EmbeddedInText()
        {
            const string text = "Payment OK: 5105105105105100 approved.";
            _ = PANHelper.TryParse(text, out var pans);

            Assert.AreEqual("Mastercard", pans[0].PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Mastercard_2Series()
        {
            const string cardNumber = "2221000000000009"; // Lower bound of 2-series
            _  = PANHelper.TryParse(cardNumber, out var pans);

            Assert.AreEqual("Mastercard", pans[0].PaymentBrand);
        }


        [TestMethod]
        public void Test_PAN_Invalid_Mastercard_WrongLength_Short()
        {
            const string cardNumber = "510510510510510"; // 15 digits
            var result = PANHelper.TryParse(cardNumber, out var _);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Test_PAN_Invalid_Mastercard_WrongLength_Long()
        {
            const string cardNumber = "51051051051051000"; // 17 digits
            var result = PANHelper.TryParse(cardNumber, out var _);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Test_PAN_Invalid_Mastercard_WrongPrefix()
        {
            const string cardNumber = "9911111111111111"; // Invalid for any brand
            var result = PANHelper.TryParse(cardNumber, out var _);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Test_PAN_Invalid_Mastercard_WithLetters()
        {
            const string cardNumber = "5105a105105105100";
            var result = PANHelper.TryParse(cardNumber, out var _);

            Assert.IsTrue(result);
        }
    }
}
