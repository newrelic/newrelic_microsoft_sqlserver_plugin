using System.Collections.Generic;

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

            var sqlServer = new SqlServerEndpoint("foo", "foo", true, null, new[] {"blah"});

            var queryLocator = new QueryLocator(null);
            IEnumerable<SqlQuery> queries = queryLocator.PrepareQueries(new[] {queryType.GetType()}, false);
            foreach (SqlQuery query in queries)
            {
                string actual = queryType.ParameterizeQuery(query.CommandText, sqlServer);
                Assert.That(actual, Is.StringContaining("AND (d.Name NOT IN ('blah')"));
            }
        }

        [Test]
        public void Should_replace_where_with_included_and_system_databases()
        {
            var queryType = new SqlServerConnections();

            var sqlServer = new SqlServerEndpoint("foo", "foo", true, new[] {new Database {Name = "bar"},}, null);

            var queryLocator = new QueryLocator(null);
            IEnumerable<SqlQuery> queries = queryLocator.PrepareQueries(new[] {queryType.GetType()}, false);
            foreach (SqlQuery query in queries)
            {
                string actual = queryType.ParameterizeQuery(query.CommandText, sqlServer);
                Assert.That(actual, Is.StringContaining("AND (d.Name IN ('bar', 'tempdb', 'master', 'model', 'msdb')"));
            }
        }

        [Test]
        public void Should_replace_where_with_included_databases()
        {
            var queryType = new SqlServerConnections();

            var sqlServer = new SqlServerEndpoint("foo", "foo", false, new[] {new Database {Name = "bar"},}, null);

            var queryLocator = new QueryLocator(null);
            IEnumerable<SqlQuery> queries = queryLocator.PrepareQueries(new[] {queryType.GetType()}, false);
            foreach (SqlQuery query in queries)
            {
                string actual = queryType.ParameterizeQuery(query.CommandText, sqlServer);
                Assert.That(actual, Is.StringContaining("AND (d.Name IN ('bar')"));
            }
        }
    }
}
