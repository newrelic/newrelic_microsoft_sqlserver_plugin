using System;
using NUnit.Framework;
using NewRelic.Microsoft.SqlServer.Plugin.Core;
using NewRelic.Platform.Binding.DotNET;

namespace NewRelic.Microsoft.SqlServer.Plugin
{
	[TestFixture]
	public class MetricMapperTests
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

		protected class MarkedUpFakeQueryType
		{
			public string IgnoredCompletely { get; set; }

			[Metric(Ignore = true)]
			public DateTime DoublyIgnored { get; set; }

			[Metric(Ignore = false)]
			public object IgnoreAnyways { get; set; }

			[Metric(MetricName = "FancyName")]
			public int MetricNameOverridden { get; set; }

			[Metric(MetricName = "SuperImportant", Ignore = true)]
			public int NiceNameButIgnored { get; set; }

			[Metric(Ignore = true)]
			public int SimplyIgnored { get; set; }

			public long ConventionalProperty { get; set; }
		}

		[Test]
		public void Assert_decimal_is_mapped()
		{
			var fake = new FakeQueryType {Decimal = 12.3m,};
			var componentData = new ComponentData();

			var queryContext = new QueryContext {ComponentData = componentData, Query = new SqlMonitorQuery(fake.GetType(), new QueryAttribute(null, "Fake"), null, "")};

			var metricMapper = new MetricMapper(fake.GetType().GetProperty("Decimal"));

			metricMapper.AddMetric(queryContext, fake);

			const string metricKey = "Fake/Decimal";
			Assert.That(componentData.Metrics.ContainsKey(metricKey), "Expected metric with correct name to be added");
			var condition = componentData.Metrics[metricKey];
			Assert.That(condition, Is.EqualTo(fake.Decimal), "Metric not mapped correctly");
		}

		[Test]
		[TestCase("Comment", TestName = "string")]
		[TestCase("EventTime", TestName = "DateTime")]
		public void Assert_invalid_type_throws(string propertyName)
		{
			var propertyInfo = typeof (FakeQueryType).GetProperty(propertyName);
			Assume.That(propertyInfo, Is.Not.Null, "Expected a property name '{0}' on {1}", propertyName, typeof (FakeQueryType).Name);

			Assert.Throws<ArgumentException>(() => new MetricMapper(propertyInfo));
		}

		[Test]
		[TestCase("Byte", (byte) 27, TestName = "Byte")]
		[TestCase("Short", (short) 32045, TestName = "Short")]
		[TestCase("Integer", 42, TestName = "Integer")]
		[TestCase("Long", 555L, TestName = "Long")]
		public void Assert_numeric_types_are_mapped(string propertyName, object value)
		{
			var fake = new FakeQueryType();
			var propertyInfo = fake.GetType().GetProperty(propertyName);
			propertyInfo.SetValue(fake, value, null);

			var componentData = new ComponentData();

			var queryContext = new QueryContext {ComponentData = componentData, Query = new SqlMonitorQuery(fake.GetType(), new QueryAttribute(null, "Fake"), null, "")};

			var metricMapper = new MetricMapper(propertyInfo);

			metricMapper.AddMetric(queryContext, fake);

			var metricKey = "Fake/" + propertyName;
			Assert.That(componentData.Metrics.ContainsKey(metricKey), "Expected metric with correct name to be added");
			var condition = componentData.Metrics[metricKey];
			Assert.That(condition, Is.EqualTo(value), "Metric not mapped correctly");
		}

		[Test(Description = "Returns the MetricName when the map is created. Otherwise, null.")]
		[TestCase("IgnoredCompletely", Result = null, TestName = "Assert non-numeric is still ignored without an attribute")]
		[TestCase("ConventionalProperty", Result = "ConventionalProperty", TestName = "Assert numeric is still discovered without an attribute")]
		[TestCase("DoublyIgnored", Result = null, TestName = "Assert non-numeric is ignored with an attribute where Ignore == true")]
		[TestCase("IgnoreAnyways", Result = null, TestName = "Assert non-numeric is ignored despite an attribute where Ignore == false")]
		[TestCase("MetricNameOverridden", Result = "FancyName", TestName = "Assert numeric with attribute has MetricName overriden")]
		[TestCase("NiceNameButIgnored", Result = null, TestName = "Assert numeric is ignored with attribute where MetricName is set and Ignore == true")]
		[TestCase("SimplyIgnored", Result = null, TestName = "Assert numeric is ignored with attribute where Ignore == true")]
		public string Assert_that_metric_attribute_is_respected(string propertyName)
		{
			var fakeType = typeof (MarkedUpFakeQueryType);
			var metricMapper = MetricMapper.TryCreate(fakeType.GetProperty(propertyName));
			return metricMapper != null ? metricMapper.MetricName : null;
		}

		[Test]
		[TestCase("Comment", TestName = "string")]
		[TestCase("EventTime", TestName = "DateTime")]
		public void Assert_try_create_mapper_gracefully_refuses_invalid_types(string propertyName)
		{
			var propertyInfo = typeof (FakeQueryType).GetProperty(propertyName);
			Assume.That(propertyInfo, Is.Not.Null, "Expected a property name '{0}' on {1}", propertyName, typeof (FakeQueryType).Name);

			var mapper = MetricMapper.TryCreate(propertyInfo);
			Assert.That(mapper, Is.Null, "Should not have mapped {0}", propertyName);
		}

		[Test]
		public void Assert_try_create_mapper_handles_decimal_correctly()
		{
			var fake = new FakeQueryType {Decimal = 12.3m};

			var componentData = new ComponentData();

			var queryContext = new QueryContext {ComponentData = componentData, Query = new SqlMonitorQuery(fake.GetType(), new QueryAttribute(null, "Fake"), null, "")};

			var mapper = MetricMapper.TryCreate(fake.GetType().GetProperty("Decimal"));
			Assert.That(mapper, Is.Not.Null, "Mapping Decimal failed");

			mapper.AddMetric(queryContext, fake);

			const string metricKey = "Fake/Decimal";
			Assert.That(componentData.Metrics.ContainsKey(metricKey), "Expected metric with correct name to be added");

			var condition = componentData.Metrics[metricKey];
			Assert.That(condition, Is.EqualTo(fake.Decimal), "Metric not mapped correctly");
		}

		[Test]
		[TestCase("Byte", (byte) 27, TestName = "Byte")]
		[TestCase("Short", (short) 32045, TestName = "Short")]
		[TestCase("Integer", 42, TestName = "Integer")]
		[TestCase("Long", 555L, TestName = "Long")]
		public void Assert_try_create_mapper_handles_numerics_correctly(string propertyName, object value)
		{
			var fake = new FakeQueryType();
			var propertyInfo = fake.GetType().GetProperty(propertyName);
			propertyInfo.SetValue(fake, value, null);

			var componentData = new ComponentData();

			var queryContext = new QueryContext { ComponentData = componentData, Query = new SqlMonitorQuery(fake.GetType(), new QueryAttribute(null, "Fake"), null, "") };

			var mapper = MetricMapper.TryCreate(propertyInfo);
			Assert.That(mapper, Is.Not.Null, "Mapping {0} failed", propertyInfo.PropertyType.Name);

			mapper.AddMetric(queryContext, fake);

			var metricKey = "Fake/" + propertyName;
			Assert.That(componentData.Metrics.ContainsKey(metricKey), "Expected metric with correct name to be added");

			var condition = componentData.Metrics[metricKey];
			Assert.That(condition, Is.EqualTo(value), "Metric not mapped correctly");
		}
	}
}
