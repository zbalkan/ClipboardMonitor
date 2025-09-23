using ClipboardMonitor.PAN;
using ClipboardMonitor.PaymentBrands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardMonitor.Tests
{
    [TestClass]
    public class JcbTests
    {
        [TestMethod]
        public void Test_PAN_Valid_Jcb_16Digits()
        {
            const string cardNumber = "3530111333300000"; // Valid JCB
            var pan = PANHelper.Parse(cardNumber)[0];

            Assert.AreEqual("JCB", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Jcb_16Digits_WithDashes()
        {
            const string cardNumber = "3530-1113-3330-0000";
            var pan = PANHelper.Parse(cardNumber)[0];

            Assert.AreEqual("JCB", pan.PaymentBrand);
        }

        [TestMethod]
        public void Test_PAN_Valid_Jcb_16Digits_WithSpaces()
        {
            const string cardNumber = "3530 1113 3330 0000";
            var pan = PANHelper.Parse(cardNumber)[0];

            Assert.AreEqual("JCB", pan.PaymentBrand);
        }

        //[TestMethod]
        
        //public void Test_PAN_Valid_Jcb_19Digits()
        //{
        //    const string cardNumber = ""; // 19-digit JCB test number needed
        //    var pan = PANHelper.Parse(cardNumber)[0];

        //    Assert.AreEqual("JCB", pan.PaymentBrand);
        //}

        //[TestMethod]
        //public void Test_PAN_Valid_Jcb_Legacy2131()
        //{
        //    const string cardNumber = ""; // 15-digit JCB legacy, need valid test number
        //    var pan = PANHelper.Parse(cardNumber)[0];

        //    Assert.AreEqual("JCB", pan.PaymentBrand);
        //}

        //[TestMethod]
        //public void Test_PAN_Valid_Jcb_Legacy1800()
        //{
        //    const string cardNumber = ""; // 15-digit JCB legacy, need valid test number
        //    var pan = PANHelper.Parse(cardNumber)[0];

        //    Assert.AreEqual("JCB", pan.PaymentBrand);
        //}

        [TestMethod]
        public void Test_PAN_Invalid_Jcb_WrongPrefix()
        {
            const string cardNumber = "3590123456789012"; // 3590 is outside 3528–3589
            var result = PANHelper.Parse(cardNumber);

            Assert.IsEmpty(result);
        }

        [TestMethod]
        public void Test_PAN_Invalid_Jcb_WrongLength()
        {
            const string cardNumber = "353011133330000"; // 15 digits but not legacy prefix
            var result = PANHelper.Parse(cardNumber);

            Assert.IsEmpty(result);
        }

        [TestMethod]
        public void Test_PAN_Invalid_Jcb_WithLetters()
        {
            const string cardNumber = "3530a11133300000";
            var result = PANHelper.Parse(cardNumber);

            Assert.IsEmpty(result);
        }

        [TestMethod]
        public void Test_PAN_Valid_Jcb_EmbeddedInText()
        {
            const string text = "Card used: 3530111333300000 for purchase";
            var pan = PANHelper.Parse(text)[0];

            Assert.AreEqual("JCB", pan.PaymentBrand);
        }
    }
}
