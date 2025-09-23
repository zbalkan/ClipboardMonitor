using ClipboardMonitor.AMSI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ClipboardMonitor.Tests
{
    [TestClass]
    public class AmsiTests
    {
        private AmsiContext _context;
        private AmsiSession _session;
        private bool _skip;

        [TestInitialize]
        public void Init()
        {
            // Skip tests in CI environments (GitHub Actions sets CI=true)
            if (Environment.GetEnvironmentVariable("CI") == "true")
            {
                _skip = true;
                return;
            }

            _context = AmsiContext.Create("UnitTests");
            _session = _context.CreateSession();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (_skip)
            {
                return;
            }

            _session.Dispose();
            _context.Dispose();
        }

        [TestMethod]
        public void Test_AMSI_Detect_EICAR()
        {
            if (_skip)
            {
                Assert.Inconclusive("Skipping AMSI test on CI (Defender not available)");
            }

            Assert.IsTrue(_session.IsMalware(
                @"X5O!P%@AP[4\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*",
                "EICAR"));
        }

        [TestMethod]
        public void Test_AMSI_Skip_False_Positive_EICAR()
        {
            if (_skip)
            {
                Assert.Inconclusive("Skipping AMSI test on CI (Defender not available)");
            }

            Assert.IsFalse(_session.IsMalware("0000", "EICAR"));
        }
    }
}
