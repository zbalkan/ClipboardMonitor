using ClipboardMonitor.PAN;
using ClipboardMonitor.PaymentBrands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardMonitor.Tests
{
    [TestClass]
    public class AmExTests
    {
        [TestMethod]
        public void Test_PAN_Valid_Amex()
        {
            const string cardNumber = "371449635398431";
            var pan = PANHelper.Parse(cardNumber)[0];

            Assert.AreEqual("Amex", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Amex_WithDashes()
        {
            const string cardNumber = "3714-496353-98431";
            var pan = PANHelper.Parse(cardNumber)[0];

            Assert.AreEqual("Amex", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Amex_WithSpaces()
        {
            const string cardNumber = "3714 496353 98431";
            var pan = PANHelper.Parse(cardNumber)[0];

            Assert.AreEqual("Amex", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Amex_EmbeddedInText()
        {
            const string text = "Transaction ID: X123, Card=371449635398431;";
            var pan = PANHelper.Parse(text)[0];

            Assert.AreEqual("Amex", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Invalid_Amex_WrongLength()
        {
            const string cardNumber = "37144963539843"; // 14 digits
            var result = PANHelper.Parse(cardNumber);

            Assert.IsEmpty(result);
        }

        [TestMethod]
        public void Test_PAN_Invalid_Amex_WrongPrefix()
        {
            const string cardNumber = "361449635398431"; // Diners Club prefix
            var result = PANHelper.Parse(cardNumber);

            Assert.IsEmpty(result);
        }

        [TestMethod]
        public void Test_PAN_Invalid_Amex_WithLetters()
        {
            const string cardNumber = "3714a49635398431";
            var result = PANHelper.Parse(cardNumber);

            Assert.IsEmpty(result);
        }

        [TestMethod]
        public void Test_PAN_Invalid_Amex_TooManyDigits()
        {
            const string cardNumber = "3714496353984310"; // 16 digits
            var result = PANHelper.Parse(cardNumber);

            Assert.IsEmpty(result);
        }
    }
}