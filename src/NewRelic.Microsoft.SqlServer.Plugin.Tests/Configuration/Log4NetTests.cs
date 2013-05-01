using NUnit.Framework;
using log4net;

namespace NewRelic.Microsoft.SqlServer.Plugin.Configuration
{
    [TestFixture]
    public class Log4NetTests
    {
        [Test]
        public void AssertCanFindLog4NetConfig()
        {
            Program.SetUpLogConfig();
            ILog logger = LogManager.GetLogger(Constants.SqlMonitorLogger);
            Assert.That(logger, Is.Not.Null);
        }
    }
}