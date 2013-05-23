using NUnit.Framework;

using NewRelic.Microsoft.SqlServer.Plugin.Configuration;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	[TestFixture]
	public class ConnectionsTests
	{
		[Test]
		public void Should_replace_where_with_excluded_databases()
		{
			var queryType = new SqlServerConnections();

			var sqlServer = new SqlServer("foo", "foo", true, null, new[] {"blah"});

			var queryLocator = new QueryLocator(null);
			var queries = queryLocator.PrepareQueries(new[] {queryType.GetType()}, false);
			foreach (var query in queries)
			{
				var actual = queryType.ParameterizeQuery(query.CommandText, sqlServer);
				Assert.That(actual, Is.StringContaining("AND (d.Name NOT IN ('blah')"));
			}
		}

		[Test]
		public void Should_replace_where_with_included_databases()
		{
			var queryType = new SqlServerConnections();

			var sqlServer = new SqlServer("foo", "foo", true, new[] {new Database {Name = "bar"},}, null);

			var queryLocator = new QueryLocator(null);
			var queries = queryLocator.PrepareQueries(new[] {queryType.GetType()}, false);
			foreach (var query in queries)
			{
				var actual = queryType.ParameterizeQuery(query.CommandText, sqlServer);
				Assert.That(actual, Is.StringContaining("AND (d.Name IN ('bar')"));
			}
		}
	}
}
