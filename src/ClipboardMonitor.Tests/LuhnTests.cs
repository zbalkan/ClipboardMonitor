using ClipboardMonitor.PAN;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardMonitor.Tests
{
    [TestClass]
    public class LuhnTests
    {
        [TestMethod]
        public void Test_Valid_Mastercard()
        {
            const string cardNumber = "5105105105105100";
            var isValid = Luhn.Validate(cardNumber);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void Test_Valid_Mastercard_With_Dashes()
        {
            const string cardNumber = "5105-1051-0510-5100";
            var isValid = Luhn.Validate(cardNumber);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void Test_Invalid_Mastercard()
        {
            const string cardNumber = "5105105105105101";
            var isValid = Luhn.Validate(cardNumber);

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void Test_Invalid_Mastercard_With_Dashes()
        {
            const string cardNumber = "5105-1051-0510-5101";
            var isValid = Luhn.Validate(cardNumber);

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void Test_Valid_Visa()
        {
            const string cardNumber = "4012888888881881";
            var isValid = Luhn.Validate(cardNumber);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void Test_Valid_Visa_With_Dashes()
        {
            const string cardNumber = "4012-8888-8888-1881";
            var isValid = Luhn.Validate(cardNumber);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void Test_Invalid_Visa()
        {
            const string cardNumber = "4012888888881882";
            var isValid = Luhn.Validate(cardNumber);

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void Test_Invalid_Visa_With_Dashes()
        {
            const string cardNumber = "4012-8888-8888-1882";
            var isValid = Luhn.Validate(cardNumber);

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void Test_Valid_Amex()
        {
            const string cardNumber = "371449635398431";
            var isValid = Luhn.Validate(cardNumber);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void Test_Valid_Amex_With_Dashes()
        {
            const string cardNumber = "3714-496353-98431";
            var isValid = Luhn.Validate(cardNumber);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void Test_Invalid_Amex()
        {
            const string cardNumber = "371449635398432";
            var isValid = Luhn.Validate(cardNumber);

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void Test_Invalid_Amex_With_Dashes()
        {
            const string cardNumber = "3714-496353-98432";
            var isValid = Luhn.Validate(cardNumber);

            Assert.IsFalse(isValid);
        }
    }
}
