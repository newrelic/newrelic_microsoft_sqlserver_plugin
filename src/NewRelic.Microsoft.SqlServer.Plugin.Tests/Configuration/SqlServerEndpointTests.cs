using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using NSubstitute;

using NUnit.Framework;

using NewRelic.Microsoft.SqlServer.Plugin.Core;
using NewRelic.Microsoft.SqlServer.Plugin.Core.Extensions;
using NewRelic.Microsoft.SqlServer.Plugin.Properties;
using NewRelic.Microsoft.SqlServer.Plugin.QueryTypes;

namespace NewRelic.Microsoft.SqlServer.Plugin.Configuration
{
	[TestFixture]
	public class SqlServerEndpointTests
	{
		public IEnumerable<TestCaseData> QueryHistoryTestData
		{
			get
			{
				return new[]
				       {
					       new TestCaseData((object) new[] {new[] {"QueryOne", "QueryTwo", "QueryThree"}})
						       .Returns(new[]
						                {
							                "QueryOne:1",
							                "QueryTwo:1",
							                "QueryThree:1"
						                }.ToArray()).SetName("Simple History"),
					       new TestCaseData((object) new[] {new[] {"QueryOne", "QueryOne", "QueryTwo", "QueryTwo", "QueryTwo", "QueryThree", "QueryThree", "QueryThree", "QueryThree"}})
						       .Returns(new[]
						                {
							                "QueryOne:2",
							                "QueryTwo:2",
							                "QueryThree:2",
						                }.ToArray()).SetName("Limit to 2 single pass"),
					       new TestCaseData((object) new[]
					                                 {
						                                 new[]
						                                 {
							                                 "QueryOne", "QueryTwo", "QueryThree"
						                                 },
						                                 new[]
						                                 {
							                                 "QueryOne", "QueryTwo", "QueryThree"
						                                 },
						                                 new[]
						                                 {
							                                 "QueryOne", "QueryTwo", "QueryThree"
						                                 },
						                                 new[]
						                                 {
							                                 "QueryOne", "QueryTwo", "QueryThree"
						                                 },
						                                 new[]
						                                 {
							                                 "QueryOne", "QueryTwo", "QueryThree"
						                                 },
					                                 })
						       .Returns(new[]
						                {
							                "QueryOne:2",
							                "QueryTwo:2",
							                "QueryThree:2",
						                }.ToArray()).SetName("Limit to 2 multi pass"),
				       };
			}
		}

