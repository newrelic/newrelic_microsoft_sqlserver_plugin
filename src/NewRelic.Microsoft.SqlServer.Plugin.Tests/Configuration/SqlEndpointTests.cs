using System.Collections.Generic;
using System.Linq;
using System.Text;

using NSubstitute;

using NUnit.Framework;

using NewRelic.Microsoft.SqlServer.Plugin.Properties;
using NewRelic.Microsoft.SqlServer.Plugin.QueryTypes;

using log4net;

namespace NewRelic.Microsoft.SqlServer.Plugin.Configuration
{
	[TestFixture]
	public class SqlEndpointTests
	{
		public IEnumerable<TestCaseData> ComponentGuidTestCases
		{
			get
			{
				return new[]
				       {
					       new TestCaseData(new SqlServer("FooServer", ".", false)).Returns(Constants.SqlServerComponentGuid).SetName("SqlServer Sets Appropriate Guid"),
					       new TestCaseData(new AzureSqlDatabase("FooServer", ".", Substitute.For<ILog>())).Returns(Constants.SqlAzureComponentGuid).SetName("AzureSqlDatabase Sets Appropriate Guid")
				       };
			}
		}

		public IEnumerable<TestCaseData> EndpointMassagesDmlActivityDataTest
		{
			get
			{
				return new[]
				       {
					       new TestCaseData(new SqlServer("FooServer", ".", false)).SetName("Assert SqlServer Endpoint Massages data appropriately"),
					       new TestCaseData(new AzureSqlDatabase("FooServer", ".", Substitute.For<ILog>())).SetName("Assert AzureEndpoint Massages data appropriately")
				       };
			}
		}

		[Test]
		[TestCaseSource("ComponentGuidTestCases")]
		public string AssertCorrectComponentGuidSuppliedToQueryContext(SqlEndpoint endpoint)
		{
			QueryContext queryContext = endpoint.CreateQueryContext(Substitute.For<ISqlQuery>(), new object[0]);

			return queryContext.ComponentData.Guid;
		}

		[Test]
		[TestCaseSource("EndpointMassagesDmlActivityDataTest")]
		public void AssertEndpointAppropriatelyMassagesData(SqlEndpoint endpoint)
		{
			SqlDmlActivity[] resultSet1 = new[]
			                              {
				                              new SqlDmlActivity
				                              {
					                              SqlHandle = Encoding.UTF8.GetBytes("AA11"),
					                              QueryHash = Encoding.UTF8.GetBytes("ABCD"),
					                              ExecutionCount = 10,
					                              QueryType = "Writes",
				                              },
				                              new SqlDmlActivity
				                              {
					                              SqlHandle = Encoding.UTF8.GetBytes("AA11"),
					                              QueryHash = Encoding.UTF8.GetBytes("ABCE"),
					                              ExecutionCount = 8,
					                              QueryType = "Writes",
				                              },
				                              new SqlDmlActivity
				                              {
					                              SqlHandle = Encoding.UTF8.GetBytes("BB12"),
					                              QueryHash = Encoding.UTF8.GetBytes("FGHI"),
					                              ExecutionCount = 500,
					                              QueryType = "Reads",
				                              },
				                              new SqlDmlActivity
				                              {
					                              SqlHandle = Encoding.UTF8.GetBytes("CC12"),
					                              QueryHash = Encoding.UTF8.GetBytes("FGHI"),
					                              ExecutionCount = 600,
					                              QueryType = "Reads",
				                              },
				                              new SqlDmlActivity
				                              {
					                              SqlHandle = Encoding.UTF8.GetBytes("EE12"),
					                              QueryHash = Encoding.UTF8.GetBytes("FGHI"),
					                              ExecutionCount = 100,
					                              QueryType = "Reads",
				                              },
			                              }.ToArray();

			var sqlQuery = Substitute.For<ISqlQuery>();
			sqlQuery.QueryType.Returns(typeof (SqlDmlActivity));

			IEnumerable<SqlDmlActivity> outputResults1 = endpoint.OnQueryExecuted(sqlQuery, resultSet1, Substitute.For<ILog>()).OfType<SqlDmlActivity>();

			Assert.That(outputResults1, Is.Not.Null);
			Assert.That(outputResults1.Count(), Is.EqualTo(1));

			SqlDmlActivity sqlDmlActivity = outputResults1.First();
			Assert.That(string.Format("Reads:{0} Writes:{1}", sqlDmlActivity.Reads, sqlDmlActivity.Writes), Is.EquivalentTo("Writes:0 Reads:0"));

			SqlDmlActivity[] resultSet2 = new[]
			                              {
				                              new SqlDmlActivity
				                              {
					                              SqlHandle = Encoding.UTF8.GetBytes("AA11"),
					                              QueryHash = Encoding.UTF8.GetBytes("ABCD"),
					                              ExecutionCount = 14,
					                              QueryType = "Writes",
				                              },
				                              new SqlDmlActivity
				                              {
					                              SqlHandle = Encoding.UTF8.GetBytes("AA11"),
					                              QueryHash = Encoding.UTF8.GetBytes("ABCE"),
					                              ExecutionCount = 18,
					                              QueryType = "Writes",
				                              },
				                              new SqlDmlActivity
				                              {
					                              SqlHandle = Encoding.UTF8.GetBytes("BB12"),
					                              QueryHash = Encoding.UTF8.GetBytes("FGHI"),
					                              ExecutionCount = 550,
					                              QueryType = "Reads",
				                              },
				                              new SqlDmlActivity
				                              {
					                              SqlHandle = Encoding.UTF8.GetBytes("CC12"),
					                              QueryHash = Encoding.UTF8.GetBytes("FGHI"),
					                              ExecutionCount = 625,
					                              QueryType = "Reads",
				                              },
				                              new SqlDmlActivity
				                              {
					                              SqlHandle = Encoding.UTF8.GetBytes("DD12"),
					                              QueryHash = Encoding.UTF8.GetBytes("FGHI"),
					                              ExecutionCount = 125,
					                              QueryType = "Reads",
				                              },
			                              }.ToArray();

			IEnumerable<SqlDmlActivity> outputResults2 = endpoint.OnQueryExecuted(sqlQuery, resultSet2, Substitute.For<ILog>()).OfType<SqlDmlActivity>();
			Assert.That(outputResults2, Is.Not.Null);
			Assert.That(outputResults2.Count(), Is.EqualTo(1));

			sqlDmlActivity = outputResults2.First();

			Assert.That(string.Format("Reads:{0} Writes:{1}", sqlDmlActivity.Reads, sqlDmlActivity.Writes), Is.EquivalentTo("Writes:14 Reads:75"));
		}

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

			SqlServer.ApplyDatabaseDisplayNames(includedDatabases, results);

			Assert.That(databaseMetric1.DatabaseName, Is.EqualTo("Fantastic"));
			Assert.That(databaseMetric2.DatabaseName, Is.EqualTo("Assassins"));
			Assert.That(databaseMetric3.DatabaseName, Is.EqualTo("Baracuda"));
			Assert.That(databaseMetric4.DatabaseName, Is.EqualTo("Quux"));
		}
	}
}
