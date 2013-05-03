using System.Linq;

using NUnit.Framework;

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
	}
}