		public IEnumerable<TestCaseData> GenerateComponentDataTestCases
		{
			get
			{
				return new[]
				       {
					       new TestCaseData((object) new[]
					                                 {
						                                 new ComponentDataRetrieverTests.GenerateComponentDataInput
						                                 {
							                                 QueryName = "RegularQuery",
							                                 DataSent = false,
							                                 MetricTransformEnum = MetricTransformEnum.Simple,
							                                 Metrics = new[]
							                                           {
								                                           new KeyValuePair<string, object>("Component/Metric/Foo", 1),
								                                           new KeyValuePair<string, object>("Component/Metric/Bar", 3),
								                                           new KeyValuePair<string, object>("Component/Metric/Baz", 45),
							                                           }.ToDictionary(k => k.Key, k => k.Value),
						                                 },
						                                 new ComponentDataRetrieverTests.GenerateComponentDataInput
						                                 {
							                                 QueryName = "RegularQuery",
							                                 DataSent = true,
							                                 MetricTransformEnum = MetricTransformEnum.Simple,
							                                 Metrics = new[]
							                                           {
								                                           new KeyValuePair<string, object>("Component/Metric/Foo", 6),
								                                           new KeyValuePair<string, object>("Component/Metric/Bar", 5),
								                                           new KeyValuePair<string, object>("Component/Metric/Baz", 89),
							                                           }.ToDictionary(k => k.Key, k => k.Value),
						                                 }
					                                 }).Returns(new[]
					                                            {
						                                            "Component/Metric/Foo:1",
						                                            "Component/Metric/Bar:3",
						                                            "Component/Metric/Baz:45",
					                                            }).SetName("Simple non-delta test"),
					       new TestCaseData((object) new[]
					                                 {
						                                 new ComponentDataRetrieverTests.GenerateComponentDataInput
						                                 {
							                                 QueryName = "RegularQuery",
							                                 DataSent = false,
							                                 MetricTransformEnum = MetricTransformEnum.Delta,
							                                 Metrics = new[]
							                                           {
								                                           new KeyValuePair<string, object>("Component/Metric/Foo", 25),
								                                           new KeyValuePair<string, object>("Component/Metric/Bar", 140),
								                                           new KeyValuePair<string, object>("Component/Metric/Baz", 60),
							                                           }.ToDictionary(k => k.Key, k => k.Value),
						                                 },
						                                 new ComponentDataRetrieverTests.GenerateComponentDataInput
						                                 {
							                                 QueryName = "RegularQuery",
							                                 DataSent = true,
							                                 MetricTransformEnum = MetricTransformEnum.Delta,
							                                 Metrics = new[]
							                                           {
								                                           new KeyValuePair<string, object>("Component/Metric/Foo", 20),
								                                           new KeyValuePair<string, object>("Component/Metric/Bar", 100),
								                                           new KeyValuePair<string, object>("Component/Metric/Baz", 50),
							                                           }.ToDictionary(k => k.Key, k => k.Value),
						                                 }
					                                 }).Returns(new[]
					                                            {
						                                            "Component/Metric/Foo:5",
						                                            "Component/Metric/Bar:40",
						                                            "Component/Metric/Baz:10",
					                                            }).SetName("Delta simple test"),
					       new TestCaseData((object) new[]
					                                 {
						                                 new ComponentDataRetrieverTests.GenerateComponentDataInput
						                                 {
							                                 QueryName = "RegularQuery",
							                                 DataSent = false,
							                                 MetricTransformEnum = MetricTransformEnum.Delta,
							                                 Metrics = new[]
							                                           {
								                                           new KeyValuePair<string, object>("Component/Metric/Foo", 10),
								                                           new KeyValuePair<string, object>("Component/Metric/Bar", 105),
								                                           new KeyValuePair<string, object>("Component/Metric/Baz", 49),
							                                           }.ToDictionary(k => k.Key, k => k.Value),
						                                 },
						                                 new ComponentDataRetrieverTests.GenerateComponentDataInput
						                                 {
							                                 QueryName = "RegularQuery",
							                                 DataSent = true,
							                                 MetricTransformEnum = MetricTransformEnum.Delta,
							                                 Metrics = new[]
							                                           {
								                                           new KeyValuePair<string, object>("Component/Metric/Foo", 20),
								                                           new KeyValuePair<string, object>("Component/Metric/Bar", 100),
								                                           new KeyValuePair<string, object>("Component/Metric/Baz", 50),
							                                           }.ToDictionary(k => k.Key, k => k.Value),
						                                 }
					                                 }).Returns(new[]
					                                            {
						                                            "Component/Metric/Foo:0",
						                                            "Component/Metric/Bar:5",
						                                            "Component/Metric/Baz:0",
					                                            }).SetName("Delta with drop in int metric value should send zero test"),
					       new TestCaseData((object) new[]
					                                 {
						                                 new ComponentDataRetrieverTests.GenerateComponentDataInput
						                                 {
							                                 QueryName = "RegularQuery",
							                                 DataSent = false,
							                                 MetricTransformEnum = MetricTransformEnum.Delta,
							                                 Metrics = new[]
							                                           {
								                                           new KeyValuePair<string, object>("Component/Metric/Foo", 10.0m),
								                                           new KeyValuePair<string, object>("Component/Metric/Bar", 105.0m),
								                                           new KeyValuePair<string, object>("Component/Metric/Baz", 49.0m),
							                                           }.ToDictionary(k => k.Key, k => k.Value),
						                                 },
						                                 new ComponentDataRetrieverTests.GenerateComponentDataInput
						                                 {
							                                 QueryName = "RegularQuery",
							                                 DataSent = true,
							                                 MetricTransformEnum = MetricTransformEnum.Delta,
							                                 Metrics = new[]
							                                           {
								                                           new KeyValuePair<string, object>("Component/Metric/Foo", 20.0m),
								                                           new KeyValuePair<string, object>("Component/Metric/Bar", 100.0m),
								                                           new KeyValuePair<string, object>("Component/Metric/Baz", 50.0m),
							                                           }.ToDictionary(k => k.Key, k => k.Value),
						                                 }
					                                 }).Returns(new[]
					                                            {
						                                            "Component/Metric/Foo:0.0",
						                                            "Component/Metric/Bar:5.0",
						                                            "Component/Metric/Baz:0.0",
					                                            }).SetName("Delta with drop in decimal metric value should send zero test"),
					       new TestCaseData((object) new[]
					                                 {
						                                 new ComponentDataRetrieverTests.GenerateComponentDataInput
						                                 {
							                                 QueryName = "RegularQuery",
							                                 DataSent = false,
							                                 MetricTransformEnum = MetricTransformEnum.Simple,
							                                 Metrics = new[]
							                                           {
								                                           new KeyValuePair<string, object>("Component/Metric/Foo", 1),
								                                           new KeyValuePair<string, object>("Component/Metric/Bar", 3),
								                                           new KeyValuePair<string, object>("Component/Metric/Baz", 45),
							                                           }.ToDictionary(k => k.Key, k => k.Value),
						                                 },
						                                 new ComponentDataRetrieverTests.GenerateComponentDataInput
						                                 {
							                                 QueryName = "WackyQuery",
							                                 DataSent = true,
							                                 MetricTransformEnum = MetricTransformEnum.Simple,
							                                 Metrics = new[]
							                                           {
								                                           new KeyValuePair<string, object>("Component/Metric/Foo", 6),
								                                           new KeyValuePair<string, object>("Component/Metric/Bar", 5),
								                                           new KeyValuePair<string, object>("Component/Metric/Baz", 89),
							                                           }.ToDictionary(k => k.Key, k => k.Value),
						                                 }
					                                 }).Throws(typeof (ArgumentException)).SetName("Assert Different Query Names throws Exception Non-Aggregate Test"),
				       };
			}
		}

