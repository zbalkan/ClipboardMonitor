using ClipboardMonitor.PAN;
using ClipboardMonitor.PaymentBrands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ClipboardMonitor.Tests
{
    [TestClass]
    public class MaskingTests
    {
        [TestMethod]
        public void Test_Mask_FullDigits()
        {
            const string input = "4012888888881881";
            var masked = PANHelper.Mask(input);

            Assert.AreEqual("401288******1881", masked);
        }

        [TestMethod]
        public void Test_Mask_WithSpaces()
        {
            const string input = "4012 8888 8888 1881";
            var masked = PANHelper.Mask(input);

            Assert.AreEqual("401288******1881", masked);
        }

        [TestMethod]
        public void Test_Mask_WithDashes()
        {
            const string input = "4012-8888-8888-1881";
            var masked = PANHelper.Mask(input);

            Assert.AreEqual("401288******1881", masked);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_Mask_Null_Throws()
        {
            PANHelper.Mask(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_Mask_Empty_Throws()
        {
            PANHelper.Mask(string.Empty);
        }
    }
}
