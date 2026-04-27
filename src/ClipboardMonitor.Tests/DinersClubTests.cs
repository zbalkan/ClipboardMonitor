using ClipboardMonitor.PAN;
using ClipboardMonitor.PaymentBrands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardMonitor.Tests
{
    [TestClass]
    public class DinersClubTests
    {
        [TestInitialize]
        public void Init()
        {
            PANData.Instance.AddPaymentBrand(new DinersClub());
        }

        [TestMethod]
        public void Test_PAN_Valid_DinersClub_Classic()
        {
            const string cardNumber = "30569309025904"; // Known valid DC test number
            var pan = PANData.Instance.Parse(cardNumber)[0];

            Assert.AreEqual("Diners Club", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_DinersClub_WithDashes()
        {
            const string cardNumber = "3056-9309-0259-04";
            var pan = PANData.Instance.Parse(cardNumber)[0];

            Assert.AreEqual("Diners Club", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_DinersClub_WithSpaces()
        {
            const string cardNumber = "3056 9309 0259 04";
            var pan = PANData.Instance.Parse(cardNumber)[0];

            Assert.AreEqual("Diners Club", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_DinersClub_36Series()
        {
            const string cardNumber = "36148900647913"; // Starts with 36
            var pan = PANData.Instance.Parse(cardNumber)[0];

            Assert.AreEqual("Diners Club", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_DinersClub_38Series()
        {
            const string cardNumber = "38520000023237"; // Starts with 38
            var pan = PANData.Instance.Parse(cardNumber)[0];

            Assert.AreEqual("Diners Club", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Invalid_DinersClub_WrongLength()
        {
            const string cardNumber = "305693090259041"; // 15 digits, should fail
            var result = PANData.Instance.Parse(cardNumber);

            Assert.IsEmpty(result);
        }

        [TestMethod]
        public void Test_PAN_Invalid_DinersClub_WrongPrefix()
        {
            const string cardNumber = "31569309025904"; // invalid 31 prefix
            var result = PANData.Instance.Parse(cardNumber);

            Assert.IsEmpty(result);
        }

        [TestMethod]
        public void Test_PAN_Invalid_DinersClub_WithLetters()
        {
            const string cardNumber = "3056a9309025904";
            var result = PANData.Instance.Parse(cardNumber);

            Assert.IsEmpty(result);
        }

        [TestMethod]
        public void Test_PAN_Valid_DinersClub_EmbeddedInText()
        {
            const string text = "Transaction completed: 30569309025904 on account";
            var pan = PANData.Instance.Parse(text)[0];

            Assert.AreEqual("Diners Club", pan.PaymentBrand);
        }
    }
}
