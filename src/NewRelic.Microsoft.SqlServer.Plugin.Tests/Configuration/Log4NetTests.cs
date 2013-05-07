using NUnit.Framework;

using NewRelic.Microsoft.SqlServer.Plugin.Properties;

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
            var logger = LogManager.Exists(Constants.SqlMonitorLogger);
            Assert.That(logger, Is.Not.Null, "Couldn't Find Logger with name '{0}'", Constants.SqlMonitorLogger);
        }
    }
}
