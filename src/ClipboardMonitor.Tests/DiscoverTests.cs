using ClipboardMonitor.PAN;
using ClipboardMonitor.PaymentBrands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardMonitor.Tests
{
    [TestClass]
    public class DiscoverTests
    {
        [TestMethod]
        public void Test_PAN_Valid_Discover_6011()
        {
            const string cardNumber = "6011111111111117";
            _  = PANHelper.TryParse(cardNumber, out var pans);

            Assert.AreEqual("Discover", pans[0].PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Discover_65()
        {
            const string cardNumber = "6500000000000002";
            _  = PANHelper.TryParse(cardNumber, out var pans);

            Assert.AreEqual("Discover", pans[0].PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Discover_WithDashes()
        {
            const string cardNumber = "6011-1111-1111-1117";
            _  = PANHelper.TryParse(cardNumber, out var pans);

            Assert.AreEqual("Discover", pans[0].PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Discover_WithSpaces()
        {
            const string cardNumber = "6011 1111 1111 1117";
            _  = PANHelper.TryParse(cardNumber, out var pans);

            Assert.AreEqual("Discover", pans[0].PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Discover_EmbeddedInText()
        {
            const string text = "Approved: 6011111111111117 for purchase.";
            _ = PANHelper.TryParse(text , out var pans);

            Assert.AreEqual("Discover", pans[0].PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Invalid_Discover_WrongLength()
        {
            const string cardNumber = "601111111111111"; // 15 digits
            var result = PANHelper.TryParse(cardNumber, out var _);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Test_PAN_Invalid_Discover_WrongPrefix()
        {
            const string cardNumber = "6010123412341234"; // 6010 not valid
            var result = PANHelper.TryParse(cardNumber, out var _);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Test_PAN_Invalid_Discover_WithLetters()
        {
            const string cardNumber = "6011a11111111117";
            var result = PANHelper.TryParse(cardNumber, out var _);

            Assert.IsTrue(result);
        }
    }
}
