using System.Collections.Generic;

using NUnit.Framework;

using NewRelic.Microsoft.SqlServer.Plugin.Configuration;

namespace NewRelic.Microsoft.SqlServer.Plugin.QueryTypes
{
	public abstract class DatabaseMetricQueryTestFixtureBase<TQuery>
		where TQuery : class, IDatabaseMetric, new()
	{
		protected abstract string IncludedDatabaseExpectedSql { get; }
		protected abstract string ExcludedDatabaseExpectedSql { get; }

		[Test]
		public void Should_replace_where_with_excluded_databases()
		{
			var queryType = new TQuery();

			var sqlServer = new SqlServerEndpoint("foo", "foo", true, null, new[] {"exclude"});

			var queryLocator = new QueryLocator(null);
			IEnumerable<SqlQuery> queries = sqlServer.FilterQueries(queryLocator.PrepareQueries(new[] {queryType.GetType()}, false));
			foreach (SqlQuery query in queries)
			{
				string actual = queryType.ParameterizeQuery(query.CommandText, sqlServer);
				Assert.That(actual, Is.StringContaining(ExcludedDatabaseExpectedSql));
			}
		}

		[Test]
		public void Should_replace_where_with_included_databases()
		{
			var queryType = new TQuery();

			var sqlServer = new SqlServerEndpoint("foo", "foo", false, new[] {new Database {Name = "include"},}, null);

			var queryLocator = new QueryLocator(null);
			IEnumerable<SqlQuery> queries = sqlServer.FilterQueries(queryLocator.PrepareQueries(new[] {queryType.GetType()}, false));
			foreach (SqlQuery query in queries)
			{
				string actual = queryType.ParameterizeQuery(query.CommandText, sqlServer);
				Assert.That(actual, Is.StringContaining(IncludedDatabaseExpectedSql));
			}
		}
	}
}
