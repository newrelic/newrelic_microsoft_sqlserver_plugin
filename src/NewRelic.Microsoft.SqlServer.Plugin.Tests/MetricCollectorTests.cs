using NSubstitute;

using NUnit.Framework;

using NewRelic.Microsoft.SqlServer.Plugin.Configuration;
using NewRelic.Microsoft.SqlServer.Plugin.QueryTypes;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	[TestFixture]
	public class MetricCollectorTests
	{
		[Test]
		public void Assert_database_names_are_replaced_when_included_databases_with_display_names_are_configured()
		{
			var includedDatabases = new[]
			                        {
				                        new Database {Name = "Foo", DisplayName = "Fantastic",},
				                        new Database {Name = "Bar", DisplayName = "Baracuda",},
				                        new Database {Name = "Baz", DisplayName = "Assassins",},
				                        new Database {Name = "Quux",},
			                        };

			var databaseMetric1 = Substitute.For<IDatabaseMetric>();
			databaseMetric1.DatabaseName = "Foo";

			// Test for case-insensitivity
			var databaseMetric2 = Substitute.For<IDatabaseMetric>();
			databaseMetric2.DatabaseName = "BAZ";

			var databaseMetric3 = Substitute.For<IDatabaseMetric>();
			databaseMetric3.DatabaseName = "Bar";

			var databaseMetric4 = Substitute.For<IDatabaseMetric>();
			databaseMetric4.DatabaseName = "Quux";

			var results = new object[]
			              {
				              databaseMetric1,
				              databaseMetric2,
				              databaseMetric3,
				              databaseMetric4,
			              };

			MetricCollector.ApplyDatabaseDisplayNames(includedDatabases, results);

			Assert.That(databaseMetric1.DatabaseName, Is.EqualTo("Fantastic"));
			Assert.That(databaseMetric2.DatabaseName, Is.EqualTo("Assassins"));
			Assert.That(databaseMetric3.DatabaseName, Is.EqualTo("Baracuda"));
			Assert.That(databaseMetric4.DatabaseName, Is.EqualTo("Quux"));
		}
	}
}
