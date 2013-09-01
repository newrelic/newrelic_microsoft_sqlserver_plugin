using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NSubstitute;

using NUnit.Framework;

using NewRelic.Microsoft.SqlServer.Plugin.Configuration;
using NewRelic.Microsoft.SqlServer.Plugin.Properties;
using NewRelic.Microsoft.SqlServer.Plugin.QueryTypes;

using log4net;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	[TestFixture]
	public class SqlEndpointTests
	{
		public IEnumerable<TestCaseData> ComponentGuidTestCases
		{
			get
			{
				yield return new TestCaseData(new SqlServerEndpoint("FooServer", ".", false)).Returns(Constants.SqlServerComponentGuid).SetName("SqlServer Sets Appropriate Guid");
				yield return new TestCaseData(new AzureSqlEndpoint("FooServer", "")).Returns(Constants.SqlAzureComponentGuid).SetName("AzureSqlEndpoint Sets Appropriate Guid");
			}
		}

		public void Assert_endpoint_appropriately_massages_duplicated_data()
		{
			var endpoint = Substitute.For<SqlEndpointBase>("", "");

			var resultSet1 = new object[]
			                 {
				                 new SqlDmlActivity
				                 {
					                 PlanHandle = Encoding.UTF8.GetBytes("AA11"),
					                 SqlStatementHash = Encoding.UTF8.GetBytes("INSERT INTO FOO"),
					                 ExecutionCount = 10,
					                 QueryType = "Writes",
				                 },
				                 new SqlDmlActivity
				                 {
					                 PlanHandle = Encoding.UTF8.GetBytes("AA11"),
					                 SqlStatementHash = Encoding.UTF8.GetBytes("INSERT INTO BAR"),
					                 ExecutionCount = 8,
					                 QueryType = "Writes",
				                 },
				                 new SqlDmlActivity
				                 {
					                 PlanHandle = Encoding.UTF8.GetBytes("AA11"),
					                 SqlStatementHash = Encoding.UTF8.GetBytes("INSERT INTO BAR"),
					                 ExecutionCount = 8,
					                 QueryType = "Writes",
				                 },
				                 new SqlDmlActivity
				                 {
					                 PlanHandle = Encoding.UTF8.GetBytes("BB12"),
					                 SqlStatementHash = Encoding.UTF8.GetBytes("SELECT * FROM FOO"),
					                 ExecutionCount = 500,
					                 QueryType = "Reads",
				                 },
				                 new SqlDmlActivity
				                 {
					                 PlanHandle = Encoding.UTF8.GetBytes("CC12"),
					                 SqlStatementHash = Encoding.UTF8.GetBytes("SELECT * FROM FOO"),
					                 ExecutionCount = 600,
					                 QueryType = "Reads",
				                 },
				                 new SqlDmlActivity
				                 {
					                 PlanHandle = Encoding.UTF8.GetBytes("EE12"),
					                 SqlStatementHash = Encoding.UTF8.GetBytes("SELECT * FROM BAR"),
					                 ExecutionCount = 100,
					                 QueryType = "Reads",
				                 },
			                 };

			IEnumerable<SqlDmlActivity> outputResults1 = endpoint.CalculateSqlDmlActivityIncrease(resultSet1, Substitute.For<ILog>()).Cast<SqlDmlActivity>().ToArray();

			Assert.That(outputResults1, Is.Not.Null);
			Assert.That(outputResults1.Count(), Is.EqualTo(1));

			SqlDmlActivity sqlDmlActivity = outputResults1.First();
			Assert.That(string.Format("Reads:{0} Writes:{1}", sqlDmlActivity.Reads, sqlDmlActivity.Writes), Is.EqualTo("Reads:0 Writes:0"));
		}

		[Test]
		[TestCaseSource("ComponentGuidTestCases")]
		public string Assert_correct_component_guid_supplied_to_query_context(SqlEndpointBase endpoint)
		{
			QueryContext queryContext = endpoint.CreateQueryContext(Substitute.For<ISqlQuery>(), new object[0]);
			return queryContext.ComponentData.Guid;
		}

		[Test]
		public void Assert_endpoint_appropriately_massages_data()
		{
			var endpoint = Substitute.For<SqlEndpointBase>("", "");

			var resultSet1 = new object[]
			                 {
				                 new SqlDmlActivity
				                 {
					                 PlanHandle = Encoding.UTF8.GetBytes("AA11"),
					                 SqlStatementHash = Encoding.UTF8.GetBytes("INSERT INTO FOO"),
					                 ExecutionCount = 10,
					                 QueryType = "Writes",
				                 },
				                 new SqlDmlActivity
				                 {
					                 PlanHandle = Encoding.UTF8.GetBytes("AA11"),
					                 SqlStatementHash = Encoding.UTF8.GetBytes("INSERT INTO BAR"),
					                 ExecutionCount = 8,
					                 QueryType = "Writes",
				                 },
				                 new SqlDmlActivity
				                 {
					                 PlanHandle = Encoding.UTF8.GetBytes("BB12"),
					                 SqlStatementHash = Encoding.UTF8.GetBytes("SELECT * FROM FOO"),
					                 ExecutionCount = 500,
					                 QueryType = "Reads",
				                 },
				                 new SqlDmlActivity
				                 {
					                 PlanHandle = Encoding.UTF8.GetBytes("CC12"),
					                 SqlStatementHash = Encoding.UTF8.GetBytes("SELECT * FROM FOO"),
					                 ExecutionCount = 600,
					                 QueryType = "Reads",
				                 },
				                 new SqlDmlActivity
				                 {
					                 PlanHandle = Encoding.UTF8.GetBytes("EE12"),
					                 SqlStatementHash = Encoding.UTF8.GetBytes("SELECT * FROM BAR"),
					                 ExecutionCount = 100,
					                 QueryType = "Reads",
				                 },
			                 };

			IEnumerable<SqlDmlActivity> outputResults1 = endpoint.CalculateSqlDmlActivityIncrease(resultSet1, Substitute.For<ILog>()).Cast<SqlDmlActivity>().ToArray();

			Assert.That(outputResults1, Is.Not.Null);
			Assert.That(outputResults1.Count(), Is.EqualTo(1));

			SqlDmlActivity sqlDmlActivity = outputResults1.First();
			Assert.That(string.Format("Reads:{0} Writes:{1}", sqlDmlActivity.Reads, sqlDmlActivity.Writes), Is.EqualTo("Reads:0 Writes:0"));

			var resultSet2 = new object[]
			                 {
				                 new SqlDmlActivity
				                 {
					                 PlanHandle = Encoding.UTF8.GetBytes("AA11"),
					                 SqlStatementHash = Encoding.UTF8.GetBytes("INSERT INTO FOO"),
					                 ExecutionCount = 14,
					                 QueryType = "Writes",
				                 },
				                 new SqlDmlActivity
				                 {
					                 PlanHandle = Encoding.UTF8.GetBytes("AA11"),
					                 SqlStatementHash = Encoding.UTF8.GetBytes("INSERT INTO BAR"),
					                 ExecutionCount = 18,
					                 QueryType = "Writes",
				                 },   
				                 //Tests Duplicate Results gets aggregated
				                 new SqlDmlActivity
				                 {
					                 PlanHandle = Encoding.UTF8.GetBytes("AA11"),
					                 SqlStatementHash = Encoding.UTF8.GetBytes("INSERT INTO BAR"),
					                 ExecutionCount = 2,
					                 QueryType = "Writes",
				                 },
				                 new SqlDmlActivity
				                 {
					                 PlanHandle = Encoding.UTF8.GetBytes("BB12"),
					                 SqlStatementHash = Encoding.UTF8.GetBytes("SELECT * FROM FOO"),
					                 ExecutionCount = 550,
					                 QueryType = "Reads",
				                 },
				                 new SqlDmlActivity
				                 {
					                 PlanHandle = Encoding.UTF8.GetBytes("CC12"),
					                 SqlStatementHash = Encoding.UTF8.GetBytes("SELECT * FROM FOO"),
					                 ExecutionCount = 625,
					                 QueryType = "Reads",
				                 },
				                 new SqlDmlActivity
				                 {
					                 PlanHandle = Encoding.UTF8.GetBytes("DD12"),
					                 SqlStatementHash = Encoding.UTF8.GetBytes("SELECT * FROM BAR"),
					                 ExecutionCount = 1,
					                 QueryType = "Reads",
				                 },
			                 };

			SqlDmlActivity[] outputResults2 = endpoint.CalculateSqlDmlActivityIncrease(resultSet2, Substitute.For<ILog>()).Cast<SqlDmlActivity>().ToArray();
			Assert.That(outputResults2, Is.Not.Null);
			Assert.That(outputResults2.Count(), Is.EqualTo(1));

			sqlDmlActivity = outputResults2.First();

			Assert.That(string.Format("Reads:{0} Writes:{1}", sqlDmlActivity.Reads, sqlDmlActivity.Writes), Is.EqualTo("Reads:76 Writes:16"));
		}

		[Test]
		public void Assert_sql_dml_actvity_data_takes_create_time_ms_into_account()
		{
			var endpoint = Substitute.For<SqlEndpointBase>("", "");
			var d1 = new DateTime(2013, 06, 20, 8, 28, 10, 100);
			var d2 = new DateTime(2013, 06, 20, 8, 28, 10, 200);

			var resultSet1 = new object[]
			                 {
				                 new SqlDmlActivity
				                 {
					                 PlanHandle = Encoding.UTF8.GetBytes("AA11"),
					                 SqlStatementHash = Encoding.UTF8.GetBytes("INSERT INTO FOO"),
					                 CreationTime = d1,
					                 ExecutionCount = 10,
					                 QueryType = "Writes",
				                 },
				                 new SqlDmlActivity
				                 {
					                 PlanHandle = Encoding.UTF8.GetBytes("AA11"),
					                 SqlStatementHash = Encoding.UTF8.GetBytes("INSERT INTO FOO"),
					                 CreationTime = d2,
					                 ExecutionCount = 12,
					                 QueryType = "Writes",
				                 },
			                 };

			IEnumerable<SqlDmlActivity> outputResults1 = endpoint.CalculateSqlDmlActivityIncrease(resultSet1, Substitute.For<ILog>()).Cast<SqlDmlActivity>().ToArray();

			Assert.That(outputResults1, Is.Not.Null);
			Assert.That(outputResults1.Count(), Is.EqualTo(1));

			SqlDmlActivity sqlDmlActivity = outputResults1.First();
			Assert.That(string.Format("Reads:{0} Writes:{1}", sqlDmlActivity.Reads, sqlDmlActivity.Writes), Is.EqualTo("Reads:0 Writes:0"));

			var resultSet2 = new object[]
			                 {
				                 new SqlDmlActivity
				                 {
					                 PlanHandle = Encoding.UTF8.GetBytes("AA11"),
					                 SqlStatementHash = Encoding.UTF8.GetBytes("INSERT INTO FOO"),
					                 CreationTime = d2,
					                 ExecutionCount = 15,
					                 QueryType = "Writes",
				                 },
			                 };

			SqlDmlActivity[] outputResults2 = endpoint.CalculateSqlDmlActivityIncrease(resultSet2, Substitute.For<ILog>()).Cast<SqlDmlActivity>().ToArray();
			Assert.That(outputResults2, Is.Not.Null);
			Assert.That(outputResults2.Count(), Is.EqualTo(1));

			sqlDmlActivity = outputResults2.First();

			Assert.That(string.Format("Reads:{0} Writes:{1}", sqlDmlActivity.Reads, sqlDmlActivity.Writes), Is.EqualTo("Reads:0 Writes:3"));
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

			SqlServerEndpoint.ApplyDatabaseDisplayNames(includedDatabases, results);

			Assert.That(databaseMetric1.DatabaseName, Is.EqualTo("Fantastic"));
			Assert.That(databaseMetric2.DatabaseName, Is.EqualTo("Assassins"));
			Assert.That(databaseMetric3.DatabaseName, Is.EqualTo("Baracuda"));
			Assert.That(databaseMetric4.DatabaseName, Is.EqualTo("Quux"));
		}
	}
}
