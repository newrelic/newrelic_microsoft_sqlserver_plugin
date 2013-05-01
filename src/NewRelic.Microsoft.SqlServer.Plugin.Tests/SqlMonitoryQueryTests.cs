using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using NewRelic.Microsoft.SqlServer.Plugin.Core;
using NewRelic.Platform.Binding.DotNET;
using log4net;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	[TestFixture]
	public class SqlMonitoryQueryTests
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
			var metricMappers = SqlMonitorQuery.GetMappers(typeof (FakeQueryType));
			Assert.That(metricMappers, Is.Not.Null);

			// Keep these out of order to ensure we don't depend on it
			var expected = new[] {"Long", "Integer", "Short", "Decimal", "Byte"};
			var actual = metricMappers.Select(m => m.MetricName).ToArray();
			Assert.That(actual, Is.EquivalentTo(expected), "Properties discovered and mapped wrong");
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

			var sqlMonitorQuery = new SqlMonitorQuery(typeof(FakeQueryType), new QueryAttribute("foo.sql", "Fake/"), Substitute.For<IDapperWrapper>(), "");

			var componentData = new ComponentData();
			sqlMonitorQuery.AddMetrics(new QueryContext {ComponentData = componentData, Query = sqlMonitorQuery, Results = fakes,});

			var expected = new[] { "Fake/Long", "Fake/Integer", "Fake/Short", "Fake/Decimal", "Fake/Byte" };
			var actual = componentData.Metrics.Keys.ToArray();
			Assert.That(actual, Is.EquivalentTo(expected), "Properties discovered and mapped wrong");
		}
	}
}
