using ClipboardMonitor.PAN;
using ClipboardMonitor.PaymentBrands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardMonitor.Tests
{
    [TestClass]
    public class UnionPayTests
    {
        [TestMethod]
        public void Test_PAN_Valid_UnionPay_16Digits_Debit()
        {
            const string cardNumber = "6212345678901265"; // Debit card, 16 digits, Luhn valid
            var pan = PANHelper.Parse(cardNumber)[0];

            Assert.AreEqual("UnionPay", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_UnionPay_16Digits_Credit()
        {
            const string cardNumber = "6212345678901232"; // Credit card, 16 digits, Luhn valid
            var pan = PANHelper.Parse(cardNumber)[0];

            Assert.AreEqual("UnionPay", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_UnionPay_19Digits()
        {
            const string cardNumber = "6212345678900000003"; // 19 digits, Luhn valid
            var pan = PANHelper.Parse(cardNumber)[0];

            Assert.AreEqual("UnionPay", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_UnionPay_WithDashes()
        {
            const string cardNumber = "6212-3456-7890-1265"; // Debit card with dashes
            var pan = PANHelper.Parse(cardNumber)[0];

            Assert.AreEqual("UnionPay", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_UnionPay_WithSpaces()
        {
            const string cardNumber = "6212 3456 7890 1232"; // Credit card with spaces
            var pan = PANHelper.Parse(cardNumber)[0];

            Assert.AreEqual("UnionPay", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_UnionPay_EmbeddedInText()
        {
            const string text = "Transaction with card: 6212345678901265 succeeded.";
            var pan = PANHelper.Parse(text)[0];

            Assert.AreEqual("UnionPay", pan.PaymentBrand);
        }

        // --- INVALID CASES (wrong length, prefix, chars, sandbox non-Luhn numbers) ---

        [TestMethod]
        public void Test_PAN_Invalid_UnionPay_WrongPrefix()
        {
            const string cardNumber = "6112345678901265"; // Starts with 61
            var result = PANHelper.Parse(cardNumber);

            Assert.IsEmpty(result);
        }

        [TestMethod]
        public void Test_PAN_Invalid_UnionPay_TooShort()
        {
            const string cardNumber = "62123456789002"; // 14 digits, sandbox test
            var result = PANHelper.Parse(cardNumber);

            Assert.IsEmpty(result);
        }

        [TestMethod]
        public void Test_PAN_Invalid_UnionPay_TooLong()
        {
            const string cardNumber = "62123456789000000021"; // 20 digits
            var result = PANHelper.Parse(cardNumber);

            Assert.IsEmpty(result);
        }

        [TestMethod]
        public void Test_PAN_Invalid_UnionPay_WithLetters()
        {
            const string cardNumber = "62123456a8901265"; // Contains a letter
            var result = PANHelper.Parse(cardNumber);

            Assert.IsEmpty(result);
        }
    }
}
