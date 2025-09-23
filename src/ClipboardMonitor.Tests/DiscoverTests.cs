using ClipboardMonitor.PAN;
using ClipboardMonitor.PaymentBrands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardMonitor.Tests
{
    [TestClass]
    public class DiscoverTests
    {
        [TestInitialize]
        public void Init()
        {
            PANData.Instance.AddPaymentBrand(new Discover());
        }

        [TestMethod]
        public void Test_PAN_Valid_Discover_6011()
        {
            const string cardNumber = "6011111111111117";
            var pan = PANData.Instance.Parse(cardNumber)[0];

            Assert.AreEqual("Discover", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Discover_65()
        {
            const string cardNumber = "6500000000000002";
            var pan = PANData.Instance.Parse(cardNumber)[0];

            Assert.AreEqual("Discover", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Discover_644()
        {
            const string cardNumber = "6440000000000007";
            var pan = PANData.Instance.Parse(cardNumber)[0];

            Assert.AreEqual("Discover", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Discover_622Range()
        {
            const string cardNumber = "6221261111111111"; // lower bound, Luhn-valid
            var pan = PANData.Instance.Parse(cardNumber)[0];

            Assert.AreEqual("Discover", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Discover_WithDashes()
        {
            const string cardNumber = "6011-1111-1111-1117";
            var pan = PANData.Instance.Parse(cardNumber)[0];

            Assert.AreEqual("Discover", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Discover_WithSpaces()
        {
            const string cardNumber = "6011 1111 1111 1117";
            var pan = PANData.Instance.Parse(cardNumber)[0];

            Assert.AreEqual("Discover", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Discover_EmbeddedInText()
        {
            const string text = "Approved: 6011111111111117 for purchase.";
            var pan = PANData.Instance.Parse(text)[0];

            Assert.AreEqual("Discover", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Invalid_Discover_WrongLength()
        {
            const string cardNumber = "601111111111111"; // 15 digits
            var result = PANData.Instance.Parse(cardNumber);

            Assert.IsEmpty(result);
        }

        [TestMethod]
        public void Test_PAN_Invalid_Discover_WrongPrefix()
        {
            const string cardNumber = "6010123412341234"; // 6010 not valid
            var result = PANData.Instance.Parse(cardNumber);

            Assert.IsEmpty(result);
        }

        [TestMethod]
        public void Test_PAN_Invalid_Discover_WithLetters()
        {
            const string cardNumber = "6011a11111111117";
            var result = PANData.Instance.Parse(cardNumber);

            Assert.IsEmpty(result);
        }
    }
}
