using System;
using System.Linq;

using NewRelic.Microsoft.SqlServer.Plugin.Core;
using NewRelic.Microsoft.SqlServer.Plugin.QueryTypes;
using NewRelic.Platform.Binding.DotNET;

using NSubstitute;

using NUnit.Framework;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	[TestFixture]
	public class SqlMonitorQueryTests
	{
		protected class FakeQueryType
		{
			public long Long { get; set; }
			public int Integer { get; set; }
			public short Short { get; set; }
			public byte Byte { get; set; }
			public decimal Decimal { get; set; }
			public string Comment { get; set; }
			public DateTime EventTime { get; set; }
		}

		[Test]
		public void Assert_only_numerics_are_returned()
		{
			MetricMapper[] metricMappers = MetricQuery.GetMappers(typeof (FakeQueryType));
			Assert.That(metricMappers, Is.Not.Null);

			// Keep these out of order to ensure we don't depend on it
			var expected = new[] {"Long", "Integer", "Short", "Decimal", "Byte"};
			string[] actual = metricMappers.Select(m => m.MetricName).ToArray();
			Assert.That(actual, Is.EquivalentTo(expected), "Properties discovered and mapped wrong");
		}

		[Test]
		[TestCase(MetricTransformEnum.Delta)]
		[TestCase(MetricTransformEnum.Simple)]
		public void Assert_query_sets_appropriate_metric_transform(MetricTransformEnum metricTransformEnum)
		{
			var attribute = new SqlServerQueryAttribute("FileIO.sql", "Foo/Bar") {MetricTransformEnum = metricTransformEnum};
			var sqlQuery = new SqlQuery(typeof (FileIoView), attribute, Substitute.For<IDapperWrapper>(), "");

			Assert.That(sqlQuery.MetricTransformEnum, Is.EqualTo(attribute.MetricTransformEnum), "SqlQuery did not set correct value from attribute for MetricTransformEnum");
		}

		[Test]
		public void Assert_results_are_mapped_into_metrics()
		{
			var fakes = new[]
			            {
				            new FakeQueryType
				            {
					            Long = 42,
					            Integer = 27,
					            Short = 12,
					            Byte = 255,
					            Decimal = 407.54m,
					            Comment = "Utterly worthless... except for logging",
					            EventTime = DateTime.Now,
				            }
			            };

			var sqlQuery = new SqlQuery(typeof (FakeQueryType), new SqlServerQueryAttribute("foo.sql", "Fake/"), Substitute.For<IDapperWrapper>(), "");

			var componentData = new ComponentData();
			sqlQuery.AddMetrics(new QueryContext(sqlQuery) {ComponentData = componentData, Results = fakes,});

			var expected = new[] {"Fake/Long", "Fake/Integer", "Fake/Short", "Fake/Decimal", "Fake/Byte"};
			string[] actual = componentData.Metrics.Keys.ToArray();
			Assert.That(actual, Is.EquivalentTo(expected), "Properties discovered and mapped wrong");
		}
	}
}