		[TestCaseSource("QueryHistoryTestData")]
		public string[] Assert_that_query_history_updated_appropriately(string[][] queryNames)
		{
			var sqlServerToMonitor = new SqlServerEndpoint("Best_DB_Ever", "", false);

			Assert.That(sqlServerToMonitor.QueryHistory.Count, Is.EqualTo(0), "History Should start off empty");

			queryNames.ForEach(queryNamesPass =>
			                   {
				                   IQueryContext[] queryContexts = queryNamesPass.Select(queryName =>
				                                                                         {
					                                                                         var queryContext = Substitute.For<IQueryContext>();
					                                                                         queryContext.QueryName.Returns(queryName);
					                                                                         return queryContext;
				                                                                         }).ToArray();
				                   sqlServerToMonitor.UpdateHistory(queryContexts);
			                   });

			string[] actual = sqlServerToMonitor.QueryHistory.Select(qh => string.Format("{0}:{1}", qh.Key, qh.Value.Count)).ToArray();

			return actual;
		}

		[Test]
		public void Assert_complex_include_system_databases_works()
		{
			var includeDbNames = new[] {"FooDb", "BarDb"};
			IEnumerable<Database> includedDbs = includeDbNames.Select(x => new Database {Name = x,});

			var sqlServerToMonitor = new SqlServerEndpoint("FooServer", ".", true, includedDbs, null);
			Assert.That(sqlServerToMonitor.ExcludedDatabaseNames.Length, Is.EqualTo(0));

			IEnumerable<string> expectedIncludes = Constants.SystemDatabases.ToList().Concat(includeDbNames);
			Assert.That(sqlServerToMonitor.IncludedDatabaseNames, Is.EquivalentTo(expectedIncludes));
		}

