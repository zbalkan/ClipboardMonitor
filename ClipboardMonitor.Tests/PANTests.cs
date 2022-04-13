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
            var valid = PAN.Validate(cardNumber, out var cardType);
            Assert.IsTrue(valid);
            Assert.AreEqual(CardType.Mastercard, cardType);
        }

        [TestMethod]
        public void Test_PAN_Valid_Mastercard_WithDashes()
        {
            const string cardNumber = "5105-1051-0510-5100";
            var valid = PAN.Validate(cardNumber, out var cardType);
            Assert.IsTrue(valid);
            Assert.AreEqual(CardType.Mastercard, cardType);
        }

        [TestMethod]
        public void Test_PAN_Valid_Visa()
        {
            const string cardNumber = "4012888888881881";
            var valid = PAN.Validate(cardNumber, out var cardType);
            Assert.IsTrue(valid);
            Assert.AreEqual(CardType.Visa, cardType);
        }

        [TestMethod]
        public void Test_PAN_Valid_Visa_WithDashes()
        {
            const string cardNumber = "4012-8888-8888-1881";
            var valid = PAN.Validate(cardNumber, out var cardType);
            Assert.IsTrue(valid);
            Assert.AreEqual(CardType.Visa, cardType);
        }

        [TestMethod]
        public void Test_PAN_Valid_Amex()
        {
            const string cardNumber = "371449635398431";
            var valid = PAN.Validate(cardNumber, out var cardType);
            Assert.IsTrue(valid);
            Assert.AreEqual(CardType.Amex, cardType);
        }

        [TestMethod]
        public void Test_PAN_Valid_Amex_WithDashes()
        {
            const string cardNumber = "3714-496353-98431";
            var valid = PAN.Validate(cardNumber, out var cardType);
            Assert.IsTrue(valid);
            Assert.AreEqual(CardType.Amex, cardType);
        }
    }
}