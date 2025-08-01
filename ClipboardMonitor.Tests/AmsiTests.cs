using ClipboardMonitor.AMSI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClipboardMonitor.Tests
{
    [TestClass]
    public class AmsiTests
    {
        private AmsiContext _context;
        private AmsiSession _session;

        [TestInitialize]
        public void Init()
        {
            _context = AmsiContext.Create("UnitTests");
            _session = _context.CreateSession();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _session.Dispose();
            _context.Dispose();
        }

        [TestMethod]
        public void Test_AMSI_Detect_EICAR()
        {
            Assert.IsTrue(_session.IsMalware(@"X5O!P%@AP[4\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*", "EICAR"));
        }

        [TestMethod]
        public void Test_AMSI_Skip_False_Positive_EICAR()
        {
            Assert.IsFalse(_session.IsMalware("0000", "EICAR"));
        }
    }
}