		[Test]
		public void Assert_include_exclude_lists_built_appropriately()
		{
			IEnumerable<Database> includedDbs = new[] {"FooDb", "BarDb"}.Select(x => new Database {Name = x,});
			var sqlServerToMonitor = new SqlServerEndpoint("FooServer", ".", false, includedDbs, new[] {"Baz"});
			Assert.That(sqlServerToMonitor.IncludedDatabaseNames, Is.EquivalentTo(new[] {"FooDb", "BarDb"}));
			Assert.That(sqlServerToMonitor.ExcludedDatabaseNames, Is.EquivalentTo(Constants.SystemDatabases.Concat(new[] {"Baz"})));
		}

		[Test]
		public void Assert_include_system_databases_works()
		{
			var sqlServerToMonitor = new SqlServerEndpoint("FooServer", ".", false);
			Assert.That(sqlServerToMonitor.IncludedDatabaseNames.Length, Is.EqualTo(0));
			Assert.That(sqlServerToMonitor.ExcludedDatabaseNames, Is.EquivalentTo(Constants.SystemDatabases));

			sqlServerToMonitor = new SqlServerEndpoint("FooServer", ".", true);
			Assert.That(sqlServerToMonitor.IncludedDatabaseNames.Length, Is.EqualTo(0));
			Assert.That(sqlServerToMonitor.ExcludedDatabaseNames.Length, Is.EqualTo(0));
		}

		[Test]
		public void Assert_that_duration_is_reported_correctly()
		{
			var sqlServerToMonitor = new SqlServerEndpoint("", "", false);
			Assert.That(sqlServerToMonitor.Duration, Is.EqualTo(0), "Expected 0 second Duration immediately after .ctor called");

			Thread.Sleep(1000);
			Assert.That(sqlServerToMonitor.Duration, Is.EqualTo(1), "Expected 1 second Duration after Thread.Sleep(1000)");
		}

		[Test]
		public void Assert_that_max_recompile_summary_ignores_other_metrics()
		{
			object[] metrics = {new SqlCpuUsage()};
			var results = new SqlServerEndpoint(null, null, false).GetMaxRecompileSummaryMetric(metrics);
			Assert.That(results, Is.Null, "Implementation should have ignored bad data.");
		}

		[Test]
		public void Assert_that_max_recompile_summary_ignores_empty_metric_array()
		{
			object[] metrics = {};
			var results = new SqlServerEndpoint(null, null, false).GetMaxRecompileSummaryMetric(metrics);
			Assert.That(results, Is.Null, "Implementation should have ignored empty data.");
		}

		[Test]
		public void Assert_that_max_recompile_summary_is_reported()
		{
			object[] metrics =
			{
				new RecompileSummary { DatabaseName = "A", SingleUseObjects = 100, SingleUsePercent = 0, MultipleUseObjects = 1, },
				new RecompileSummary { DatabaseName = "B", SingleUseObjects = 1, SingleUsePercent = 80, MultipleUseObjects = 1, },
				new RecompileSummary { DatabaseName = "C", SingleUseObjects = 1, SingleUsePercent = 0, MultipleUseObjects = 50, },
			};
			var queryContext = new SqlServerEndpoint(null, null, false).GetMaxRecompileSummaryMetric(metrics);
			var results = queryContext.Results.ToArray();

			Assert.That(queryContext, Is.Not.SameAs(metrics), "Expected a new Array");

			var max = results.OfType<RecompileMaximums>().SingleOrDefault();
			Assert.That(max, Is.Not.Null, "Expected a new metric in the results");
			Assert.That(max.SingleUseObjects, Is.EqualTo(100), "Wrong SingleUseObjects value");
			Assert.That(max.SingleUsePercent, Is.EqualTo(80m), "Wrong SingleUsePercent value");
			Assert.That(max.MultipleUseObjects, Is.EqualTo(50), "Wrong MultipleUseObjects value");
		}
	}
}
