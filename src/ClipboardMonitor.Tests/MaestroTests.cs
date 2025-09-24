using ClipboardMonitor.PAN;
using ClipboardMonitor.PaymentBrands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardMonitor.Tests
{
    [TestClass]
    public class MaestroTests
    {
        [TestMethod]
        public void Test_PAN_Valid_Maestro_5018()
        {
            const string cardNumber = "501883388073561126";
            _  = PANHelper.TryParse(cardNumber, out var pans);

            Assert.AreEqual("Maestro", pans[0].PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Maestro_5020()
        {
            const string cardNumber = "5020290416621666007";
            _  = PANHelper.TryParse(cardNumber, out var pans);

            Assert.AreEqual("Maestro", pans[0].PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Maestro_5038()
        {
            const string cardNumber = "5038309169337926360";
            _  = PANHelper.TryParse(cardNumber, out var pans);

            Assert.AreEqual("Maestro", pans[0].PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Maestro_5893()
        {
            const string cardNumber = "5893300725462249";
            _  = PANHelper.TryParse(cardNumber, out var pans);

            Assert.AreEqual("Maestro", pans[0].PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Maestro_6304()
        {
            const string cardNumber = "63040459290493947";
            _  = PANHelper.TryParse(cardNumber, out var pans);

            Assert.AreEqual("Maestro", pans[0].PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Maestro_6759()
        {
            const string cardNumber = "6759649826438453";
            _  = PANHelper.TryParse(cardNumber, out var pans);

            Assert.AreEqual("Maestro", pans[0].PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Maestro_6761()
        {
            const string cardNumber = "6761000000000006";
            _  = PANHelper.TryParse(cardNumber, out var pans);

            Assert.AreEqual("Maestro", pans[0].PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Maestro_6762()
        {
            const string cardNumber = "6762018936274528";
            _  = PANHelper.TryParse(cardNumber, out var pans);

            Assert.AreEqual("Maestro", pans[0].PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Maestro_6763()
        {
            const string cardNumber = "6763774571716764";
            _  = PANHelper.TryParse(cardNumber, out var pans);

            Assert.AreEqual("Maestro", pans[0].PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Maestro_6771()
        {
            const string cardNumber = "6771798021000016";
            _  = PANHelper.TryParse(cardNumber, out var pans);

            Assert.AreEqual("Maestro", pans[0].PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Maestro_6761_WithSpaces()
        {
            const string cardNumber = "6761 5897 7391 4468";
            _  = PANHelper.TryParse(cardNumber, out var pans);

            Assert.AreEqual("Maestro", pans[0].PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Maestro_WithDashes()
        {
            const string cardNumber = "6759-6498-2643-8453";
            _  = PANHelper.TryParse(cardNumber, out var pans);

            Assert.AreEqual("Maestro", pans[0].PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Maestro_EmbeddedInText()
        {
            const string text = "Customer used card 5020 7807 9855 4848 for purchase.";
            _ = PANHelper.TryParse(text, out var pans);

            Assert.AreEqual("Maestro", pans[0].PaymentBrand);
        }

        // ❌ Invalid Maestro PANs

        [TestMethod]
        public void Test_PAN_Invalid_Maestro_WrongPrefix()
        {
            const string cardNumber = "5412345678901234"; // Mastercard
            var result = PANHelper.TryParse(cardNumber, out var _);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Test_PAN_Invalid_Maestro_TooShort()
        {
            const string cardNumber = "67596498264"; // 11 digits
            var result = PANHelper.TryParse(cardNumber, out var _);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Test_PAN_Invalid_Maestro_TooLong()
        {
            const string cardNumber = "67596498264384531234"; // 20 digits
            var result = PANHelper.TryParse(cardNumber, out var _);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Test_PAN_Invalid_Maestro_WithLetters()
        {
            const string cardNumber = "6759a49826438453";
            var result = PANHelper.TryParse(cardNumber, out var _);

            Assert.IsTrue(result);
        }
    }
}
