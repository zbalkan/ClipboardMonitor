using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardMonitor.Tests
{
    [TestClass]
    public class PANTests
    {
        [TestMethod]
        public void Test_PAN_Valid_Mastercard()
        {
            const string cardNumber = "5105105105105100";
            var pan = PAN.Parse(cardNumber)[0];

            Assert.AreEqual("Mastercard", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Mastercard_WithDashes()
        {
            const string cardNumber = "5105-1051-0510-5100";
            var pan = PAN.Parse(cardNumber)[0];

            Assert.AreEqual("Mastercard", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Visa()
        {
            const string cardNumber = "4012888888881881";
            var pan = PAN.Parse(cardNumber)[0];

            Assert.AreEqual("Visa", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Visa_WithDashes()
        {
            const string cardNumber = "4012-8888-8888-1881";
            var pan = PAN.Parse(cardNumber)[0];

            Assert.AreEqual("Visa", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Amex()
        {
            const string cardNumber = "371449635398431";
            var pan = PAN.Parse(cardNumber)[0];

            Assert.AreEqual("Amex", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Amex_WithDashes()
        {
            const string cardNumber = "371449635398431";
            var pan = PAN.Parse(cardNumber)[0];

            Assert.AreEqual("Amex", pan.PaymentBrand);
        }
    }
}