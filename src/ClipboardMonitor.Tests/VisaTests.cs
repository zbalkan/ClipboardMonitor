using ClipboardMonitor.PAN;
using ClipboardMonitor.PaymentBrands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardMonitor.Tests
{
    [TestClass]
    public class VisaTests
    {
        [TestInitialize]
        public void Init()
        {
            PANData.Instance.AddPaymentBrand(new Visa());
        }

        [TestMethod]
        public void Test_PAN_Valid_Visa_13Digits()
        {
            const string cardNumber = "4222222222222"; // 13-digit Visa
            var pan = PANData.Instance.Parse(cardNumber)[0];

            Assert.AreEqual("Visa", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Visa_16Digits()
        {
            const string cardNumber = "4012888888881881";
            var pan = PANData.Instance.Parse(cardNumber)[0];

            Assert.AreEqual("Visa", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Visa_19Digits()
        {
            const string cardNumber = "4444333322221111455"; // 19-digit Visa
            var pan = PANData.Instance.Parse(cardNumber)[0];

            Assert.AreEqual("Visa", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Visa_WithDashes()
        {
            const string cardNumber = "4012-8888-8888-1881";
            var pan = PANData.Instance.Parse(cardNumber)[0];

            Assert.AreEqual("Visa", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Visa_WithSpaces()
        {
            const string cardNumber = "4012 8888 8888 1881";
            var pan = PANData.Instance.Parse(cardNumber)[0];

            Assert.AreEqual("Visa", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Visa_EmbeddedInText()
        {
            const string text = "Transaction=OK; Card=4012888888881881;";
            var pan = PANData.Instance.Parse(text)[0];

            Assert.AreEqual("Visa", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Invalid_Visa_WrongLength_14Digits()
        {
            const string cardNumber = "40128888888818"; // 14 digits
            var result = PANData.Instance.Parse(cardNumber);

            Assert.IsEmpty(result);
        }

        [TestMethod]
        public void Test_PAN_Invalid_Visa_WrongLength_15Digits()
        {
            const string cardNumber = "401288888888188"; // 15 digits
            var result = PANData.Instance.Parse(cardNumber);

            Assert.IsEmpty(result);
        }

        [TestMethod]
        public void Test_PAN_Invalid_Visa_WrongLength_17Digits()
        {
            const string cardNumber = "40128888888818818"; // 17 digits
            var result = PANData.Instance.Parse(cardNumber);

            Assert.IsEmpty(result);
        }

        [TestMethod]
        public void Test_PAN_Invalid_Visa_WrongLength_18Digits()
        {
            const string cardNumber = "401288888888188188"; // 18 digits
            var result = PANData.Instance.Parse(cardNumber);

            Assert.IsEmpty(result);
        }

        [TestMethod]
        public void Test_PAN_Invalid_Visa_WrongPrefix()
        {
            const string cardNumber = "3012888888881881"; // Not Visa
            var result = PANData.Instance.Parse(cardNumber);

            Assert.IsEmpty(result);
        }

        [TestMethod]
        public void Test_PAN_Invalid_Visa_WithLetters()
        {
            const string cardNumber = "4012a88888881881";
            var result = PANData.Instance.Parse(cardNumber);

            Assert.IsEmpty(result);
        }
    }
}
