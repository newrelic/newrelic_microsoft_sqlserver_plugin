using System.Linq;
using NSubstitute;
using NUnit.Framework;
using NewRelic.Microsoft.SqlServer.Plugin.Core;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	[TestFixture]
	public class QueryLocatorTests
	{
		[Test]
		public void Assert_funcs_are_correctly_configured()
		{
			var dapperWrapper = Substitute.For<IDapperWrapper>();

			var queries = new QueryLocator(dapperWrapper).PrepareQueries();
			foreach (var query in queries)
			{
				var results = query(null);
				Assert.That(results, Is.EqualTo(new object[0]));
			}
		}

		[Test]
		public void Assert_some_query_types_are_found()
		{
			var assembly = typeof (QueryLocator).Assembly;

			var types = assembly.GetTypes();
			Assume.That(types, Is.Not.Empty, "Expected at least one type in the Plugin assembly");

			var typesWithAttribute = types.Where(t => t.GetCustomAttributes(typeof (SqlMonitorQueryAttribute), false).Any());
			Assert.That(typesWithAttribute, Is.Not.Empty, "Expected at least one QueryType using the " + typeof (SqlMonitorQueryAttribute).Name);
		}

		[Test]
		public void Assert_that_queries_are_located()
		{
			var queries = new QueryLocator(new DapperWrapper()).PrepareQueries();
			Assert.That(queries, Is.Not.Empty, "Expected some queries to be returned");
		}
	}
}
