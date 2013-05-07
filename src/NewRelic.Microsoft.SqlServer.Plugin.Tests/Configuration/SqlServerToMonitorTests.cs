using System.Linq;
using System.Threading;

using NUnit.Framework;

using NewRelic.Microsoft.SqlServer.Plugin.Properties;

namespace NewRelic.Microsoft.SqlServer.Plugin.Configuration
{
	[TestFixture]
	public class SqlServerToMonitorTests
	{
		[Test]
		public void AssertIncludeExcludeListsBuiltAppropriately()
		{
			var sqlServerToMonitor = new SqlServerToMonitor("FooServer", ".", false, new[] {"FooDb*", "Bar%db"}, new[] {"Baz"});
			Assert.That(sqlServerToMonitor.IncludedDatabases, Is.EquivalentTo(new[] {"FooDb%", "Bar%db"}));
			Assert.That(sqlServerToMonitor.ExcludedDatabases, Is.EquivalentTo(Constants.SystemDatabases.Concat(new[] {"Baz"})));
		}

		[Test]
		public void AssertIncludeSystemDatabasesWorks()
		{
			var sqlServerToMonitor = new SqlServerToMonitor("FooServer", ".", false);
			Assert.That(sqlServerToMonitor.IncludedDatabases.Length, Is.EqualTo(0));
			Assert.That(sqlServerToMonitor.ExcludedDatabases, Is.EquivalentTo(Constants.SystemDatabases));

			sqlServerToMonitor = new SqlServerToMonitor("FooServer", ".", true);
			Assert.That(sqlServerToMonitor.IncludedDatabases.Length, Is.EqualTo(0));
			Assert.That(sqlServerToMonitor.ExcludedDatabases.Length, Is.EqualTo(0));
		}

		[Test]
		public void Assert_that_duration_is_reported_correctly()
		{
			var sqlServerToMonitor = new SqlServerToMonitor("", "", false);
			Assert.That(sqlServerToMonitor.Duration, Is.EqualTo(0), "Expected 0 second Duration immediately after .ctor called");

			Thread.Sleep(1000);
			Assert.That(sqlServerToMonitor.Duration, Is.EqualTo(1), "Expected 1 second Duration after Thread.Sleep(1000)");
		}
	}
}
